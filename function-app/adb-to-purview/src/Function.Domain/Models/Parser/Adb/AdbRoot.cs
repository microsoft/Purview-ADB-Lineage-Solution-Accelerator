using Newtonsoft.Json;

namespace Function.Domain.Models.Adb
{
    public class AdbRoot
    {
        [JsonProperty("job_id")]
        public long JobId = 0;
       [JsonProperty("run_id")]
        public long RunId = 0;
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
        [JsonProperty("trigger")]
        public string Trigger = "";
        [JsonProperty("creator_user_name")]
        public string CreatorUserName = "";
        [JsonProperty("run_name")]
        public string RunName = "";
        [JsonProperty("run_page_url")]
        public string RunPageUrl = "";
        [JsonProperty("run_type")]
        public string RunType = "";
        [JsonProperty("parent_run_id")]
        public long ParentRunId = 0;
        [JsonProperty("tasks")]
        public JobTask[]? JobTasks = null;

    }
}