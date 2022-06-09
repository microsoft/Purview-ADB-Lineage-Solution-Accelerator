using System.Threading.Tasks;
using Function.Domain.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Function.Domain.Models.OL;
using Function.Domain.Models.Adb;

namespace Function.Domain.Helpers.Parser
{
    /// <summary>
    /// Enriches the OpenLineage Job related messages with data obtained from the ADB Job Rest API
    /// </summary>
    public class OlMessageEnrichment : IOlMessageEnrichment
    {
        private readonly IAdbClientProvider _adbClientProvider;
        private readonly ILogger<OlMessageEnrichment> _logger;

        public OlMessageEnrichment(ILoggerFactory loggerFactory, IConfiguration config)
        {
            _logger = loggerFactory.CreateLogger<OlMessageEnrichment>();
            _adbClientProvider = new AdbClientProvider(loggerFactory, config);
        }

        /// <summary>
        /// Calls the ADB Job Rest API to get the Job details for the given OpenLineage event JobId
        /// </summary>
        /// <param name="olEvent">The OpenLineage event</param>
        /// <returns>If job call succeeds and information is available, an enriched event containing the new data.
        ///  If not, returns null</returns>
        public async Task<EnrichedEvent?> GetEnrichedEvent(Event olEvent)
        {
            // If the run id is present then this is a job triggered run rather then interactive notebook run
            if (olEvent.Run.Facets.EnvironmentProperties != null &&
                olEvent.Run.Facets.EnvironmentProperties.EnvironmentProperties.SparkDatabricksJobRunId != "")
            {
                var jobRunId = olEvent.Run.Facets.EnvironmentProperties.EnvironmentProperties.SparkDatabricksJobRunId;
                var adbWorkspaceUrl = olEvent.Job.Namespace.Split('#')[0];
                var adbRoot = await _adbClientProvider.GetSingleAdbJobAsync(long.Parse(jobRunId), adbWorkspaceUrl);

                if (adbRoot == null)
                {
                    return null;
                }

                AdbRoot? adbParentRoot = null;
                // See if run task was part of a multi-task job
                if (adbRoot.ParentRunId != 0)
                {
                    adbParentRoot = await _adbClientProvider.GetSingleAdbJobAsync(adbRoot.ParentRunId, adbWorkspaceUrl);
                    if (adbParentRoot == null)
                    {
                        _logger.LogWarning($"A parent job exists but cannot be retrieved from Databricks.  Parent job id: {adbRoot.ParentRunId}");
                    }
                }
                var rtrnJob = new EnrichedEvent(olEvent, adbRoot, adbParentRoot);
                rtrnJob.IsInteractiveNotebook = false;
                return rtrnJob;
            }
            var rtrnNb = new EnrichedEvent(olEvent, null, null);
            rtrnNb.IsInteractiveNotebook = true;
            return rtrnNb;
        }

    }
}