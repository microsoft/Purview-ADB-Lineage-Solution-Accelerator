using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

namespace Function.Domain.Helpers
{
    public interface IHttpHelper
    {
         public Task<HttpResponseData> CreateSuccessfulHttpResponse(HttpRequestData req, object data);
         public HttpResponseData CreateServerErrorHttpResponse(HttpRequestData req);
    }
}