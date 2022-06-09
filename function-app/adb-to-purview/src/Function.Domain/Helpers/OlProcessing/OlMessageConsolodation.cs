using System;
using System.Threading.Tasks;
using System.Threading;
using Function.Domain.Models.OL;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace Function.Domain.Helpers
{
    /// <summary>
    /// OpenLineage Start messages contain environment properties, Complete messages do not. This class
    /// matches and consolodates Start messages with Complete messages. 
    /// </summary>
    public class OlMessageConsolodation : IOlMessageConsolodation
    {
        private IConfiguration _config;
        private ILogger _log;
        private TableServiceClient _serviceClient;
        private TableClient _tableClient;


        const string STORAGE = "FunctionStorage";
        const string TABLE = "OlEventConsolodation";

        const string TABLE_PARTITION = "OlEvent_Part";
        const string TABLE_RUN_CLMN = "RunId";
        const string TABLE_ENV_CLMN = "EnvFacet";
        const string COMPLETE_EVENT = "COMPLETE";
        const string START_EVENT = "START";
        private string _storageConStr;

        /// <summary>
        /// Construtor for the OlMessageConsolodation class
        /// </summary>
        /// <param name="loggerFactory">Logger Factory passed down from Function Framework DI</param>
        /// <param name="config">Function configuration passed down from Function Framework DI</param>
        public OlMessageConsolodation(ILoggerFactory loggerFactory, IConfiguration config)
        {
            _config = config;
            _log = loggerFactory.CreateLogger<OlMessageConsolodation>();
            _storageConStr = _config[STORAGE];
            var options = new TableClientOptions();
            options.Retry.MaxRetries = 3;
            _serviceClient = new TableServiceClient(_storageConStr, options);
            _tableClient = _serviceClient.GetTableClient(TABLE);
        }

        private async Task<bool> InitTable()
        {
            try
            {
                await _serviceClient.CreateTableIfNotExistsAsync(TABLE);
            }
            catch (System.Exception ex)
            {
                _log.LogError(ex, $"OlMessageConsolodation-ProcessStartEvent: Error {ex.Message} initializing table storage");
                throw;
            }
            return true;
        }

        /// <summary>
        /// Store OpenLineage START events environment facet in azure table storage indexed by JobRunId
        /// </summary>
        /// <param name="olEvent">The event to store</param>
        /// <param name="jobRunId">The jobRunId of the event</param>
        /// <param name="envParent">the envrionment facet to store</param>
        /// <returns></returns>
        public async Task<bool> CaptureEnvironmentFromStart(Event olEvent, string jobRunId, EnvironmentPropsParent envParent)
        {
            if (!(await InitTable()))
            {
                return(false);
            }
            // if this is a start event with an environment, store environment for 
            // future consolodation
            if (await ProcessStartEvent(olEvent, jobRunId, envParent))
            {
                return (true);
            }
            return false;
        }

        /// <summary>
        /// Consolodates OpenLineage COMPLETE events with envrionment data from corisponding START events
        /// </summary>
        /// <param name="olEvent">The OL COMPLETE event to consolodate</param>
        /// <param name="jobRunId">The JobRunId to look up the corisponding START event envrionment data</param>
        /// <returns></returns>
        public async Task<Event?> ConsolodateCompleteEvent(Event olEvent, string jobRunId)
        {
            if (!(await InitTable()))
            {
                return null;
            }
            if (await JoinEventData(olEvent, jobRunId))
            {
                return olEvent;
            }
            else {
                // Return origional event here. If using the old jar, the env facet would be included.
                return null;
            }   
        }

        // Uses function storage account to store ol event info which must be
        // combined with other ol event data to make a complete Purview entity
        // returns true if it is a consolodation event
        private async Task<bool> ProcessStartEvent(Event olEvent, string jobRunId, EnvironmentPropsParent envParent)
        {
            if (!IsStartEventEnvironment(olEvent))
            {
                return false;
            }
            try
            {
                var entity = new TableEntity(TABLE_PARTITION, olEvent.Run.RunId)
                {
                    { "EnvFacet", JsonConvert.SerializeObject(olEvent.Run.Facets.EnvironmentProperties) }
                };

                await _tableClient.AddEntityAsync(entity);
            }
            catch (RequestFailedException ex)
            {
                _log.LogInformation($"OlMessageConsolodation-ProcessStartEvent: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                _log.LogError(ex, $"OlMessageConsolodation-ProcessStartEvent: Error {ex.Message} when processing entity");
                return false;
            }
            return true;
        }

        private async Task<bool> JoinEventData(Event olEvent, string jobRunId)
        {
            if (!IsJoinEvent(olEvent))
            {
                return false;
            }
            
            TableEntity te;

            // Processing time can sometimes cause complete events 
            int retryCount = 4;
            int currentRetry = 0;
            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (true)
            {
                try
                {
                    te = await _tableClient.GetEntityAsync<TableEntity>(TABLE_PARTITION, olEvent.Run.RunId, new string[] { "EnvFacet" });
                    break;
                }
                catch (RequestFailedException)
                {
                    currentRetry++;
                    _log.LogWarning($"Start event was missing, retrying to consolodate message. Retry count: {currentRetry}");
                    if (currentRetry > retryCount)
                    {
                        return false;
                    }
                }
                await Task.Delay(delay);
            }

            // Add Environment to event
            var envFacet = JsonConvert.DeserializeObject<EnvironmentPropsParent>(te["EnvFacet"].ToString() ?? "");
                if (envFacet is null)
                {
                    _log.LogWarning($"OlMessageConsolodation-JoinEventData: Warning environment facet for COMPLETE event is null");
                    return false;
                }
                olEvent.Run.Facets.EnvironmentProperties = envFacet;

                // clean up table over time
                try
                {
                    var delresp = await _tableClient.DeleteEntityAsync(TABLE_PARTITION, olEvent.Run.RunId);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, $"OlMessageConsolodation-JoinEventData: Error {ex.Message} when deleting entity");
                }

            return true;
        }

        // Returns true if olEvent is of type START and has the environment facet
        private bool IsStartEventEnvironment(Event olEvent)
        {
            if (olEvent.EventType == START_EVENT && olEvent.Run.Facets.EnvironmentProperties != null)
                {
                    return true;
                }

            return false;
        }

        private bool IsJoinEvent(Event olEvent)
        {
            if (olEvent.EventType == COMPLETE_EVENT)
            {
                if (olEvent.Inputs.Count > 0 && olEvent.Outputs.Count > 0)
                {
                    return true;
                }
                else
                {
                    _log.LogError("OlMessageConsolodation-IsJoinEvent: No Inputs and or Ouputs detected");
                    return false;
                }
            }
            else
                {
                    _log.LogWarning($"OlMessageConsolodation-IsJoinEvent: Event type is not \"{COMPLETE_EVENT}\"");
                    return false;
                }
        }
        
    }
}