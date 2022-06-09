using System;
using System.Linq;
using System.Collections.Generic;
using Function.Domain.Models.OL;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Function.Domain.Helpers.Parser
{
    /// <summary>
    /// Validates that OpenLineage events are valid for the parser.
    /// </summary>
    public class ValidateOlEvent : IValidateOlEvent
    {
        private ILogger _log;
        private Event _event = new Event();

        /// <summary>
        /// Validate OlEvent Constructor
        /// </summary>
        /// <param name="loggerFactory">DI logger for the function library</param>
        /// <param name="olEvent">OpenLineage Event message</param>
        /// <exception cref="ArgumentNullException">Thrown if olEvent parameter is null</exception>
        public ValidateOlEvent(ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger<ValidateOlEvent>();
        }

        /// <summary>
        /// Performs initial validation of OpenLineage input
        /// The tested criteria include:
        /// 1. Events have both inputs and outputs
        /// 2. Events do not have the same input and output
        /// 3. EventType is START or COMPLETE
        /// 4. If EventType is START, there is a Environment Facet
        /// </summary>
        /// <param name="olEvent">OpenLineage Event message</param>
        /// <returns>true if input is valid, false if not</returns>
        public bool Validate(Event olEvent){
            if (olEvent.Inputs.Count > 0 && olEvent.Outputs.Count > 0)
            {
                // Need to rework for multiple inputs and outputs in one packet - possibly combine and then hash
                if (InOutEqual(olEvent))
                { 
                    return false; 
                }
                if (olEvent.EventType == "START")
                {
                    if (olEvent.Run.Facets.EnvironmentProperties == null)
                    {
                        return false;
                    }
                    return true;
                }
                else if (olEvent.EventType == "COMPLETE")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private bool InOutEqual(Event ev)
        {
            List<string> nms = ev.Inputs.Select(m => m.Name.TrimEnd('/').ToLower()).ToList();
            List<string> nms2 = ev.Outputs.Select(m => m.Name.TrimEnd('/').ToLower()).ToList();
            List<string> nmspc = ev.Inputs.Select(m => m.NameSpace.TrimEnd('/').ToLower()).ToList();
            List<string> nmspc2 = ev.Outputs.Select(m => m.NameSpace.TrimEnd('/').ToLower()).ToList();
            nms.Sort();
            nms2.Sort();
            nmspc.Sort();
            nmspc2.Sort();
            return Enumerable.SequenceEqual(nms, nms2) && Enumerable.SequenceEqual(nms, nms2);
        }
    }
}