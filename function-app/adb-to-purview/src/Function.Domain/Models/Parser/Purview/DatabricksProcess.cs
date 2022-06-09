using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class DatabricksProcess
    {
        [JsonProperty("typeName")]
        public string TypeName = "databricks_process";
        [JsonProperty("attributes")]
        public DatabricksProcessAttributes Attributes = new DatabricksProcessAttributes();
        [JsonProperty("relationshipAttributes")]
        public DatabricksProcessRelationshipAttributes RelationshipAttributes = new DatabricksProcessRelationshipAttributes();
    }
}