using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Adb
{
    public class SparkPythonTask
    {
        [JsonProperty("python_file")]
        public string PythonFile = "";
        [JsonProperty("parameters")]
        public List<string> Parameters = new List<string>();

    }
}