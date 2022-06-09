using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class DatabricksPythonWheelTask
    {
        [JsonProperty("typeName")]
        public string TypeName { get; set; } = "databricks_python_wheel_task";
        [JsonProperty("attributes")]
        public DatabricksPythonWheelTaskAttributes Attributes { get; set; } = new DatabricksPythonWheelTaskAttributes();
        [JsonProperty("relationshipAttributes")]
        public DatabricksTaskRelationshipAttributes RelationshipAttributes { get; set; } = new DatabricksTaskRelationshipAttributes();
    }
}