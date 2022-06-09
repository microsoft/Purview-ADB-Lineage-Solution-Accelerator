using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
       public class DatabricksNotebookTaskAttributes : DatabricksJobTaskAttributes, IDatabricksJobTaskAttributes
    {
        [JsonProperty("notebookPath")]
        public string NotebookPath = "";
        [JsonProperty("baseParameters")]
        public Dictionary<string,string> BaseParameters = new Dictionary<string,string>(); 
    }
}