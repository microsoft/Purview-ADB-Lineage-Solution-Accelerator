using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
       public class DatabricksNotebookAttributes
    {
        [JsonProperty("name")]
        public string Name = "";
        [JsonProperty("qualifiedName")]
        public string QualifiedName = ""; 
        [JsonProperty("clusterName")]
        public string ClusterName = "";  
        [JsonProperty("user")]
        public string User = "";
        [JsonProperty("sparkVersion")]
        public string SparkVersion = ""; 
    }
}