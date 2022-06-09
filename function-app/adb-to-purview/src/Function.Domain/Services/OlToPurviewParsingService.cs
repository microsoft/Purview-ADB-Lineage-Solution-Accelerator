using System;
using System.Threading.Tasks;
using Function.Domain.Models.OL;
using Function.Domain.Models.Purview;
using Function.Domain.Models.Adb;
using Function.Domain.Models.Settings;
using Newtonsoft.Json;
using Function.Domain.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Function.Domain.Services
{
    /// <summary>
    /// Service responsible for parsing OpenLineage messages and turning them into Purview / Atlas entities.
    /// </summary>
    public class OlToPurviewParsingService : IOlToPurviewParsingService
    {
        private ILogger<OlToPurviewParsingService> _logger;
        private ILoggerFactory _loggerFactory;
        const string PREFIX = "{\"entities\": [";
        const string SUFFIX = "]}";
        private IConfiguration _config;

        /// <summary>
        /// Constructs the OlToPurviewParsingService from the Function framework using DI
        /// </summary>
        /// <param name="loggerFactory">Logger Factory to support DI from function framework or code calling helper classes</param>
        /// <param name="config">Function framwork config from DI</param>
        public OlToPurviewParsingService(ILoggerFactory loggerFactory, IConfiguration config)
        {
            _logger = loggerFactory.CreateLogger<OlToPurviewParsingService>();
            _loggerFactory = loggerFactory;
            _config = config;
        }
        
        /// <summary>
        /// Takes in metadata from ADB API and OpenLineage, and returns Atlas object JSON to create these entities in Purview
        /// </summary>
        /// <param name="eventData">Contains OpenLineage and, optionally data obtained from the ADB Jobs API</param>
        /// <returns>Serialized Atlas entities</returns>
        public string? GetPurviewFromOlEvent(EnrichedEvent eventData)
        {
            if (!verifyEventData(eventData))
            {
                _logger.LogWarning($"OlToPurviewParsingService-GetPurviewFromOlEventAsync: Event data is not valid - eventData: {JsonConvert.SerializeObject(eventData)}");
                return null;
            }

            IDatabricksToPurviewParser parser = new DatabricksToPurviewParser(_loggerFactory, _config, eventData);

            if (eventData.IsInteractiveNotebook)
            {
                return ParseInteractiveNotebook(parser);
            }
            else if (parser.GetJobType() == JobType.JobNotebook)
            {
                return ParseJobNotebook(parser);
            }
            else
            {
                return ParseJobTask(parser);
            }
        }

        private string ParseInteractiveNotebook(IDatabricksToPurviewParser parser)
        {
            var databricksWorkspace = parser.GetDatabricksWorkspace();
            var databricksNotebook = parser.GetDatabricksNotebook(databricksWorkspace.Attributes.QualifiedName, true);
            var databricksProcess = parser.GetDatabricksProcess(databricksNotebook.Attributes.QualifiedName);

            var databricksWorkspaceStr = JsonConvert.SerializeObject(databricksWorkspace);
            var databricksNotebookStr = JsonConvert.SerializeObject(databricksNotebook);
            var databricksProcessStr = JsonConvert.SerializeObject(databricksProcess);

            return $"{PREFIX}{databricksWorkspaceStr},{databricksNotebookStr},{databricksProcessStr}{SUFFIX}";
        }

        private string ParseJobNotebook(IDatabricksToPurviewParser parser)
        {
            var databricksWorkspace = parser.GetDatabricksWorkspace();
            var databricksJob = parser.GetDatabricksJob(databricksWorkspace.Attributes.QualifiedName);
            var databricksNotebook = parser.GetDatabricksNotebook(databricksWorkspace.Attributes.QualifiedName, false);
            var databricksNotebookTask = parser.GetDatabricksNotebookTask(databricksNotebook.Attributes.QualifiedName,
                                                                            databricksJob.Attributes.QualifiedName);
            var databricksProcess = parser.GetDatabricksProcess(databricksNotebookTask.Attributes.QualifiedName);
            var databricksWorkspaceStr = JsonConvert.SerializeObject(databricksWorkspace);
            var databricksNotebookStr = JsonConvert.SerializeObject(databricksNotebook);
            var databricksJobStr = JsonConvert.SerializeObject(databricksJob);
            var databricksNotebookTaskStr = JsonConvert.SerializeObject(databricksNotebookTask);
            var databricksProcessStr = JsonConvert.SerializeObject(databricksProcess);

            return $"{PREFIX}{databricksWorkspaceStr},{databricksNotebookStr},{databricksJobStr},{databricksNotebookTaskStr},{databricksProcessStr}{SUFFIX}";
        }

        private string ParseJobTask(IDatabricksToPurviewParser parser)
        {
            var databricksWorkspace = parser.GetDatabricksWorkspace();
            var databricksJob = parser.GetDatabricksJob(databricksWorkspace.Attributes.QualifiedName);
            IDatabricksJobTaskAttributes databricksTaskAttributes;
            string databricksTaskStr = "";
            switch (parser.GetJobType())
            {
                case JobType.JobJar:
                    var databricksSparkJarTask = parser.GetDatabricksSparkJarTask(databricksJob.Attributes.QualifiedName);
                    databricksTaskAttributes = (IDatabricksJobTaskAttributes) databricksSparkJarTask.Attributes;
                    databricksTaskStr = JsonConvert.SerializeObject(databricksSparkJarTask);
                    break;
                case JobType.JobPython:
                    var databricksPythonTask = parser.GetDatabricksPythonTask(databricksJob.Attributes.QualifiedName);
                    databricksTaskAttributes = (IDatabricksJobTaskAttributes) databricksPythonTask.Attributes;
                    databricksTaskStr = JsonConvert.SerializeObject(databricksPythonTask);                
                    break;
                case JobType.JobWheel:
                    var databricksPythonWheelTask = parser.GetDatabricksPythonWheelTask(databricksJob.Attributes.QualifiedName);
                    databricksTaskAttributes = (IDatabricksJobTaskAttributes) databricksPythonWheelTask.Attributes;
                    databricksTaskStr = JsonConvert.SerializeObject(databricksPythonWheelTask);
                    break;
                default:
                    _logger.LogWarning($"OlToPurviewParsingService-GetPurviewFromOlEventAsync: Job type is not supported");
                    return "";
            }

            if (databricksTaskAttributes == null || databricksTaskStr == "")
            {
                _logger.LogWarning($"OlToPurviewParsingService-GetPurviewFromOlEventAsync: Unable to get task attributes");
                return "";                
            }

            var databricksProcess = parser.GetDatabricksProcess(databricksTaskAttributes.QualifiedName);
            var databricksWorkspaceStr = JsonConvert.SerializeObject(databricksWorkspace);
            var databricksJobStr = JsonConvert.SerializeObject(databricksJob);
            var databricksProcessStr = JsonConvert.SerializeObject(databricksProcess);

            return $"{PREFIX}{databricksWorkspaceStr},{databricksJobStr},{databricksTaskStr},{databricksProcessStr}{SUFFIX}";
        }

        private bool verifyEventData(EnrichedEvent eventData)
        {
            if (eventData == null || eventData.OlEvent == null)
            {
                return false;
            }
            if ((eventData?.OlEvent?.Run?.Facets?.EnvironmentProperties?.EnvironmentProperties.SparkDatabricksNotebookPath == null ||
                eventData?.OlEvent?.Run?.Facets?.EnvironmentProperties?.EnvironmentProperties.SparkDatabricksNotebookPath == "") &&
                eventData?.AdbRoot == null)
            {
                return false;
            }
            return true;
        }
    }
}