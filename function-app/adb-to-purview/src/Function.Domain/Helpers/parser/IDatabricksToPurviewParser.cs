using Function.Domain.Models.Purview;
using Function.Domain.Models.Adb;

namespace Function.Domain.Helpers
{
    public interface IDatabricksToPurviewParser
    {
        public DatabricksWorkspace GetDatabricksWorkspace();
        public DatabricksJob GetDatabricksJob(string workspaceQn);
        public DatabricksNotebook GetDatabricksNotebook(string workspaceQn, bool isInteractive);
        public DatabricksNotebookTask GetDatabricksNotebookTask(string notebookQn, string workspaceQn);
        public DatabricksPythonTask GetDatabricksPythonTask(string jobQn);
        public DatabricksPythonWheelTask GetDatabricksPythonWheelTask(string jobQn);
        public DatabricksSparkJarTask GetDatabricksSparkJarTask(string jobQn);
        public DatabricksProcess GetDatabricksProcess(string taskQn);
        public JobType GetJobType();
    }
}