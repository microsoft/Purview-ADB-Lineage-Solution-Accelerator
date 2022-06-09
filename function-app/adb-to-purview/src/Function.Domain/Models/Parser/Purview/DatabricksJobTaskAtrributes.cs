using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
    public class DatabricksJobTaskAttributes
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        [JsonProperty("qualifiedName")]
        public string QualifiedName { get; set; } = "";
        [JsonProperty("jobId")]
        public long JobId { get; set; } = 0;
        [JsonProperty("clusterId")]
        public string ClusterId { get; set; } = "";
        [JsonProperty("sparkVersion")]
        public string SparkVersion { get; set; } = "";
    }
}