// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        /// <param name="olEvent">This is the OpenLineage data passed from the Spark listener</param>
        /// <returns> 
        /// true: if the message should be passed on for further processing
        /// false: if the message should be filtered out
        /// </returns>
        public bool FilterOlMessage(Event olEvent)
        {
            try {
                var validateEvent = new ValidateOlEvent(_loggerFactory);
                return validateEvent.Validate(olEvent);
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Error during event validation: {olEvent.ToString()}, error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Returns the namespace of the job that the OpenLineage event belongs to
        /// </summary>
        /// <param name="strRequest"> the OpenLineage event </param>
        /// <returns> the namespace value from the event </returns>
        public string GetJobNamespace(Event olEvent)
        {
            return olEvent?.Job.Namespace ?? "";
        }
    }
}