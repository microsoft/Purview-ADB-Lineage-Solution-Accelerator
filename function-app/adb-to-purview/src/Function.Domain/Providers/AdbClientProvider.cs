// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
using Function.Domain.Models.Settings;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace Function.Domain.Providers
{
    /// <summary>
    /// Provider for REST API calls to the ADB API.
    /// </summary>
    class AdbClientProvider : IAdbClientProvider
        {
        private AppConfigurationSettings? config = new AppConfigurationSettings();

        // static for simple function cache
        private static JwtSecurityToken? _bearerToken;
        private HttpClient _client;
        private ILogger _log;

        /// <summary>
        /// Constructs the AdbClientProvider object from the Function framework using DI
        /// </summary>
        /// <param name="loggerFactory">Logger Factory to support DI from function framework or code calling helper classes</param>
        /// <param name="config">Function framework config from DI</param>
        public AdbClientProvider(ILoggerFactory loggerFactory, IConfiguration config)
        {
            _log = loggerFactory.CreateLogger<AdbClientProvider>();
            _client  = new HttpClient();
        }
        private async Task GetBearerTokenAsync()
        {
            // Even if this is a console application here, a daemon application is a confidential client application
            IConfidentialClientApplication app;

            if (config!.IsAppUsingClientSecret())
            {
                // Even if this is a console application here, a daemon application is a confidential client application
                app = ConfidentialClientApplicationBuilder.Create(config.ClientID)
                    .WithClientSecret(config.ClientSecret)
                    .WithAuthority(new Uri(config.Authority))
                    .Build();
            }
            else
            {
                ICertificateLoader certificateLoader = new DefaultCertificateLoader();
                certificateLoader.LoadIfNeeded(config!.Certificate!);

                app = ConfidentialClientApplicationBuilder.Create(config.ClientID)
                    .WithCertificate(config!.Certificate!.Certificate)
                    .WithAuthority(new Uri(config.Authority))
                    .Build();
            }

            string[] scopes = new string[] { "2ff814a6-3304-4ab8-85cb-cd0e6f879c1d/.default" };

            AuthenticationResult? result;
            try
            {
                foreach(string s in scopes){
                    _log.LogInformation(s);
                }
                _log.LogInformation(config.ClientID);
                _log.LogInformation(config.Authority);
                
                result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                // Mitigation: change the scope to be as expected
                _log.LogError("Error getting Authentication Token for Databricks API");
                return;
            }
            catch (Exception coreex)
            {

                _log.LogError($"Error getting Authentication Token for Databricks API");
                _log.LogError(coreex.Message);
                return;
            }

            _bearerToken = new JwtSecurityToken(result.AccessToken);

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

            var request = new HttpRequestMessage() {
                RequestUri = new Uri($"https://{adbWorkspaceUrl}.databricks.azure.cn/api/2.1/jobs/runs/get?run_id={runId}"),
                Method = HttpMethod.Get,
            };
            request.Headers.Authorization  =
                new AuthenticationHeaderValue("Bearer", _bearerToken!.RawData);

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