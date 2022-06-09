using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
       public class DatabricksJobAttributes
    {
        [JsonProperty("name")]
        public string Name = "";
        [JsonProperty("qualifiedName")]
        public string QualifiedName = "";  
        [JsonProperty("jobId")]
        public long JobId = 0;  
        [JsonProperty("creatorUserName")]
        public string CreatorUserName = ""; 
    }
}