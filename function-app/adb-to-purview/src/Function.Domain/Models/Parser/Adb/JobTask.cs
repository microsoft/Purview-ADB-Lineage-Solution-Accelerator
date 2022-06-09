using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Adb
{
    public class JobTask
    {
        [JsonProperty("run_id")]
        public long RunId = 0;
        [JsonProperty("task_key")]
        public string TaskKey = "";
        [JsonProperty("existing_cluster_id")]
        public string? ExistingClusterId = null;
        [JsonProperty("cluster_instance")]
        public ClusterInstance ClusterInstance = new ClusterInstance();
        [JsonProperty("run_page_url")]
        public string RunPageUrl = "";
        [JsonProperty("start_time")]
        public long StartTime = 0;
        [JsonProperty("setup_duration")]
        public long SetupDuration = 0;
        [JsonProperty("execution_duration")]
        public long ExecutionDuration = 0;
        [JsonProperty("cleanup_duration")]
        public long CleanupDuration = 0;
        [JsonProperty("end_time")]
        public long EndTime = 0;
        [JsonProperty("libraries")]
        public List<Dictionary<string,string>>? Libraries = null;
        [JsonProperty("notebook_task")]
        public NotebookTask? NotebookTask = null;
        [JsonProperty("spark_jar_task")]
        public SparkJarTask? SparkJarTask = null;
        [JsonProperty("spark_python_task")]
        public SparkPythonTask? SparkPythonTask = null;
        [JsonProperty("python_wheel_task")]
        public PythonWheelTask? PythonWheelTask = null;
    }
}