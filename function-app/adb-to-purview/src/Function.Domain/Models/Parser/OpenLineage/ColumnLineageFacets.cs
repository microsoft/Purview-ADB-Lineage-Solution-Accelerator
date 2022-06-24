using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Function.Domain.Models.OL
{
    [JsonObject("facets")]
    public class ColumnLineageFacets
    {
        [JsonProperty("columnLineage")]
        public ColumnLineageFacetsClass LifeCycleStateChange = new ColumnLineageFacetsClass();
    }

    public class ColumnLineageFacetsClass
    {
        [JsonProperty("fields")]
        public Dictionary <string,ColumnLineageInputFieldClass>  fields = new Dictionary < string, ColumnLineageInputFieldClass > ();
    }
    [JsonObject("inputFields")]
    public class ColumnLineageInputFieldClass
    {
        
        public List<ColumnLineageIdentifierClass> inputFields = new List<ColumnLineageIdentifierClass>();
    }

    public class ColumnLineageIdentifierClass
    {
       [JsonProperty("namespace")]
       public string nameSpace { get; set; } = "";
       [JsonProperty("name")]
       public string name { get; set; } = "";
       [JsonProperty("field")] 
       public string field { get; set; } = "";
    }
}