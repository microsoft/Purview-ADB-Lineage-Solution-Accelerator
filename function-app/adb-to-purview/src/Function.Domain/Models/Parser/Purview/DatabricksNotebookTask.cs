using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
    public class DatabricksNotebookTask 
    {
        [JsonProperty("typeName")]
        public string TypeName { get; set; } = "databricks_notebook_task";
        [JsonProperty("attributes")]
        public  DatabricksNotebookTaskAttributes Attributes { get; set; } = new DatabricksNotebookTaskAttributes();
        [JsonProperty("relationshipAttributes")]
        public DatabricksNotebookTaskRelationshipAttributes RelationshipAttributes { get; set; } = new DatabricksNotebookTaskRelationshipAttributes();
    }
}