using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
       public class DatabricksSparkJarTaskAttributes : DatabricksJobTaskAttributes, IDatabricksJobTaskAttributes
    {
        [JsonProperty("mainClassName")]
        public string MainClassName = "";
        [JsonProperty("jarUri")]
        public string JarUri = "";
        [JsonProperty("jar")]
        public string Jar = "";
        [JsonProperty("parameters")]
        public List<string> Parameters = new List<string>(); 
    }

}