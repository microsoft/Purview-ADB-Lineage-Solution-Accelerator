using Newtonsoft.Json;

namespace Function.Domain.Models.OL
{
    public class Outputs : IInputsOutputs
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        [JsonProperty("namespace")]
        public string NameSpace { get; set; } = "";
        [JsonProperty("facets")]
        public OutputFacets Facets = new OutputFacets();
    }
}