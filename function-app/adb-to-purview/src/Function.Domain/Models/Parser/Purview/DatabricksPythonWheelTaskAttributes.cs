using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
       public class DatabricksPythonWheelTaskAttributes : DatabricksJobTaskAttributes, IDatabricksJobTaskAttributes
    {
        [JsonProperty("packageName")]
        public string PackageName = "";
        [JsonProperty("entryPoint")]
        public string EntryPoint = "";  
        [JsonProperty("wheel")]
        public string Wheel = ""; 
        [JsonProperty("parameters")]
        public List<string> Parameters = new List<string>(); 
    }

}