using Newtonsoft.Json;

namespace Function.Domain.Models.OL
{
    public class Inputs: IInputsOutputs
    {
        public string Name { get; set; } = "";
        [JsonProperty("namespace")]
        public string NameSpace { get; set; } = "";
    }
}