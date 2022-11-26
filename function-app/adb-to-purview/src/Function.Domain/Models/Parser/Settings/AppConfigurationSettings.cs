// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using Microsoft.Identity.Web;
using System;
using Newtonsoft.Json;

namespace Function.Domain.Models.Settings
{
    /// <summary>
    /// Read Configurations and set default values
    /// </summary>
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
        public string AuthEndPoint { get; set; } = "https://login.microsoftonline.com/";
        public string Authority
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, AuthEndPoint+"{0}", TenantId);
            }
        }
        public string AuthenticationUri { get; set; } = "purview.azure.net";
        public string AppDomainUrl { get; set; } = "purview.azure.com";
        public string ResourceUri { get; set; } = "https://purview.azure.com";
        public int maxQueryPlanSize {get; set; } = Int32.MaxValue;
        public bool prioritizeFirstResourceSet { get; set; } = true;
        public string purviewApiEndPoint { get; set; } = "{ResourceUri}/catalog/api";
        public string purviewApiEntityBulkMethod { get; set; } = "/atlas/v2/entity/bulk";
        public string purviewApiEntityByGUIDMethod { get; set; } = "/atlas/v2/entity/guid/";
        public string purviewApiEntityByTypeMethod { get; set; } = "/atlas/v2/entity/bulk/uniqueAttribute/type/";
        public string purviewApiEntityQueryMethod { get; set; } = "/search/query?api-version=2021-05-01-preview";
        public string purviewApiSearchAdvancedMethod { get; set; } = "/atlas/v2/search/advanced";
        public int tokenCacheTimeInHours {get;set;} = 6;
        public int dataEntityCacheTimeInSeconds {get;set;} = 60;
        public CertificateDescription? Certificate { get; set; }
        public string? ListenToMessagesFromPurviewKafka { get; set; }
        public string KafkaName { get; set; } = "atlas_entities";
        public string KafkaConsumerGroup { get; set; } = "$Default";
        public string ResourceSet { get; set; }= "azure_datalake_gen2_resource_set;azure_blob_resource_set";
        public string Spark_Entities { get; set; }= "databricks_workspace;databricks_job;databricks_notebook;databricks_notebook_task";
        public string Spark_Process { get; set; }= "databricks_process";
        public string? FilterExeption { get; set; }= "{\"attributes\":[{\"typeName\":\"column\"},{\"typeName\":\"tabular_schema\"},{\"typeName\":\"spark_process\"},{\"typeName\":\"spark_application\"},{\"typeName\":\"databricks*\"}]}";
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
        private string[] resourceSet={};
        public bool IsResourceSet_Entity(string entityName)
        {
            if (resourceSet.Length == 0)
                resourceSet = this.ResourceSet.Split(";");
            var findTypeName = Array.Find<string>(resourceSet!, element => element.Equals(entityName));
            if (findTypeName == entityName)
                return true;
            return false;
        }
        public AppConfigurationSettings()
        {
            ReadSettings();
        }
    }
}
