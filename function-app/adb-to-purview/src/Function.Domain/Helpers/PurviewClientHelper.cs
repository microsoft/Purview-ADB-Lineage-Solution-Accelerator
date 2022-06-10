using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Dynamic;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Function.Domain.Providers;
using Function.Domain.Models;
using Function.Domain.Constants;
using Newtonsoft.Json.Linq;
using Function.Domain.Models.Purview;

namespace Function.Domain.Helpers
{

    /// <summary>
    /// Helper Class that Connect to HTTP Service used to call Microsoft Purview API
    /// </summary>
    public class PurviewClientHelper : IPurviewClientHelper
    {
        private readonly IHttpClientManager httpClientManager;
        private readonly ILogger logger;
        private int postretries;
        private int numofretries = 1;

        /// <summary>
        /// Create Object with IHttpClientManager and ILogger
        /// </summary>
        /// <param name="httpClientManager"></param>
        /// <param name="logger"></param>
        public PurviewClientHelper(
          IHttpClientManager httpClientManager,
          ILogger logger
          )
        {
            if (!int.TryParse(Environment.GetEnvironmentVariable("PostAPICallsRetries"), out postretries))
                postretries = 3;
            this.httpClientManager = httpClientManager;
            this.logger = logger;
        }

        /// <summary>
        /// Post APIs request to Microsoft Purview API
        /// </summary>
        /// <param name="correlationId">Correlation ID to be used in the logs</param>
        /// <param name="token">Token to access API</param>
        /// <param name="batchUpdatePayload">Data to send to API</param>
        /// <param name="bulkUpdateEndpoint">API end Point</param>
        /// <returns>HttpResponseMessage</returns>
        public async Task<HttpResponseMessage> PostEntitiesToPurview(string correlationId, string token, dynamic batchUpdatePayload, string bulkUpdateEndpoint)
        {

            string payloadString = batchUpdatePayload.ToString();
            try
            {
                var response = await httpClientManager.PostAsync(new Uri(bulkUpdateEndpoint), new StringContent(payloadString, Encoding.UTF8, HttpConstants.ContentTypeJson), token);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode.ToString().ToLower() == "requesttimeout")
                    {
                        if (numofretries <= postretries)
                        {
                            numofretries++;
                            return await PostEntitiesToPurview(correlationId, token, batchUpdatePayload, bulkUpdateEndpoint);
                        }
                    }
                    numofretries = 1;
                    logger.LogError("Purview Publish Entity Metadata Error : Error :" + responseContent);
                    return response;
                }
                else
                {
                    numofretries = 1;
                    logger.LogInformation("Purview Entity Updated Successfully ");
                    return response;
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Purview Publish Entity Metadata Error" + ex.Message);
                HttpResponseMessage? msgreturn = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                numofretries = 1;
                return msgreturn;
            }
        }

        /// <summary>
        /// Post APIs request to Microsoft Purview API
        /// </summary>
        /// <param name="correlationId">Correlation ID to be used in the logs</param>
        /// <param name="qualifiedName">Qualified Name to search by</param>
        /// <param name="bulkUpdateEndpoint">API end Point</param>
        /// <param name="token">Token to access API</param>
        /// <returns>PurviewEntitySearchResponseModel</returns>
        public async Task<PurviewEntitySearchResponseModel> GetEntitiesFromPurview(string correlationId, string qualifiedName, string purviewSearchEndpoint, string token)
        {
            PurviewEntitySearchResponseModel entityObjectModel = new PurviewEntitySearchResponseModel();
            entityObjectModel.entities = new List<EntityModel>();
            try
            {
                var searchContent = JsonConvert.SerializeObject(new { keyword = qualifiedName, limit = PurviewAPIConstants.DefaultSearchLimit, offset = 0 }).ToString();
                var response = await httpClientManager.PostAsync(new Uri(purviewSearchEndpoint), new StringContent(searchContent, Encoding.UTF8, HttpConstants.ContentTypeJson), token);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.ReasonPhrase != "OK")
                {
                    if (responseContent == null)
                    {
                        logger.LogError($"Error Loading to Purview: Return Code: {response.StatusCode} - Reason:{response.ReasonPhrase}");
                        numofretries = 1;
                        return entityObjectModel;
                    }
                    if (response.StatusCode.ToString().ToLower() == "requesttimeout")
                    {
                        if (numofretries <= postretries)
                        {
                            numofretries++;
                            return await GetEntitiesFromPurview(correlationId, qualifiedName, purviewSearchEndpoint, token);
                        }
                    }
                    numofretries = 1;
                    logger.LogError($"Error Loading to Purview: Return Code: {response.StatusCode} - Reason:{response.ReasonPhrase} - Message:{responseContent}");
                    return entityObjectModel;
                }


                string responseContentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var entitiesFromPurview = JsonConvert.DeserializeObject<PurviewEntitySearchResponseModel>(responseContentString) ?? new PurviewEntitySearchResponseModel();
                logger.LogInformation($"Purview Loaded Processies: Return Code: {response.StatusCode} - Reason:{response.ReasonPhrase} - Content: {responseContentString}");

                if (entitiesFromPurview.entities != null)
                {
                    foreach (var entity in entitiesFromPurview.entities)
                    {
                        if (entity.qualifiedName.ToString().ToLower().Trim('/') == qualifiedName.ToLower().Trim('/'))
                        {
                            entityObjectModel.entities.Add(entity);
                        }
                    }
                }
                numofretries = 1;
                return entityObjectModel;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                numofretries = 1;
                return entityObjectModel;
            }

        }

        /// <summary>
        /// Post APIs request to Microsoft Purview API, only used for query API
        /// </summary>
        /// <param name="correlationId">Correlation ID to be used in the logs</param>
        /// <param name="token">Token to access API</param>
        /// <param name="purviewSearchEndpoint">API end Point</param>
        /// <param name="filterValue">Filter to be applyed on the search</param>
        /// <returns>List<QueryValeuModel></returns>
        public async Task<List<QueryValeuModel>> QueryEntities(string correlationId, string token, string purviewSearchEndpoint, object filterValue)
        {
            PurviewQueryResponseModel entityObjectModel = new PurviewQueryResponseModel();
            List<QueryValeuModel> entities = new List<QueryValeuModel>();
            int offset = 0;
            int totalEntities = 1000;
            int numberEntities = 1;
            bool printNumberEntitiesOnSearch = true;
            try
            {
                while (totalEntities > numberEntities)
                {
                    var searchContent = JsonConvert.SerializeObject(new { limit = PurviewAPIConstants.DefaultSearchLimit, offset = offset, filter = filterValue }).ToString();
                    var response = await httpClientManager.PostAsync(new Uri(purviewSearchEndpoint), new StringContent(searchContent, Encoding.UTF8, HttpConstants.ContentTypeJson), token);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.ReasonPhrase != "OK")
                    {
                        if (responseContent == null)
                        {
                            logger.LogError($"Error Loading to Purview: Return Code: {response.StatusCode} - Reason:{response.ReasonPhrase}");
                            numofretries = 1;
                            return entities;
                        }
                        if (response.StatusCode.ToString().ToLower() == "requesttimeout")
                        {
                            if (numofretries <= postretries)
                            {
                                numofretries++;
                                return await QueryEntities(correlationId, token, purviewSearchEndpoint, filterValue);
                            }
                        }
                        numofretries = 1;
                        return entities;
                    }

                    string responseContentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    logger.LogInformation($"Purview Loaded Processies: Return Code: {response.StatusCode} - Reason:{response.ReasonPhrase} - Content: {responseContentString}");

                    entityObjectModel = JsonConvert.DeserializeObject<PurviewQueryResponseModel>(responseContentString) ?? new PurviewQueryResponseModel();
                    JToken responsetoken = JToken.Parse(responseContent);
                    string responseCount = responsetoken["@search.count"]!.ToString();

                    totalEntities = Int32.Parse(responseCount ?? "0");
                    if (printNumberEntitiesOnSearch)
                    {
                        logger.LogInformation($"Search {purviewSearchEndpoint} content {searchContent}");
                        logger.LogInformation($"{totalEntities} Entities found");
                    }
                    numberEntities = numberEntities + 1000;
                    offset = numberEntities;
                    if (entityObjectModel.entities.Count > 0)
                        entities.AddRange(entityObjectModel.entities);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                numofretries = 1;
            }
            numofretries = 1;
            return entities;
        }

        /// <summary>
        /// Post APIs request to Microsoft Purview API, only used for advaced search API
        /// </summary>
        /// <param name="correlationId">Correlation ID to be used in the logs</param>
        /// <param name="qualifiedName">Qualified Name to search by</param>
        /// <param name="purviewSearchEndpoint">API end Point</param>
        /// <param name="token">Token to access API</param>
        /// <param name="filterValue">Filter to be applyed on the search</param>
        /// <returns>List<QueryValeuModel></returns>
        public async Task<PurviewEntitySearchResponseModel> GetEntitiesFromPurview(string correlationId, string qualifiedName, string purviewSearchEndpoint, string token, object filterValue)
        {
            PurviewEntitySearchResponseModel entityObjectModel = new PurviewEntitySearchResponseModel();
            entityObjectModel.entities = new List<EntityModel>();
            int offset = 0;
            int totalEntities = 1000;
            int numberEntities = 1;
            bool printNumberEntitiesOnSearch = true;
            try
            {
                while (totalEntities > numberEntities)
                {
                    var searchContent = JsonConvert.SerializeObject(new { keyword = qualifiedName, limit = PurviewAPIConstants.DefaultSearchLimit, offset = offset, filter = filterValue }).ToString();
                    var response = await httpClientManager.PostAsync(new Uri(purviewSearchEndpoint), new StringContent(searchContent, Encoding.UTF8, HttpConstants.ContentTypeJson), token);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.ReasonPhrase != "OK")
                    {
                        if (responseContent == null)
                        {
                            logger.LogError($"Error Loading to Purview: Return Code: {response.StatusCode} - Reason:{response.ReasonPhrase}");
                            numofretries = 1;
                            return entityObjectModel;
                        }
                        if (response.StatusCode.ToString().ToLower() == "requesttimeout")
                        {
                            if (numofretries <= postretries)
                            {
                                numofretries++;
                                return await GetEntitiesFromPurview(correlationId, qualifiedName, purviewSearchEndpoint, token, filterValue);
                            }
                        }
                        numofretries = 1;
                        logger.LogError($"Error Loading to Purview: Return Code: {response.StatusCode} - Reason:{response.ReasonPhrase} - Message:{responseContent}");
                        return entityObjectModel;
                    }


                    string responseContentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    logger.LogInformation($"Purview Loaded Processies: Return Code: {response.StatusCode} - Reason:{response.ReasonPhrase} - Content: {responseContentString}");

                    var entitiesFromPurview = JsonConvert.DeserializeObject<PurviewEntitySearchResponseModel>(responseContentString) ?? new PurviewEntitySearchResponseModel();
                    JToken responsetoken = JToken.Parse(responseContent);
                    string responseCount = responsetoken["@search.count"]!.ToString();

                    totalEntities = Int32.Parse(responseCount ?? "0");
                    if (printNumberEntitiesOnSearch)
                    {
                        logger.LogInformation($"Search {purviewSearchEndpoint} content {searchContent}");
                        logger.LogInformation($"{totalEntities} Entities found on search for: {qualifiedName}");
                    }
                    numberEntities = numberEntities + 1000;
                    offset = numberEntities;
                    if (entitiesFromPurview.entities != null)
                    {
                        foreach (var entity in entitiesFromPurview.entities)
                        {
                            if ((entity.entityType == "azure_datalake_gen2_resource_set") || (entity.entityType == "azure_blob_resource_set"))
                            {
                                //logger.LogInformation($"Looking for: {qualifiedName.ToLower().Trim('/')} on {entity.qualifiedName.ToString().ToLower()} IndexOf");
                                if (entity.qualifiedName.ToString().ToLower().IndexOf(qualifiedName.ToLower().Trim('/')) > -1)
                                {
                                    entityObjectModel.entities.Add(entity);
                                    numofretries = 1;
                                    return entityObjectModel;
                                }
                                //logger.LogInformation($"Looking for: {qualifiedName.ToLower().Replace("blob", "dfs").Trim('/')} on {entity.qualifiedName.ToString().ToLower()} IndexOf");
                                if (entity.qualifiedName.ToString().ToLower().IndexOf(qualifiedName.ToLower().Replace("blob", "dfs").Trim('/')) > -1)
                                {
                                    entityObjectModel.entities.Add(entity);
                                    numofretries = 1;
                                    return entityObjectModel;
                                }
                            }
                            else
                            {
                                //logger.LogInformation($"Looking for: {qualifiedName.ToLower().Trim('/')} on {entity.qualifiedName.ToString().ToLower().Trim('/')} Iqual");
                                if (entity.qualifiedName.ToString().ToLower().Trim('/') == qualifiedName.ToLower().Trim('/'))
                                {
                                    entityObjectModel.entities.Add(entity);
                                    numofretries = 1;
                                    return entityObjectModel;
                                }
                                //logger.LogInformation($"Looking for: {qualifiedName.ToLower().Replace("blob", "dfs").Trim('/')} on {entity.qualifiedName.ToString().ToLower().Trim('/')} Iqual");
                                if (entity.qualifiedName.ToString().ToLower().Trim('/') == qualifiedName.ToLower().Replace("blob", "dfs").Trim('/'))
                                {
                                    entityObjectModel.entities.Add(entity);
                                    numofretries = 1;
                                    return entityObjectModel;
                                }
                            }
                        }
                    }
                }
                numofretries = 1;
                return entityObjectModel;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                numofretries = 1;
                return entityObjectModel;
            }

        }

        /// <summary>
        /// GET APIs request to Microsoft Purview API
        /// </summary>
        /// <param name="correlationId">Correlation ID to be used in the logs</param>
        /// <param name="qualifiedName">Qualified Name to search by</param>
        /// <param name="purviewSearchEndpoint">API end Point</param>
        /// <param name="token">Token to access API</param>
        /// <param name="typeName">Type to search by</param>
        /// <returns>List<Asset></returns>
        public async Task<List<Asset>> GetEntityFromPurview(string correlationId, string qualifiedName, string purviewSearchEndpoint, string token, string typeName)
        {
            List<Asset> entities = new List<Asset>();
            Dictionary<string, string> postHeaders = new Dictionary<string, string>();
            postHeaders.Add("Accept", "*/*");
            postHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            postHeaders.Add("Connection", "keep-alive");
            try
            {
                string finalURL = $"{purviewSearchEndpoint}{typeName}?ignoreRelationships=false&attr_N:qualifiedName={qualifiedName}";
                var response = await httpClientManager.GetAsync(new Uri(finalURL), token);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.ReasonPhrase != "OK")
                {
                    if (responseContent == null)
                    {
                        logger.LogError($"Error Loading to Purview: Return Code: {response.StatusCode} - Reason:{response.ReasonPhrase}");
                        numofretries = 1;
                        return entities;
                    }
                    if (response.StatusCode.ToString().ToLower() == "requesttimeout")
                    {
                        if (numofretries <= postretries)
                        {
                            numofretries++;
                            return await GetEntityFromPurview(correlationId, qualifiedName, purviewSearchEndpoint, token, typeName);
                        }
                    }
                    numofretries = 1;
                    logger.LogError($"Error Loading to Purview: Return Code: {response.StatusCode} - Reason:{response.ReasonPhrase} - Message:{responseContent}");
                    return entities;
                }


                string responseContentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var entitiesFromPurview = JsonConvert.DeserializeObject<AtlasEntityWithExtInfo>(responseContentString) ?? new AtlasEntityWithExtInfo();

                if (entitiesFromPurview.entities != null)
                {
                    foreach (var entity in entitiesFromPurview.entities)
                    {
                        if (entity.attributes!["qualifiedName"]!.ToString()!.ToLower().Trim('/') == qualifiedName.ToLower().Trim('/'))
                        {
                            entities.Add(entity);
                        }
                    }
                }
                numofretries=1;
                return entities;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                numofretries=1;
                return entities;
            }

        }

        /// <summary>
        /// DELETE APIs request to Microsoft Purview API
        /// </summary>
        /// <param name="correlationId">Correlation ID to be used in the logs</param>
        /// <param name="token">Token to access API</param>
        /// <param name="entityGuid">Entity GUID in Microsoft Purview</param>
        /// <param name="purviewSearchEndpoint">API end Point</param>
        /// <returns>Nothing</returns>
        public async Task DeleteEntityByGuidInPurview(string correlationId, string token, string entityGuid, string purviewDeleteEndPoint)
        {
            string finalURL = $"{purviewDeleteEndPoint}{entityGuid}";
            try
            {
                var response = await httpClientManager.DeleteAsync(new Uri(finalURL), token);
                bool responseSuccessCode = response?.IsSuccessStatusCode ?? false;
                if (!responseSuccessCode)
                {
                    logger.LogError("Unable to Delete Entity " + entityGuid);
                    if ((response?.StatusCode.ToString().ToLower()?? "") == "requesttimeout")
                    {
                        if (numofretries <= postretries)
                        {
                            numofretries++;
                            await DeleteEntityByGuidInPurview(correlationId,token,entityGuid,purviewDeleteEndPoint);
                        }
                    }
                    numofretries = 1;
                }
                else
                {
                    logger.LogInformation("Entity Delete Successful ");
                    numofretries = 1;
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Purview Delete Processor Error " + ex.Message);
                numofretries = 1;
            }
        }


        /// <summary>
        /// GET APIs request to Microsoft Purview API
        /// </summary>
        /// <param name="token">Token to access API</param>
        /// <param name="purviewEndPoint">API end Point</param>
        /// <param name="entityGuid">Entity GUID in Microsoft Purview</param>
        /// <returns>JObject</returns>
        public async Task<JObject> GetEntityByGuid(string token, string purviewEndPoint, string entityGuid)
        {
            string finalURL = $"{purviewEndPoint}{entityGuid}";
            try
            {
                var response = await httpClientManager.GetAsync(new Uri(finalURL), token);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.ReasonPhrase != "OK")
                {
                    if (responseContent == null)
                    {
                        logger.LogError($"Error Loading to Purview: Return Code: {response.StatusCode} - Reason:{response.ReasonPhrase}");
                        numofretries = 1;
                        return new JObject();
                    }
                    if ((response?.StatusCode.ToString().ToLower()?? "") == "requesttimeout")
                    {
                        if (numofretries <= postretries)
                        {
                            numofretries++;
                            return await GetEntityByGuid(token,purviewEndPoint,entityGuid);
                        }
                    }
                    numofretries = 1;
                    logger.LogError($"Error Loading to Purview: Return Code: {response?.StatusCode} - Reason:{response?.ReasonPhrase} - Message:{responseContent}");
                    return new JObject();
                }


                string responseContentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var entitiesFromPurview = JsonConvert.DeserializeObject<JObject>(responseContentString) ?? new JObject();
                numofretries = 1;
                return entitiesFromPurview;
            }
            catch (Exception ex)
            {
                numofretries = 1;
                logger.LogError(ex, "Purview Delete Processor Error");
            }
            numofretries = 1;
            return new JObject();
        }
    }
}
