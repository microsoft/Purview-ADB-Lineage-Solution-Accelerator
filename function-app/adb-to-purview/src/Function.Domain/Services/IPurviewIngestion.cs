using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Function.Domain.Services
{
    public interface IPurviewIngestion
    {
        public Task<JArray> SendToPurview(JArray Processes);
        public Task<bool> SendToPurview(JObject json);
    }
}