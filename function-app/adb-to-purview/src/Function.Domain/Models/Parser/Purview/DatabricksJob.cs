using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class DatabricksJob
    {
        [JsonProperty("typeName")]
        public string TypeName = "databricks_job";
        [JsonProperty("attributes")]
        public DatabricksJobAttributes Attributes = new DatabricksJobAttributes();
        [JsonProperty("relationshipAttributes")]
        public DatabricksJobRelationshipAttributes RelationshipAttributes = new DatabricksJobRelationshipAttributes();
    }
}