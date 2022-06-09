using System.Threading.Tasks;
using Function.Domain.Models.OL;

namespace Function.Domain.Helpers
{
    public interface IOlMessageConsolodation
    {

        public Task<bool> CaptureEnvironmentFromStart(Event olEvent, string jobRunId, EnvironmentPropsParent envParent);

        public Task<Event?> ConsolodateCompleteEvent(Event olEvent, string jobRunId);

    }
}