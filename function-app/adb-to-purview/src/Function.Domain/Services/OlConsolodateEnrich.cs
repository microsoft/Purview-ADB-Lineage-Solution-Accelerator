using System;
using System.Threading.Tasks;
using Function.Domain.Models.OL;
using Function.Domain.Models;
using Newtonsoft.Json;
using Function.Domain.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Function.Domain.Helpers.Parser;

namespace Function.Domain.Services
{
    /// <summary>
    /// Service that consolodates OpenLineage Start messages, containing environment data, with Complete messages.  
    /// Further, this service enriches OpenLineage messages with data from the ADB Jobs API
    /// </summary>
    public class OlConsolodateEnrich : IOlConsolodateEnrich
    {
        private const string START_EVENT_TYPE = "START";
        private const string COMPLETE_EVENT_TYPE = "COMPLETE";
        private readonly ILogger<OlConsolodateEnrich> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        private Event _event = new Event();

        /// <summary>
        /// Constructs the OlConsolodateEnrich object from the Function framework using DI
        /// </summary>
        /// <param name="loggerFactory">Logger Factory to support DI from function framework or code calling helper classes</param>
        /// <param name="configuration">Function framwork config from DI</param>
        public OlConsolodateEnrich(
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<OlConsolodateEnrich>();
            _loggerFactory = loggerFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Consolodates the start and complete events into a single event. This is done because the complete event does not
        /// have the environment facet attached to it.  The event filter should have already been run for OpenLineage to filter
        /// out unwanted events.
        /// </summary>
        /// <param name="strEvent"> The event in json format coming from the filter function </param>
        /// <returns> Enriched event or null if unable to process the event </returns>
        public async Task<EnrichedEvent?> ProcessOlMessage(String strEvent)
        {

            var trimString = TrimPrefix(strEvent);
            try
            {
                _event = JsonConvert.DeserializeObject<Event>(trimString) ?? new Event();
            }
            catch (JsonSerializationException ex) {
                _logger.LogWarning(ex,$"Unrecognized Message: {strEvent}, error: {ex.Message} path: {ex.Path}");
            }

            var validateOlEvent = new ValidateOlEvent(_loggerFactory);
            var olMessageConsolodation = new OlMessageConsolodation(_loggerFactory, _configuration);
            var olEnrichMessage = new OlMessageEnrichment(_loggerFactory, _configuration);

            // Validate the event
            if (!validateOlEvent.Validate(_event))
            {
                return null;
            }

            try{
                // Store the start event 
                if (_event.EventType == START_EVENT_TYPE)
                {
                    if (_event.Run.Facets.EnvironmentProperties != null)
                    {
                        if (!await olMessageConsolodation.CaptureEnvironmentFromStart(_event, _event.Run.RunId, _event.Run.Facets.EnvironmentProperties!))
                        {
                            _logger.LogError("Problem capturing environment from start event");
                            return null;
                        }
                    }
                }
                // consolodate and enrich the complete event if possible
                else if (_event.EventType == COMPLETE_EVENT_TYPE)
                {
                    var consolodatedEvent = await olMessageConsolodation.ConsolodateCompleteEvent(_event, _event.Run.RunId);
                    if (consolodatedEvent == null)
                    {
                        return null;
                    }
                    else
                    {
                        var enrichedEvent = await olEnrichMessage.GetEnrichedEvent(consolodatedEvent);
                        if (enrichedEvent == null)
                        {
                            return null;
                        }
                        return enrichedEvent;
                    }
                }
                else
                {
                    _logger.LogError($"OlMessageConsolodation-ProcessStartEvent: Error {_event.EventType} is not a valid event type");
                    return null;
                }
            }
            // Unknown error in consolodation
            catch (Exception ex){
                _logger.LogError(ex,$"Error consolodating event: {strEvent}, error: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Returns the Namespace value form the OpenLineage event
        /// </summary>
        /// <returns>Namespace value from OpenLineage</returns>
        public string GetJobNamespace()
        {
            return _event.Job.Namespace;
        }

        private Event? ParseOlEvent(string strEvent)
        {
            try{
                var trimString = TrimPrefix(strEvent);
                return JsonConvert.DeserializeObject<Event>(trimString);
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