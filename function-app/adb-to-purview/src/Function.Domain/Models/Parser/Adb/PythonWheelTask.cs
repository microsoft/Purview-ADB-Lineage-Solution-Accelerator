using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Adb
{
    public class PythonWheelTask
    {
        [JsonProperty("package_name")]
        public string PackageName = "";
        [JsonProperty("entry_point")]
        public string EntryPoint = "";
         [JsonProperty("parameters")]
        public List<string> Parameters = new List<string>();
        [JsonProperty("named_parameters")]
        public Dictionary<string,string> NamedParameters = new Dictionary<string,string>(); 

    }
}