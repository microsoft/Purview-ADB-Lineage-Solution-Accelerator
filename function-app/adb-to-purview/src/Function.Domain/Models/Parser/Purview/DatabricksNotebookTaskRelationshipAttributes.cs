using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class DatabricksNotebookTaskRelationshipAttributes
    {
        [JsonProperty("job")]
        public RelationshipAttribute Job = new RelationshipAttribute();
        [JsonProperty("notebook")]
        public RelationshipAttribute Notebook = new RelationshipAttribute();
    }

}