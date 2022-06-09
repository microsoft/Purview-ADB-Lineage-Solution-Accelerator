using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.OL
{
    public class Plan
    {
        [JsonProperty("class")]
        public string ApacheClass { get; set; } = "";

        [JsonProperty("num-children")]
        public int NumChildren { get; set; }
        public string outputColumnNames { get; set; } = "";

        [JsonProperty("projectList", NullValueHandling = NullValueHandling.Ignore)]
        public List<List<Project>> projectList { get; set; } = new List<List<Project>>();

        [JsonProperty("aggregateExpressions", NullValueHandling = NullValueHandling.Ignore)]
        public List<List<Project>> aggregateExpressions { get; set; } = new List<List<Project>>();
    }
}