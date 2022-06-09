using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
       public class DatabricksPythonTaskAttributes : DatabricksJobTaskAttributes, IDatabricksJobTaskAttributes
    {
        [JsonProperty("pythonFile")]
        public string PythonFile = "";
        [JsonProperty("parameters")]
        public List<string> Parameters = new List<string>(); 
    }
}