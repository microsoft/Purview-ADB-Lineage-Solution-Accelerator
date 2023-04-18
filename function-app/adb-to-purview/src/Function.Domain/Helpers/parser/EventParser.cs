// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Function.Domain.Models.Purview;
using Function.Domain.Models.OL;
using Microsoft.Extensions.Logging;
using Function.Domain.Models.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Function.Domain.Helpers
{
    public class EventParser:IEventParser
    {
        private ILogger _logger;
        private AppConfigurationSettings? _appSettingsConfig = new AppConfigurationSettings();
        public EventParser(ILogger logger){
            _logger = logger;
        }
        public Event? ParseOlEvent(string eventPayload){
            try{
            var trimString = TrimPrefix(eventPayload);
            var _event = JsonConvert.DeserializeObject<Event>(trimString);
            int planSize = System.Text.Encoding.Unicode.GetByteCount(_event!.Run.Facets.SparkLogicalPlan.ToString());
            if (planSize > _appSettingsConfig!.maxQueryPlanSize){
                _logger.LogWarning("Query Plan size exceeded maximum. Removing query plan from OpenLineage Event");
                _event.Run.Facets.SparkLogicalPlan = new JObject();
            }
            return _event;
            }
            catch (JsonSerializationException ex) {
                _logger.LogWarning($"Json Serialization Issue: {eventPayload}, error: {ex.Message} path: {ex.Path}");
            }
            // Parsing error
            catch (Exception ex){
                _logger.LogWarning($"Unrecognized Message: {eventPayload}, error: {ex.Message}");
            }
            return null;

        }
        public string TrimPrefix(string strEvent){
            return strEvent.Substring(strEvent.IndexOf('{')).Trim();
        }
    }
}

