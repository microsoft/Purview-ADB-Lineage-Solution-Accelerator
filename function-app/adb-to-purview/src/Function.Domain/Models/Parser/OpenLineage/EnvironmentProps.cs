using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.OL
{
    public class EnvironmentProps
    {
        [JsonProperty("spark.databricks.clusterUsageTags.clusterName")]
        public string  SparkDatabricksClusterUsageTagsClusterName = "";
        [JsonProperty("spark.databricks.job.runId")]
        public string SparkDatabricksJobRunId = "";
        [JsonProperty("spark.databricks.job.type")]
        public string SparkDatabricksJobType = "";
        [JsonProperty("spark.databricks.clusterUsageTags.azureSubscriptionId")]
        public string SparkDatabricksClusterUsageTagsAzureSubscriptionId  = "";
        [JsonProperty("spark.databricks.notebook.path")]
        public string SparkDatabricksNotebookPath = "";
        [JsonProperty("spark.databricks.clusterUsageTags.clusterOwnerOrgId")]
        public string SparkDatabricksClusterUsageTagsClusterOwnerOrgId = "";

        //Other MountPoint Structure
        //public Dictionary<string,string> MountPoints = new Dictionary<string, string>();
        public List<MountPoint> MountPoints = new List<MountPoint>();
        public string User = "";
        public string UserId = "";
        public string OrgId = "";
    }
}