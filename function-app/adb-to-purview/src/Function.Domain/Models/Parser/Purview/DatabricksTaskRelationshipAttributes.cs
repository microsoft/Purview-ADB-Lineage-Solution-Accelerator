using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class DatabricksTaskRelationshipAttributes
    {
        [JsonProperty("job")]
        public RelationshipAttribute Job = new RelationshipAttribute();
    }

}