// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
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
        static Queue<string> _completedRunIds = new Queue<string>();


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
        /// <param name="envParent">the environment facet to store</param>
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
        /// Consolidates OpenLineage COMPLETE events with environment data from corresponding START events
        /// </summary>
        /// <param name="olEvent">The OL COMPLETE event to consolidate</param>
        /// <param name="jobRunId">The JobRunId to look up the corresponding START event environment data</param>
        /// <returns></returns>
        public async Task<Event?> ConsolodateCompleteEvent(Event olEvent, string jobRunId)
        {
            if (!(await InitTable()))
            {
                return null;
            }
            if (RunIdProcessed(jobRunId))
            {
                return null;
            }
            if (await JoinEventData(olEvent, jobRunId))
            {
                return olEvent;
            }
            else {
                return null;
            }   
        }


        // Uses a static queue as a circular buffer to keep track of processed JobRunIds. This is used to prevent duplicate 
        // events from being processed.
        // This code will change once Open Lineage supports differentiating between complete events.
        private bool RunIdProcessed(string runId)
        {
            bool rtrn = _completedRunIds.Contains(runId);
            if (!rtrn)
            {
                _completedRunIds.Enqueue(runId);
            }
            if (_completedRunIds.Count > 10)
            {
                _completedRunIds.Dequeue();
            }
            return rtrn;
        }

        // Uses function storage account to store ol event info which must be
        // combined with other ol event data to make a complete Purview entity
        // returns true if it is a consolidation event
        private async Task<bool> ProcessStartEvent(Event olEvent, string jobRunId, EnvironmentPropsParent envParent)
        {
            if (!IsStartEventEnvironment(olEvent))
            {
                return false;
            }
            try
            {
                if (isDataSourceV2Event(olEvent))
                // Store inputs and env facet.
                {
                    var entity = new TableEntity(TABLE_PARTITION, olEvent.Run.RunId)
                    {
                        { "EnvFacet", JsonConvert.SerializeObject(olEvent.Run.Facets.EnvironmentProperties) },
                        { "Inputs", JsonConvert.SerializeObject(olEvent.Inputs) }

                    };
                    await _tableClient.AddEntityAsync(entity);
                }
                else {
                // Store only env facet.
                    var entity = new TableEntity(TABLE_PARTITION, olEvent.Run.RunId)
                    {
                        { "EnvFacet", JsonConvert.SerializeObject(olEvent.Run.Facets.EnvironmentProperties) }

                    };
                    await _tableClient.AddEntityAsync(entity);
                }
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
            TableEntity te_inputs;

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
                    _log.LogWarning($"Start event was missing, retrying to consolidate message. Retry count: {currentRetry}");
                    if (currentRetry > retryCount)
                    {
                        return false;
                    }
                }
                await Task.Delay(delay);
            }

            // Get inputs. Todo: Check if more efficient to get inputs within the same while loop above. Can we get 2 entities at the same time? 
            currentRetry = 0;
            while (true)
            {
                try
                {
                    te_inputs = await _tableClient.GetEntityAsync<TableEntity>(TABLE_PARTITION, olEvent.Run.RunId, new string[] { "Inputs" });
                    break;
                }
                catch (RequestFailedException)
                {
                    currentRetry++;
                    _log.LogWarning($"Start event was missing, retrying to consolidate message to get inputs. Retry count: {currentRetry}");
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

            // Check if saved any inputs from the START event (will only be done for events containing DataSourceV2 sources)
            if (te_inputs is not null) {
                var saved_inputs = JsonConvert.DeserializeObject<List<Inputs>>(te_inputs["Inputs"].ToString() ?? "");

                if (saved_inputs is null) {
                        _log.LogWarning($"OlMessageConsolodation-JoinEventData: Warning: no inputs found for datasource v2 COMPLETE event");
                        return false;
                }

                // Check inputs saved against inputs captured in this COMPLETE event and combine while removing any duplicates.
                // Checking for duplicates needed since we save all the inputs captured from the START event. Perhaps it may be better to 
                // only save the DataSourceV2 inputs?
                var inputs = new List<Inputs>(saved_inputs.Count + olEvent.Inputs.Count);
                inputs.AddRange(saved_inputs);
                inputs.AddRange(olEvent.Inputs);
                var unique_inputs = inputs.Distinct();
                olEvent.Inputs = unique_inputs.ToList();
            }

            // clean up table over time. 
            try
            {
                var delresp = await _tableClient.DeleteEntityAsync(TABLE_PARTITION, olEvent.Run.RunId);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"OlMessageConsolodation-JoinEventData: Error {ex.Message} when deleting entity");
            }

            // If no inputs were saved from the start event, then we need to make sure we're only processing this COMPLETE event
            // if it has both inputs and outputs (reflects original logic, prior to supporting DataSourceV2 events)
            if (te_inputs is null && !(olEvent.Inputs.Count > 0 && olEvent.Outputs.Count > 0)) {
                return false;
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

        /// <summary>
        /// Helper function to determine if the event is one of
        /// the data source v2 ones which needs us to save the 
        /// inputs from the start event
        /// </summary>
        private bool isDataSourceV2Event(Event olEvent) {
            string[] special_cases = {"azurecosmos://", "iceberg://"}; // todo: make this configurable?
            // // Don't need to process START events here as they have both inputs and outputs
            // if (olEvent.EventType == "COMPLETE") return false;

            foreach (var outp in olEvent.Outputs)
            {
                foreach (var source in special_cases)
                {
                    if (outp.NameSpace.StartsWith(source)) return true;
                }   
            }
            return false;
        }

        private bool IsJoinEvent(Event olEvent)
        {
            if (olEvent.EventType == COMPLETE_EVENT)
            {
                if (olEvent.Outputs.Count > 0)
                {
                    return true;
                }
                else
                {
                    _log.LogError("OlMessageConsolodation-IsJoinEvent: No Inputs and or Outputs detected");
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