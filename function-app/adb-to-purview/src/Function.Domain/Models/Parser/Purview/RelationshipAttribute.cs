using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class RelationshipAttribute
    {
        [JsonProperty("qualifiedName")]
        public string QualifiedName = ""; 
    }
}