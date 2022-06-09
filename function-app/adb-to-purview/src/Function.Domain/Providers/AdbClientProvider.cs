using System;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Function.Domain.Models.Adb;

namespace Function.Domain.Providers
{
    /// <summary>
    /// Provider for REST API calls to the ADB API.
    /// </summary>
    class AdbClientProvider : IAdbClientProvider
        {
        private string _tenantId;
        private string _clientId;
        private string _clientSecret;

        // static for simple function cache
        private static JwtSecurityToken? _managementToken;
        private static JwtSecurityToken? _bearerToken;
        private HttpClient _client;
        private ILogger _log;

        /// <summary>
        /// Constructs the AdbClientProvider object from the Function framework using DI
        /// </summary>
        /// <param name="loggerFactory">Logger Factory to support DI from function framework or code calling helper classes</param>
        /// <param name="config">Function framwork config from DI</param>
        public AdbClientProvider(ILoggerFactory loggerFactory, IConfiguration config)
        {
            _log = loggerFactory.CreateLogger<AdbClientProvider>();
            _client  = new HttpClient();
            _tenantId = config["TenantId"];
            _clientId = config["ClientId"];
            _clientSecret = config["ClientSecret"];
        }
        private async Task GetBearerTokenAsync()
        {
            try {
                var BaseAddress = $"https://login.microsoftonline.com/{_tenantId}/oauth2/v2.0/token";
                var form = new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"},
                    {"scope", "2ff814a6-3304-4ab8-85cb-cd0e6f879c1d/.default"},
                    {"client_id", _clientId },
                    {"client_secret", _clientSecret}
                };

                var tokenResponse = await _client.PostAsync(BaseAddress, new FormUrlEncodedContent(form));

                tokenResponse.EnsureSuccessStatusCode();
                JObject? resultjson = JObject.Parse(tokenResponse.Content.ReadAsStringAsync().Result);
                if (resultjson is not null)
                {
                    _bearerToken = new JwtSecurityToken((resultjson.SelectToken("access_token") ?? "").ToString());
                }
            }
            catch (Exception ex) 
            {
                _log.LogError(ex, $"AdbClient-GetBearerTokenAsync: error, message: {ex.Message}");
            }
        }
        private async Task GetManagementTokenAsync()
        {
            try {
                var BaseAddress = $"https://login.microsoftonline.com/{_tenantId}/oauth2/token";
                var form = new Dictionary<string, string>
                {
                    {"Authorization: Bearer", _bearerToken!.RawData},
                    {"grant_type", "client_credentials"},
                    {"resource", "https://management.core.windows.net/"},
                    {"client_id", _clientId },
                    {"client_secret", _clientSecret}
                };

                var tokenResponse = await _client.PostAsync(BaseAddress, new FormUrlEncodedContent(form));

                tokenResponse.EnsureSuccessStatusCode();
                JObject? resultjson = JObject.Parse(tokenResponse.Content.ReadAsStringAsync().Result);
                _managementToken = new JwtSecurityToken((resultjson.SelectToken("access_token") ?? "").ToString());
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"AdbClient-GetManagementTokenAsync: error, message: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets information about Azure Databricks Jobs
        /// </summary>
        /// <param name="runId">The ADB runId for the Job</param>
        /// <param name="adbWorkspaceUrl">The first part of the ADB workspace URL.  E.g. adb-000000000000000.0</param>
        /// <returns></returns>
        public async Task<AdbRoot?> GetSingleAdbJobAsync(long runId, string adbWorkspaceUrl)
        {
            if (isTokenExpired(_bearerToken))
            {
                await GetBearerTokenAsync();
                
                if (_bearerToken is null)
                {
                    _log.LogError("AdbClient-GetSingleAdbJobAsync: unable to get bearer token");
                    return null;
                }
            }

            if (isTokenExpired(_managementToken))
            {
                await GetManagementTokenAsync();

                if (_managementToken is null) 
                {
                    _log.LogError("AdbClient-GetSingleAdbJobAsync: unable to get management token");
                    return null;
                }
            }

            var request = new HttpRequestMessage() {
                RequestUri = new Uri($"https://{adbWorkspaceUrl}.azuredatabricks.net/api/2.1/jobs/runs/get?run_id={runId}"),
                Method = HttpMethod.Get,
            };
            request.Headers.Authorization  =
                new AuthenticationHeaderValue("Bearer", _bearerToken!.RawData);
            request.Headers.TryAddWithoutValidation("X-Databricks-Azure-SP-Management-Token", _managementToken!.RawData);

            AdbRoot? resultAdbRoot = null;
           try {
                var tokenResponse = await _client.SendAsync(request);

                tokenResponse.EnsureSuccessStatusCode();
                resultAdbRoot = JsonConvert.DeserializeObject<AdbRoot>(await tokenResponse.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"AdbClient-GetSingleAdbJobAsync: error, message: {ex.Message}");
            }
            return resultAdbRoot;
        }
        private bool isTokenExpired(JwtSecurityToken? jwt)
        {
            if (jwt is null)
            {
                return true;
            }
            if (jwt.ValidTo > DateTime.Now.AddMinutes(3))
            {
                _log.LogInformation("AdbClient-isTokenExpired: Token cache hit");
                return false;
            }
            return true;
        }
    }
}