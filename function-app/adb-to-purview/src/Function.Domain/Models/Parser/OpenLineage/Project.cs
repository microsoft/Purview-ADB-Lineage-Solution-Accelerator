using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Function.Domain.Models.OL
{

    public class Project
    {
        [JsonProperty("class")]
        public string ApacheClass { get; set; } = "";

        [JsonProperty("num-children")]
        public long NumChildren { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("dataType")]
        public string DataType { get; set; } = "";

        [JsonProperty("qualifier")]
        public List<object> Qualifier { get; set; } = new List<object>();
    }



}
