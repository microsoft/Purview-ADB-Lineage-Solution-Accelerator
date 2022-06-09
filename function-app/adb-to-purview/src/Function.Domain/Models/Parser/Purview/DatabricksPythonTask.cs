using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class DatabricksPythonTask 
    {
        [JsonProperty("typeName")]
        public string TypeName { get; set; } = "databricks_python_task";
        [JsonProperty("attributes")]
        public DatabricksPythonTaskAttributes Attributes { get; set; } = new DatabricksPythonTaskAttributes();
        [JsonProperty("relationshipAttributes")]
        public DatabricksTaskRelationshipAttributes RelationshipAttributes { get; set; } = new DatabricksTaskRelationshipAttributes();
    }
}