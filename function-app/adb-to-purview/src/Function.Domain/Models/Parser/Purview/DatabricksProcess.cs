using Newtonsoft.Json;
using System.Collections.Generic;
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
        [JsonProperty("columnAttributes")]
        public List<ColumnLevelAttributes> ColumnLevel = new List<ColumnLevelAttributes>();
    }
}