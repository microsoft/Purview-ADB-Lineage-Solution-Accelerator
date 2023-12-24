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
                var _totalPayloadSize = System.Text.Encoding.Unicode.GetByteCount(trimString);

                var _event = JsonConvert.DeserializeObject<Event>(trimString);
                if (_event == null){
                    _logger.LogWarning($"ParseOlEvent: Event Payload was null");
                    return null;
                }
                
                // Handle the 1 MB size limit of an Event Hub in an opinionated way
                _logger.LogDebug($"ParseOlEvent: Payload Size Initial: {_totalPayloadSize}");
                if (_totalPayloadSize > _appSettingsConfig!.maxPayloadSize){
                    // First remove the Spark Plan, it has the least impact
                    _logger.LogWarning("Total Payload from OpenLineage exceeded maximum size.");
                    _logger.LogWarning("Total Payload from OpenLineage exceeded maximum size: Removing Spark Plan");
                    _event.Run.Facets.SparkLogicalPlan = new JObject();
                    var _sizeAfterStripPlan = System.Text.Encoding.Unicode.GetByteCount(JsonConvert.SerializeObject(_event).ToString());
                    _logger.LogDebug($"ParseOlEvent: Payload Size After Pruning Spark Plan: {_totalPayloadSize}");

                    if (_sizeAfterStripPlan > _appSettingsConfig!.maxPayloadSize){
                        // Next remove the column lineage but this affects column mapping
                        _logger.LogWarning("Total Payload from OpenLineage exceeded maximum size: Removing column lineage from OpenLineage Event");
                        System.Collections.Generic.List<Outputs> updatedOutputs = new System.Collections.Generic.List<Outputs>();
                        foreach (var output in _event.Outputs){
                            output.Facets.ColFacets = new ColumnLineageFacetsClass();
                            updatedOutputs.Add(output);
                        }
                        _event.Outputs = updatedOutputs;
                    }

                    var _sizeAfterStripPlanAndColLineage = System.Text.Encoding.Unicode.GetByteCount(JsonConvert.SerializeObject(_event).ToString());
                    _logger.LogWarning($"ParseOlEvent - Payload Size After Pruning Spark Plan and ColumnLineage: {_sizeAfterStripPlanAndColLineage}");
                    // TODO: Add reducing Mount Points
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

