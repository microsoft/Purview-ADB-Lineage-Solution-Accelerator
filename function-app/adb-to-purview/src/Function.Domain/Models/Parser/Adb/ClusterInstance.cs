using Newtonsoft.Json;

namespace Function.Domain.Models.Adb
{
    public class ClusterInstance
    {
        [JsonProperty("cluster_id")]
        public string ClusterId = "";

    }
}