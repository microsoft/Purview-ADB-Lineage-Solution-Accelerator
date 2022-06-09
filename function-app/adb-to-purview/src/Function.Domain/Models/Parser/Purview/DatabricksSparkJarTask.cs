using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class DatabricksSparkJarTask
    {
        [JsonProperty("typeName")]
        public string TypeName = "databricks_spark_jar_task";
        [JsonProperty("attributes")]
        public DatabricksSparkJarTaskAttributes Attributes = new DatabricksSparkJarTaskAttributes();
        [JsonProperty("relationshipAttributes")]
        public DatabricksTaskRelationshipAttributes RelationshipAttributes = new DatabricksTaskRelationshipAttributes();
    }
}