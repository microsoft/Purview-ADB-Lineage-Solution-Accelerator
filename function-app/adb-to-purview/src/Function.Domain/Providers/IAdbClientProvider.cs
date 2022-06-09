using System.Threading.Tasks;
using Function.Domain.Models.Adb;

namespace Function.Domain.Providers
{
    public interface IAdbClientProvider
    {
        public Task<AdbRoot?> GetSingleAdbJobAsync(long runId, string adbWorkspaceUrl);
    }
}