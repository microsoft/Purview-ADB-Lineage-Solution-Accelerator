using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Adb
{
    public class NotebookTask
    {
        [JsonProperty("notebook_path")]
        public string NotebookPath = "";

        [JsonProperty("base_parameters")]
        public Dictionary<string,string> BaseParameters = new Dictionary<string, string>();

    }
}