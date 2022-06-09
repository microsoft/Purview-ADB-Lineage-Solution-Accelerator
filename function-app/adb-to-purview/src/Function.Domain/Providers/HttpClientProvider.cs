using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Function.Domain.Constants;

/// <summary>
/// HttpClient Manager
/// Class that handles all the Http Client requests 
/// </summary>
namespace Function.Domain.Providers
{
    [ExcludeFromCodeCoverage]
    public class HttpClientManager : IHttpClientManager
    {
        private static readonly HttpClient client;
        private readonly ILogger logger;


        static HttpClientManager()
        {
            client = new HttpClient();
        }

        /// <summary>
        /// HttpClient manager constructor
        /// </summary>
        public HttpClientManager(
            ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task<T?> GetAsync<T>(string url, string accessToken, IDictionary<string, string>? headers = null)
        {
            return await GetAsync<T>(url, new AuthenticationHeaderValue(AuthenticationConstants.Bearer, accessToken), headers);
        }

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<T?> GetAsync<T>(string url, AuthenticationHeaderValue accessToken, IDictionary<string, string>? headers = null)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = accessToken;
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }
                var response = await GetAsync(new Uri(url), accessToken);
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                logger.LogError(nameof(GetAsync) + $"Exception while calling {url}", ex);
            }
            return default;
        }


        /// <summary>
        /// DeleteAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage?> DeleteAsync(Uri url, string accessToken)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await client.DeleteAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(nameof(DeleteAsync) + $"Exception while calling {url}", ex);
            }
            return default;
        }

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAsync(Uri url, string accessToken)
        {
            return await GetAsync(url, new AuthenticationHeaderValue(AuthenticationConstants.Bearer, accessToken));
        }

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAsync(Uri url, AuthenticationHeaderValue accessToken)
        {
            client.DefaultRequestHeaders.Authorization = accessToken;
            return await client.GetAsync(url);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync(Uri url, HttpContent content, string accessToken, IDictionary<string, string>? headers = null)
        {
            var token = new AuthenticationHeaderValue(AuthenticationConstants.Bearer, accessToken);
            return await PostAsync(url, content, token, headers);
        }

        /// <summary>
        /// PostAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync(Uri url, HttpContent content, AuthenticationHeaderValue accessToken, IDictionary<string, string>? headers = null)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = accessToken;
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            return await client.PostAsync(url, content);
        }
    }
}
