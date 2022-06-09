using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Function.Domain.Models.OL
{
    public class Facets
    {
        [JsonProperty("environment-properties")]
        public EnvironmentPropsParent? EnvironmentProperties;
        [JsonProperty("spark.logicalPlan")]
        public JObject SparkLogicalPlan = new JObject();
        [JsonProperty("spark_version")]
        public SparkVer SparkVersion = new SparkVer();
    }
}