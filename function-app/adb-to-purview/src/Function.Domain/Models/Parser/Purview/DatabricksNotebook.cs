using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class DatabricksNotebook
    {
        [JsonProperty("typeName")]
        public string TypeName = "databricks_notebook";
        [JsonProperty("attributes")]
        public DatabricksNotebookAttributes Attributes = new DatabricksNotebookAttributes();
        [JsonProperty("relationshipAttributes")]
        public DatabricksNotebookRelationshipAttributes RelationshipAttributes = new DatabricksNotebookRelationshipAttributes();
    }
}