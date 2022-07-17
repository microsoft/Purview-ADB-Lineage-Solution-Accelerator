using System.Globalization;
using Microsoft.Identity.Web;
using System;
using Newtonsoft.Json;

namespace Function.Domain.Models.Settings
{
    public class AppConfigurationSettings
    {
        public string? APPINSIGHTS_INSTRUMENTATIONKEY { get; set; }
        public string FunctionStorage { get; set; } = "UseDevelopmentStorage=true";
        public string FUNCTIONS_WORKER_RUNTIME { get; set; } = "dotnet-isolated";
        public string AzureWebJobsStorage { get; set; } = "UseDevelopmentStorage=true";
        public string? ListenToMessagesFromEventHub { get; set; }
        public string? SendMessagesToEventHub { get; set; }
        public string? EventHubName { get; set; }
        public string? OlToPurviewMappings { get; set; }
        public string? PurviewAccountName { get; set; }
        public string? ClientID { get; set; }
        public string? ClientSecret { get; set; }
        public string? TenantId { get; set; }
        public string EventHubConsumerGroup { get; set; } = "read";
        public bool usePurviewTypes { get; set; } = false;
        public bool useResourceSet { get; set; } = true;
        public string AuthEndPoint { get; set; } = "https://login.microsoftonline.com/{0}";
        public string Authority
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, AuthEndPoint, TenantId);
            }
        }
        public string AuthenticationUri { get; set; } = "purview.azure.net";
        public string AppDomainUrl { get; set; } = "purview.azure.com";
        public string ResourceUri { get; set; } = "https://purview.azure.com";
        public string purviewApiEndPoint { get; set; } = "{ResourceUri}/catalog/api";
        public string purviewApiEntityBulkMethod { get; set; } = "/atlas/v2/entity/bulk";
        public string purviewApiEntityByGUIDMethod { get; set; } = "/atlas/v2/entity/guid/";
        public string purviewApiEntityByTypeMethod { get; set; } = "/atlas/v2/entity/bulk/uniqueAttribute/type/";
        public string purviewApiEntityQueryMethod { get; set; } = "/search/query?api-version=2021-05-01-preview";
        public string purviewApiSearchAdvancedMethod { get; set; } = "/atlas/v2/search/advanced";
        public int tokenCacheTimeInHours {get;set;} = 6;
        public int dataEntityCacheTimeInSeconds {get;set;} = 60;
        public CertificateDescription? Certificate { get; set; }
        public string purviewAppDomain()
        {
            return AppDomainUrl;
        }
        public string PurviewApiBaseUrl()
        {
            return $"https://{this.PurviewAccountName}.{this.purviewAppDomain()}";
        }
        public bool IsAppUsingClientSecret()
        {
            if(ClientSecret != null)
                return !ClientSecret!.Equals("");
            return false;
        }
        private void ReadSettings()
        {
            foreach (var p in this.GetType().GetProperties())
            {
                var val = Environment.GetEnvironmentVariable(p.Name) ?? "";
                if (!val.Equals(""))
                    if(p.PropertyType == typeof(bool))
                        p.SetValue(this, bool.Parse(val));
                    else
                        if(p.PropertyType == typeof(Microsoft.Identity.Web.CertificateDescription))
                             p.SetValue(this, JsonConvert.DeserializeObject<CertificateDescription>(val));
                        else
                            if(p.PropertyType == typeof(int))
                                p.SetValue(this, int.Parse(val));
                            else
                                p.SetValue(this, val);
            }
        }
        public AppConfigurationSettings()
        {
            ReadSettings();
        }
    }
}