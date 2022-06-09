using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Function.Domain.Models;
using Function.Domain.Models.Purview;

namespace Function.Domain.Helpers
{
    public interface IPurviewClientHelper
    {
        public Task<HttpResponseMessage> PostEntitiesToPurview(string correlationId, string token, dynamic batchUpdatePayload, string bulkUpdateEndpoint);
        public Task<PurviewEntitySearchResponseModel> GetEntitiesFromPurview(string correlationId, string qualifiedName, string purviewSearchEndpoint, string token);
        public Task DeleteEntityByGuidInPurview(string correlationId, string token, string entityGuid, string purviewDeleteEndPoint);
        public Task<List<Asset>> GetEntityFromPurview(string correlationId, string qualifiedName, string purviewSearchEndpoint, string token, string typeName);
    }
}
