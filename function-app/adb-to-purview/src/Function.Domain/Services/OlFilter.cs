using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Function.Domain.Models.OL;
using Function.Domain.Helpers.Parser;
using Newtonsoft.Json.Linq;
using Function.Domain.Models.Settings;

namespace Function.Domain.Services
{
    /// <summary>
    /// Service which filters out OpenLineage events that are not supported by the current version of the function.
    /// </summary>
    public class OlFilter : IOlFilter
    {
        private ILogger _logger;
        private ILoggerFactory _loggerFactory;
        private AppConfigurationSettings? _appSettingsConfig = new AppConfigurationSettings();

        /// <summary>
        /// Constructor for the OlFilter Service
        /// </summary>
        /// <param name="loggerFactory">Passing the logger factory with dependency injection</param>
        public OlFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<OlFilter>();
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Filters the OL events to only those that need to be passed on to the event hub
        /// </summary>
        /// <param name="strRequest">This is the OpenLineage data passed from the Spark listener</param>
        /// <returns> 
        /// true: if the message should be passed on for further processing
        /// false: if the message should be filtered out
        /// </returns>
        public bool FilterOlMessage(string strRequest)
        {
            var olEvent = ParseOlEvent(strRequest);
            try {
                if (olEvent is not null)
                {
                    var validateEvent = new ValidateOlEvent(_loggerFactory);
                    return validateEvent.Validate(olEvent);
                }
                return false;
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Error during event validation: {strRequest}, error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Returns the namespace of the job that the OpenLineage event belongs to
        /// </summary>
        /// <param name="strRequest"> the OpenLineage event </param>
        /// <returns> the namespace value from the event </returns>
        public string GetJobNamespace(string strRequest)
        {
            var olEvent = ParseOlEvent(strRequest);
            return olEvent?.Job.Namespace ?? "";
        }

        private Event? ParseOlEvent(string strEvent)
        {
            try{
                var trimString = TrimPrefix(strEvent);
                var _event = JsonConvert.DeserializeObject<Event>(trimString);
                int planSize = System.Text.Encoding.Unicode.GetByteCount(_event!.Run.Facets.SparkLogicalPlan.ToString());
                if (planSize > _appSettingsConfig!.maxQueryPlanSize){
                    _logger.LogWarning("Query Plan size exceeded maximum. Removing query plan from OpenLineage Event");
                    _event.Run.Facets.SparkLogicalPlan = new JObject();
                }
                return _event;
            }
            catch (JsonSerializationException ex) {
                _logger.LogWarning($"Json Serialization Issue: {strEvent}, error: {ex.Message} path: {ex.Path}");
            }
            // Parsing error
            catch (Exception ex){
                _logger.LogWarning($"Unrecognized Message: {strEvent}, error: {ex.Message}");
            }
            return null;
        }
        
        private string TrimPrefix(string strEvent)
        {
            return strEvent.Substring(strEvent.IndexOf('{')).Trim();
        }
    }
}