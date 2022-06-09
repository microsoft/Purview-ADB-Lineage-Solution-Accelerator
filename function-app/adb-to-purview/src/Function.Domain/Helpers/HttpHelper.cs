using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace Function.Domain.Helpers
{
    public class HttpHelper : IHttpHelper
    {
        public async Task<HttpResponseData> CreateSuccessfulHttpResponse(HttpRequestData req, object data)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(data);

            return response;
        }
        public HttpResponseData CreateServerErrorHttpResponse(HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);

            return response;
        }
    }
}