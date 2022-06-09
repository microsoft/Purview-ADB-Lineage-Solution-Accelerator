using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class InputOutput
    {
        [JsonProperty("uniqueAttributes")]
        public UniqueAttributes UniqueAttributes { get; set; } = new UniqueAttributes();
        [JsonProperty("typeName")]
        public string TypeName = ""; 
    }

}