using Newtonsoft.Json;
namespace Function.Domain.Models
{
    public class UniqueAttributes
    {
        [JsonProperty("qualifiedName")]
        public string QualifiedName { get; set; } = "";

    }

}