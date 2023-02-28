// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Function.Domain.Helpers;
using Function.Domain.Models.Settings;
using Function.Domain.Models.OL;
using Function.Domain.Models.Adb;
using Function.Domain.Models.Purview;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Function.Domain.Helpers
{
    /// <summary>
    /// Creates Purview Databricks objects from OpenLineage and ADB data from the jobs API
    /// </summary>
    public class DatabricksToPurviewParser: IDatabricksToPurviewParser
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ParserSettings _parserConfig;
        private readonly IQnParser _qnParser;
        private readonly IColParser _colParser;
        private readonly EnrichedEvent _eEvent;
        private readonly string _adbWorkspaceUrl;
        const string SETTINGS = "OlToPurviewMappings";
        Regex ADF_JOB_NAME_REGEX = new Regex(@"^ADF_(.*)_(.*)_(.*)_(.*)$", RegexOptions.Compiled );

        /// <summary>
        /// Constructor for DatabricksToPurviewParser
        /// </summary>
        /// <param name="loggerFactory">Loggerfactory from Function framework DI</param>
        /// <param name="configuration">Configuration from Function framework DI</param>
        /// <param name="eEvent">The enriched event which combines OpenLineage data with data from ADB get job API</param>
        public DatabricksToPurviewParser(ILoggerFactory loggerFactory, IConfiguration configuration, EnrichedEvent eEvent)
        {
            _logger = loggerFactory.CreateLogger<DatabricksToPurviewParser>();
            _loggerFactory = loggerFactory;

            try{
            var map = configuration[SETTINGS];
            _parserConfig = JsonConvert.DeserializeObject<ParserSettings>(map) ?? throw new MissingCriticalDataException("critical config not found");
            } 
            catch (Exception ex) {
                _logger.LogError(ex,"DatabricksToPurviewParser: Error retrieving ParserSettings.  Please make sure these are configured on your function.");
                throw;
            }
            if (_parserConfig is null || eEvent.OlEvent?.Run.Facets.EnvironmentProperties is null)
            {
                var ex = new MissingCriticalDataException("DatabricksToPurviewParser: Missing critical data.  Please make sure your OlToPurviewMappings configuration is correct.");
                _logger.LogError(ex, ex.Message);
                throw ex;
            }
            _eEvent = eEvent;
            _adbWorkspaceUrl = _eEvent.OlEvent.Job.Namespace.Split('#')[0];
            _parserConfig.AdbWorkspaceUrl = this.GetDatabricksWorkspace().Attributes.Name;
            _qnParser = new QnParser(_parserConfig, _loggerFactory,
                                      _eEvent.OlEvent.Run.Facets.EnvironmentProperties!.EnvironmentProperties.MountPoints);

            _colParser = new ColParser(_parserConfig, _loggerFactory,
                                      _eEvent.OlEvent,
                                      _qnParser);

            _adbWorkspaceUrl = _eEvent.OlEvent.Job.Namespace.Split('#')[0];

        }

        /// <summary>
        /// Gets the job type from the supported ADB job types.  Currently all are supported except Spark Submit jobs.
        /// </summary>
        /// <returns></returns>
        public JobType GetJobType()
        {
            if (_eEvent.AdbRoot?.JobTasks?[0] == null)
            {
                return JobType.InteractiveNotebook;
            }
            else if (_eEvent.AdbRoot?.JobTasks[0].NotebookTask != null)
            {
                return JobType.JobNotebook;
            }
            else if (_eEvent.AdbRoot?.JobTasks[0].SparkPythonTask != null)
            {
                return JobType.JobPython;
            }
            else if (_eEvent.AdbRoot?.JobTasks[0].PythonWheelTask != null)
            {
                return JobType.JobWheel;
            }
            else if (_eEvent.AdbRoot?.JobTasks[0].SparkJarTask != null)
            {
                return JobType.JobJar;
            }
            return JobType.Unsupported;
        }

        /// <summary>
        /// Creates a Purview Databricks workspace object for an enriched event
        /// </summary>
        /// <returns>A Databricks workspace object</returns>
        public DatabricksWorkspace GetDatabricksWorkspace()
        {
            DatabricksWorkspace databricksWorkspace = new DatabricksWorkspace();
            databricksWorkspace.Attributes.Name = $"{_adbWorkspaceUrl}.azuredatabricks.net";
            databricksWorkspace.Attributes.QualifiedName = $"databricks://{_adbWorkspaceUrl}.azuredatabricks.net";
            
            return databricksWorkspace;
        }

        /// <summary>
        /// Creates a Databricks job object from an enriched event
        /// </summary>
        /// <param name="workspaceQn">Requires a workspace qualified name to form its own qualified name</param>
        /// <returns>A Databricks job object</returns>
        /// <exception cref="MissingCriticalDataException">Thrown if information from the get ADB job API is missing</exception>
        public DatabricksJob GetDatabricksJob(string workspaceQn)
        {
            AdbRoot adbJobRoot; 
            if (_eEvent.AdbParentRoot != null)
            {
                adbJobRoot = _eEvent.AdbParentRoot;
            }
            else
            {
                if (_eEvent.AdbRoot == null)
                {
                    throw new MissingCriticalDataException("DatabricksToPurviewParser-GetDatabricksJob: Critical ADB data is missing, cannot create job.");
                }
                adbJobRoot  = _eEvent.AdbRoot;
            }
            var databricksJob = new DatabricksJob();
            string _jobName = adbJobRoot.RunName;
            string _jobId = _eEvent.AdbRoot!.JobId.ToString();
            // Special case for Azure Data Factory
            // If we match this pattern in the job name, strip the last element since it's a random guid
            // This will allow us to have the same name / qualified name each run
            if (IsAdfJobName(_jobName)){
                _logger.LogInformation($"Azure Data Factory Job being processed: ({_jobName})");
                _jobName = TruncateAdfJobName(_jobName);
                _jobId = _jobName;
            }
            databricksJob.Attributes.Name = _jobName;
            databricksJob.Attributes.QualifiedName = $"databricks://{_adbWorkspaceUrl}.azuredatabricks.net/jobs/{_jobId}";
            databricksJob.Attributes.JobId = adbJobRoot.JobId;
            databricksJob.Attributes.CreatorUserName = adbJobRoot.CreatorUserName;

            databricksJob.RelationshipAttributes.Workspace.QualifiedName = workspaceQn;

            return databricksJob;
        }

        /// <summary>
        /// Creates a Databricks notebook object from an enriched event
        /// </summary>
        /// <param name="workspaceQn">Requires a workspace qualified name to form its own qualified name</param>
        /// <param name="isInteractive">Indicates whether the notebook was run interactively, or as part of a job task</param>
        /// <returns>A Databricks notebook object</returns>
        public DatabricksNotebook GetDatabricksNotebook(string workspaceQn, bool isInteractive)
        {
            var databricksNotebook = new DatabricksNotebook();
            string notebookPath = "";
            if (isInteractive)
            {
                notebookPath = _eEvent.OlEvent!.Run.Facets.EnvironmentProperties!.EnvironmentProperties.SparkDatabricksNotebookPath;
            }
            else
            {
                if (_eEvent.AdbRoot?.JobTasks?[0]?.NotebookTask?.NotebookPath != null)
                {
                    notebookPath = _eEvent.AdbRoot.JobTasks[0].NotebookTask!.NotebookPath;
                }
            }
            var notebookName = notebookPath.Substring(notebookPath.LastIndexOf("/") + 1);
            databricksNotebook.Attributes.Name = notebookName;
            databricksNotebook.Attributes.QualifiedName = $"{workspaceQn}/notebooks/{notebookPath.Trim('/')}";
            databricksNotebook.Attributes.ClusterName = _eEvent.OlEvent!.Run.Facets.EnvironmentProperties!.EnvironmentProperties.SparkDatabricksClusterUsageTagsClusterName;
            databricksNotebook.Attributes.User = _eEvent.OlEvent!.Run.Facets.EnvironmentProperties!.EnvironmentProperties.User;
            databricksNotebook.Attributes.SparkVersion = _eEvent.OlEvent.Run.Facets.SparkVersion.SparkVersion;

            databricksNotebook.RelationshipAttributes.Workspace.QualifiedName = workspaceQn;
            
            return databricksNotebook;
        }

        // This is data for the base job task. If it is not present, it is a critical error.
        // Data specific to a job task, if missing, will be filled in with default empty values.
        private void GetDatabricksJobTaskAttributes(DatabricksJobTaskAttributes taskAttributes)
        {
            if (_eEvent.AdbRoot?.JobTasks is null || _eEvent.AdbRoot.JobTasks.Length == 0)
            {
                var ex = new MissingCriticalDataException("DatabricksToPurviewParser-GetDatabricksJobTaskAttributes: Missing critical data - JobTasks.");
                _logger.LogError(ex, ex.Message);
                throw ex;
            }
            
            string _taskKey = _eEvent.AdbRoot.JobTasks[0].TaskKey;
            string _taskJobId = _eEvent.AdbRoot.JobId.ToString();
            // Special case for Azure Data Factory
            // If we match this pattern in the job name, strip the last element since it's a random guid
            // This will allow us to have the same name / qualified name each run
            if (IsAdfJobName(_taskKey)){
                _logger.LogInformation($"Azure Data Factory Task being processed: ({_taskKey})");
                _taskJobId = TruncateAdfJobName(_taskKey);
                _taskKey = TruncateAdfTaskName(_taskKey);
            }
            taskAttributes.Name = _taskKey;
            string jobQn = $"databricks://{_adbWorkspaceUrl}.azuredatabricks.net/jobs/{_taskJobId}";
            taskAttributes.QualifiedName = $"{jobQn}/tasks/{_taskKey}";
            taskAttributes.JobId = _eEvent.AdbRoot.JobId;
            taskAttributes.ClusterId = _eEvent.AdbRoot.JobTasks[0].ClusterInstance.ClusterId;
            taskAttributes.SparkVersion = _eEvent.OlEvent?.Run.Facets.SparkVersion.SparkVersion ?? "";
        }

        /// <summary>
        /// Creates a Databricks notebook task object from an enriched event
        /// </summary>
        /// <param name="notebookQn">Requires a notebook qualified name to form its own qualified name</param>
        /// <param name="jobQn">Requires a job qualified name to form its own qualified name</param>
        /// <returns>A Databricks notebook task object</returns>
        public DatabricksNotebookTask GetDatabricksNotebookTask(string notebookQn, string jobQn)
        {
            var databricksNotebookTask = new DatabricksNotebookTask();
            GetDatabricksJobTaskAttributes(databricksNotebookTask.Attributes);
            databricksNotebookTask.Attributes.NotebookPath = _eEvent.AdbRoot?.JobTasks?[0].NotebookTask?.NotebookPath ?? "";
            if (_eEvent.AdbRoot?.JobTasks?[0].NotebookTask?.BaseParameters.Keys is not null)
            {
                databricksNotebookTask.Attributes.BaseParameters = _eEvent.AdbRoot.JobTasks[0].NotebookTask!.BaseParameters;
            }
            databricksNotebookTask.RelationshipAttributes.Notebook.QualifiedName = notebookQn;
            databricksNotebookTask.RelationshipAttributes.Job.QualifiedName = jobQn;

            return databricksNotebookTask;
        }

        /// <summary>
        /// Creates a Databricks python task object from an enriched event
        /// </summary>
        /// <param name="jobQn">Requires a job qualified name to form its own qualified name</param>
        /// <returns>A Databricks python task object</returns>
        public DatabricksPythonTask GetDatabricksPythonTask(string jobQn)
        {
            var databricksPythonTask = new DatabricksPythonTask();
            GetDatabricksJobTaskAttributes(databricksPythonTask.Attributes);
            databricksPythonTask.Attributes.PythonFile = _eEvent.AdbRoot?.JobTasks?[0].SparkPythonTask?.PythonFile ?? "";
            databricksPythonTask.Attributes.Parameters = _eEvent.AdbRoot?.JobTasks?[0].SparkPythonTask?.Parameters ?? new List<string>();
            databricksPythonTask.RelationshipAttributes.Job.QualifiedName = jobQn;
            
            return databricksPythonTask;
        }

        /// <summary>
        /// Creates a Databricks python wheel task object from an enriched event
        /// </summary>
        /// <param name="jobQn">Requires a job qualified name to form its own qualified name</param>
        /// <returns>A Databricks python wheel task object</returns>
        public DatabricksPythonWheelTask GetDatabricksPythonWheelTask(string jobQn)
        {
            var databricksPythonWheelTask = new DatabricksPythonWheelTask();
            GetDatabricksJobTaskAttributes(databricksPythonWheelTask.Attributes);
            databricksPythonWheelTask.Attributes.PackageName = _eEvent.AdbRoot?.JobTasks?[0].PythonWheelTask?.PackageName ?? "";
            databricksPythonWheelTask.Attributes.EntryPoint = _eEvent.AdbRoot?.JobTasks?[0].PythonWheelTask?.EntryPoint ?? "";
            databricksPythonWheelTask.Attributes.Parameters = _eEvent.AdbRoot?.JobTasks?[0].PythonWheelTask?.Parameters ?? new List<string>();
            databricksPythonWheelTask.Attributes.Wheel = _eEvent.AdbRoot?.JobTasks?[0].Libraries?[0].wheelName ?? "";

            databricksPythonWheelTask.RelationshipAttributes.Job.QualifiedName = jobQn;
            
            return databricksPythonWheelTask;
        }

        /// <summary>
        /// Creates a Databricks spark jar task object from an enriched event
        /// </summary>
        /// <param name="jobQn">Requires a job qualified name to form its own qualified name</param>
        /// <returns>A Databricks spark jar task object</returns>
        public DatabricksSparkJarTask GetDatabricksSparkJarTask(string jobQn)
        {
            var databricksSparkJarTask = new DatabricksSparkJarTask();
            GetDatabricksJobTaskAttributes(databricksSparkJarTask.Attributes);
            databricksSparkJarTask.Attributes.MainClassName = _eEvent.AdbRoot?.JobTasks?[0].SparkJarTask?.MainClassName ?? "";
            databricksSparkJarTask.Attributes.JarUri = _eEvent.AdbRoot?.JobTasks?[0].SparkJarTask?.JarUri ?? "";
            databricksSparkJarTask.Attributes.Parameters = _eEvent.AdbRoot?.JobTasks?[0].SparkJarTask?.Parameters ?? new List<string>();
            databricksSparkJarTask.Attributes.Jar = _eEvent.AdbRoot?.JobTasks?[0].Libraries?[0].jarName ?? "";

            databricksSparkJarTask.RelationshipAttributes.Job.QualifiedName = jobQn;
            
            return databricksSparkJarTask;
        }

        /// <summary>
        /// Creates a Databricks process object from an enriched event
        /// </summary>
        /// <param name="taskQn">Requires a task qualified name to form its own qualified name</param>
        /// <returns>A Databricks spark jar task object</returns>
        public DatabricksProcess GetDatabricksProcess(string taskQn)
        {
            var databricksProcess = new DatabricksProcess();
            //var ColumnAttributes = new ColumnLevelAttributes();

            var inputs = new List<InputOutput>();
            foreach (IInputsOutputs input in _eEvent.OlEvent!.Inputs)
            {
                inputs.Add(GetInputOutputs(input));
            }

            var outputs = new List<InputOutput>();
            foreach (IInputsOutputs output in _eEvent.OlEvent!.Outputs)
            {
                outputs.Add(GetInputOutputs(output));
            }

            databricksProcess.Attributes = GetProcAttributes(taskQn, inputs,outputs,_eEvent.OlEvent);
            databricksProcess.RelationshipAttributes.Task.QualifiedName = taskQn; 
            return databricksProcess;
        }

        
        private DatabricksProcessAttributes GetProcAttributes(string taskQn, List<InputOutput> inputs, List<InputOutput> outputs, Event sparkEvent)
        {
            var pa = new DatabricksProcessAttributes();
            pa.Name = sparkEvent.Run.Facets.EnvironmentProperties!.EnvironmentProperties.SparkDatabricksNotebookPath + sparkEvent.Outputs[0].Name;
            pa.QualifiedName = $"{taskQn}/processes/{GetInputsOutputsHash(inputs, outputs)}";
            pa.ColumnMapping = JsonConvert.SerializeObject(_colParser.GetColIdentifiers());
            pa.SparkPlan = sparkEvent.Run.Facets.SparkLogicalPlan.ToString(Formatting.None);
            pa.Inputs = inputs;
            pa.Outputs = outputs;
            return pa;
        }

        private InputOutput GetInputOutputs(IInputsOutputs inOut)
        {
            var id = _qnParser.GetIdentifiers(inOut.NameSpace,inOut.Name);
            var inputOutputId = new InputOutput();
            inputOutputId.TypeName = id.PurviewType;
            inputOutputId.UniqueAttributes.QualifiedName = id.QualifiedName;

            return inputOutputId;
        }

        private string GetInputsOutputsHash(List<InputOutput> inputs, List<InputOutput> outputs)
        {
            inputs.Sort((x, y) => x.UniqueAttributes.QualifiedName.CompareTo(y.UniqueAttributes.QualifiedName));;
            StringBuilder sInputs = new StringBuilder(inputs.Count);
            foreach (var input in inputs)
            {
                sInputs.Append(input.UniqueAttributes.QualifiedName.ToLower().ToString());
                if (!input.Equals(inputs.Last()))
                {
                    sInputs.Append(",");
                }
            }
            var inputHash = GenerateMd5Hash(sInputs.ToString());
            // Outputs should only ever have one item
            var outputHash = GenerateMd5Hash(outputs[0].UniqueAttributes.QualifiedName.ToString());

            return $"{inputHash}->{outputHash}";
        }

        private string GenerateMd5Hash(string input)
        {
            byte[] tmpSource;
            byte[] tmpHash;

            //Create a byte array from source data.
            tmpSource = ASCIIEncoding.ASCII.GetBytes(input);

            //Compute hash based on source data.
            tmpHash = MD5.Create().ComputeHash(tmpSource);

            StringBuilder sOutput = new StringBuilder(tmpHash.Length);
            for (int i=0;i < tmpHash.Length; i++)
            {
                sOutput.Append(tmpHash[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

        // Special case for Azure Data Factory
        // If we match this pattern in the job name, strip the last element since it's a random guid
        // This will allow us to have the same name / qualified name each run
        private bool IsAdfJobName(string inputName){
            // Follows the pattern ADF_factoryName_pipelineName_notebookName_pipelineRunId
            return (ADF_JOB_NAME_REGEX.Matches(inputName).Count > 0);
        }
        private string TruncateAdfTaskName(string inputName){
            // Return ADF_factoryName_pipelineName_notebookName portions
            string[] job_name_parts = inputName.Split("_");
            string[] job_name_except_last_element = job_name_parts.Take(job_name_parts.Count() - 1).ToArray();
            return string.Join("_", job_name_except_last_element);
        }
        private string TruncateAdfJobName(string inputName){
            // Return ADF_factoryName_pipelineName portions
            string[] job_name_parts = inputName.Split("_");
            string[] job_name_except_last_element = job_name_parts.Take(job_name_parts.Count() - 2).ToArray();
            return string.Join("_", job_name_except_last_element);
        }

        public IColParser GetColumnParser()
        {
            return this._colParser;
        }
    }
}