using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Adb
{
    public class SparkJarTask
    {
        [JsonProperty("jar_uri")]
        public string JarUri = "";
        [JsonProperty("main_class_name")]
        public string MainClassName = "";
        [JsonProperty("parameters")]
        public List<string> Parameters = new List<string>();

    }
}