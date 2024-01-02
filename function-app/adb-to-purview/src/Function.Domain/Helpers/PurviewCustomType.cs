// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Function.Domain.Providers;
using Function.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Function.Domain.Models.Purview;
using System.Collections.Generic;
using Function.Domain.Models.Settings;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Runtime.Caching;

namespace Function.Domain.Helpers
{
    /// <summary>
    /// Class responsible for the interaction with Purview API
    /// </summary>
    public class PurviewCustomType
    {
        private readonly ILogger _logger;
        private readonly string EntityType = "purview_custom_connector_generic_entity_with_columns";
        private PurviewClient _client;
        private JObject? properties;
        private AppConfigurationSettings? config = new AppConfigurationSettings();
        public JObject? Fullentity = new JObject();
        bool useResourceSet = true;
        public string? originalQualifiedName {get; private set;}
        /// <summary>
        /// Property that contains all Json attributes for the Custom data Entity in Microsoft Purview
        /// </summary>
        public JObject Properties
        {
            get { return properties!; }
        }
        /// <summary>
        /// Get the current qualifedName
        /// </summary>
        public string currentQualifiedName()
        {
            return properties!["attributes"]!["qualifiedName"]!.ToString();
        }
        /// <summary>
        /// Creation of a Microsoft Purview Custom Type entity that initialize all attributes needed
        /// </summary>
        /// <param name="name">Name of the Data Entity</param>
        /// <param name="typeName">Type of the Data Entity</param>
        /// <param name="qualified_name">Qualified Name  of the Data Entity</param>
        /// <param name="data_type">Original Type in case Data entity can't be found (Not scanned yet by Microsoft Purview)</param>
        /// <param name="description">Description  of the Data Entity</param>
        /// <param name="guid">GUID  of the Data Entity</param>
        /// <param name="logger">Hand to the logger to be used globally to log information during execution</param>
        /// <param name="client">Client to connect to Microsoft Purview API</param>
        public PurviewCustomType(string name, string typeName, string qualified_name, string data_type, string description, Int64 guid, ILogger logger, PurviewClient client)
        {
            _logger = logger;
            _client = client;
            useResourceSet = config!.useResourceSet;

            Init(name
            , typeName
            , qualified_name
            , data_type
            , description
            , guid
            );
            _logger.LogInformation($"New Entity Initialized in the process with a passed Purview Client: Nome:{name} - qualified_name:{qualified_name} - Type: {typeName} - Guid:{guid}");
        }
        /// <summary>
        /// Creation of a Microsoft Purview Custom Type entity that initialize all attributes needed
        /// </summary>
        /// <param name="name">Name of the Data Entity</param>
        /// <param name="typeName">Type of the Data Entity</param>
        /// <param name="qualified_name">Qualified Name  of the Data Entity</param>
        /// <param name="data_type">Original Type in case Data entity can't be found (Not scanned yet by Microsoft Purview)</param>
        /// <param name="description">Description  of the Data Entity</param>
        /// <param name="guid">GUID  of the Data Entity</param>
        /// <param name="logger">Hand to the logger to be used globally to log information during execution</param>
        public PurviewCustomType(string name, string typeName, string qualified_name, string data_type, string description, Int64 guid, ILogger logger)
        {
            _logger = logger;
            _client = new PurviewClient(_logger!);
            Init(name
            , typeName
            , qualified_name
            , data_type
            , description
            , guid
            );
            _logger.LogInformation($"New Entity Initialized in the process with internal Purview Client: Nome:{name} - qualified_name:{qualified_name} - Guid:{guid}");
        }
        /// <summary>
        /// Validate if the Data entity is a Spark entity
        /// </summary>
        /// <param name="typeName">Type name</param>
        /// <returns>boolean</returns>
        public bool IsSpark_Entity(string typeName)
        {
            string[]? spark_entities = config!.Spark_Entities!.Split(";");
            var findTypeName = Array.Find<string>(spark_entities!, element => element.Equals(typeName));
            if (findTypeName == typeName)
                return true;
            return false;
        }
        /// <summary>
        /// Validate if the entity is a Spark Process
        /// </summary>
        /// <param name="typeName">Type name</param>
        /// <returns>boolean</returns>
        public bool IsSpark_Process(string typeName)
        {
            string[]? spark_entities = config!.Spark_Process.Split(";");
            var findTypeName = Array.Find<string>(spark_entities!, element => element.Equals(typeName));
            if (findTypeName == typeName)
                return true;
            return false;
        }

        /// <summary>
        /// Validate if the entity is a Blob or Data Lake entity type but not a resource set
        /// </summary>
        /// <param name="typeName">Type name</param>
        /// <returns>boolean</returns>
        public bool IsBlobOrDataLakeFS_Entity(string typeName)
        {
            string typeNameLowercase = typeName.ToLower();
            return ((typeNameLowercase == "azure_blob_path") || (typeNameLowercase == "azure_datalake_gen2_path") || (typeNameLowercase == "azure_datalake_gen2_filesystem"));
        }

        /// <summary>
        /// Validate if the entity is a Blob or Data Lake resource type but not a file path or system type
        /// </summary>
        /// <param name="typeName">Type name</param>
        /// <returns>boolean</returns>
        public bool IsBlobOrDataLakeResourceSet_Entity(string typeName)
        {
            string typeNameLowercase = typeName.ToLower();
            return ((typeNameLowercase == "azure_datalake_gen2_resource_set") || (typeNameLowercase == "azure_blob_resource_set"));
        }

        private void Init(string name, string typeName, string qualified_name, string data_type, string description, Int64 guid)
        {
            is_dummy_asset = false;
            properties = new JObject();
            originalQualifiedName = qualified_name;
            //Loading Entity properties into Json format of an AtlasEntity
            properties.Add("typeName", typeName);
            properties.Add("guid", guid);
            properties.Add("attributes", new JObject());
            properties.Add("relationshipAttributes", new JObject());
            ((JObject)properties["attributes"]!).Add("name", name);
            ((JObject)properties["attributes"]!).Add("qualifiedName", qualified_name);
            ((JObject)properties["attributes"]!).Add("data_type", data_type);
            ((JObject)properties["attributes"]!).Add("description", description);
        }

        /// <summary>
        /// Get a list on Data Entities in Microsoft Purview using Entity Qualified Name and Type
        /// </summary>
        /// <returns>List of Asset</returns>
        public async Task<List<Asset>> GetEntity()
        {
            var entities = await _client.Get_Entity(properties!["attributes"]!["qualifiedName"]!.ToString()
                , properties!["typeName"]!.ToString());

            return entities;
        }

        /// <summary>
        /// Validate if the entity is not found in Microsoft Purview
        /// </summary>
        /// <value>Boolean</value>
        public bool is_dummy_asset { get; set; }

        private List<string>? qNames = new List<string>();
        /// <summary>
        /// Search the entity in Microsoft Purview using Query API, only using Qualified Name
        /// </summary>
        /// <returns>QueryValeuModel</returns>
        public async Task<QueryValeuModel> QueryInPurview()
        {
            return await QueryInPurview(string.Empty);
        }
        /// <summary>
        /// Search the entity in Microsoft Purview using Query API, using Qualified Name and Type
        /// </summary>
        /// <TypeName>Type to search By</TypeName>
        /// <returns>QueryValeuModel</returns>
        public async Task<QueryValeuModel> QueryInPurview(string TypeName)
        {
            QueryValeuModel obj = new QueryValeuModel();
            qNames = Build_Searchable_QualifiedName(this.properties!["attributes"]!["qualifiedName"]!.ToString());
            string itemType = this.properties!["typeName"]!.ToString();
            string itemName = this.properties!["attributes"]!["name"]!.ToString();
            JObject filter = new JObject();
            this.is_dummy_asset = true;

            foreach (string name in qNames!)
            {
                if (!filter.ContainsKey("filter"))
                {
                    filter.Add("filter", new JObject());
                    ((JObject)filter!["filter"]!).Add("and", new JArray());
                }
                //Support for the cases when user use wasbs protocol for a ADLS GEN 2
                if ((name.Contains(".dfs.core.windows.net")) || (name.Contains(".blob.core.windows.net")))
                {
                    string orName = name.Contains(".dfs.core.windows.net") ? name.Replace(".dfs.core.windows.net", ".blob.core.windows.net") : name.Replace(".blob.core.windows.net", ".dfs.core.windows.net");
                    var orcondition = new JObject();
                    orcondition.Add("or", new JArray());
                    var condition = new JObject();
                    condition.Add("attributeName", "qualifiedName");
                    condition.Add("operator", "contains");
                    condition.Add("attributeValue", name);
                    ((JArray)((JObject)orcondition!)["or"]!).Add(condition);
                    condition = new JObject();
                    condition.Add("attributeName", "qualifiedName");
                    condition.Add("operator", "contains");
                    condition.Add("attributeValue", orName);
                    ((JArray)((JObject)orcondition!)["or"]!).Add(condition);
                    ((JArray)((JObject)filter!["filter"]!)["and"]!).Add(orcondition);
                }
                //Support for the case for hive managed tables
                else if ((name.Contains("@adb-")) && (name.TrimEnd().EndsWith(".azuredatabricks.net")))
                {
                    // The @ symbol along with the the azuredatabricks.net information appears to
                    // match on a lot of random values. Splitting on @ symbol prevents the wild matches
                    foreach(string hiveTablePart in name.Split("@")){
                        var condition = new JObject();
                        condition.Add("attributeName", "qualifiedName");
                        condition.Add("operator", "contains");
                        condition.Add("attributeValue", hiveTablePart);
                        ((JArray)((JObject)filter!["filter"]!)["and"]!).Add(condition);
                    };
                }
                else
                {
                    var condition = new JObject();
                    condition.Add("attributeName", "qualifiedName");
                    condition.Add("operator", "contains");
                    condition.Add("attributeValue", name);
                    ((JArray)((JObject)filter!["filter"]!)["and"]!).Add(condition);
                }
                //                }
            }
            if (filter.ContainsKey("filter"))
            {
                if ((((JArray)((JObject)filter!["filter"]!)["and"]!).Count > 0) && (IsSpark_Process(this.properties!["typeName"]!.ToString().ToLower())))
                {
                    var condition = new JObject();
                    condition.Add("not", new JObject());
                    ((JObject)condition["not"]!).Add("entityType", this.properties!["typeName"]!.ToString().ToLower());
                    ((JArray)((JObject)filter!["filter"]!)["and"]!).Add(condition);
                }
                if ((itemType.ToLower().Contains("databricks")))
                {
                    TypeName = itemType;
                    var condition = new JObject();
                    condition.Add("attributeName", "name");
                    condition.Add("operator", "eq");
                    condition.Add("attributeValue", itemName);
                    ((JArray)((JObject)filter!["filter"]!)["and"]!).Add(condition);
                }

                if ((TypeName != string.Empty))
                {
                    var condition = new JObject();
                    condition.Add("entityType", TypeName);
                    ((JArray)((JObject)filter!["filter"]!)["and"]!).Add(condition);
                }
            }

            String _fqn = properties!["attributes"]!["qualifiedName"]!.ToString();
            List<QueryValeuModel> results = await this._client.Query_entities(filter["filter"]!);
            _logger.LogDebug($"Existing Asset Match Search for {_fqn}: Found {results.Count} candidate matches");
            if (results.Count > 0)
            {
                _logger.LogDebug($"Existing Asset Match Search for {_fqn}: The first match has a fqn of {results[0].qualifiedName} and type of {results[0].entityType}");
                List<QueryValeuModel> validEntitiesAfterFiltering = await SelectReturnEntity(results);
                _logger.LogDebug($"Existing Asset Match Search for {_fqn}: Found {validEntitiesAfterFiltering.Count} valid entity matches");
                if (validEntitiesAfterFiltering.Count > 0)
                {
                    _logger.LogInformation($"Existing Asset Match Search for {_fqn}: The first valid match has a fqn of {validEntitiesAfterFiltering[0].qualifiedName} and type of {validEntitiesAfterFiltering[0].entityType}");
                    obj = validEntitiesAfterFiltering[0];
                    properties["guid"] = validEntitiesAfterFiltering[0].id;
                    properties["typeName"] = validEntitiesAfterFiltering[0].entityType;
                    properties!["attributes"]!["qualifiedName"] = validEntitiesAfterFiltering[0].qualifiedName;
                    this.Fullentity = await this._client.GetByGuid(validEntitiesAfterFiltering[0].id);
                    this.is_dummy_asset = false;
                }
                // If there are matches but there are none that are valid, it should still be a dummy asset
                else
                {
                    _logger.LogInformation($"Existing Asset Match Search for {_fqn}: Changing type to placeholder type because zero valid entities");
                    properties["typeName"] = EntityType;
                }
            }
            else
            {
                _logger.LogDebug($"Existing Asset Match Search for {_fqn}: Changing type to dummy type because zero search results in general");
                properties["typeName"] = EntityType;
            }
            return obj;
        }
        private async Task<List<QueryValeuModel>> SelectReturnEntity(List<QueryValeuModel> results)
        {
            List<QueryValeuModel> validEntities = new List<QueryValeuModel>();
            bool matchingResourceSetHasBeenSeen = false;
            foreach (QueryValeuModel entity in results)
            {
                _logger.LogDebug($"Validating {this.to_compare_QualifiedName} vs {entity.qualifiedName} - Type: {entity.entityType} search score: {entity.SearchScore}");
                if (IsSpark_Entity(entity.entityType))
                    if (results[0].qualifiedName.ToLower().Trim('/') != this.properties!["attributes"]!["qualifiedName"]!.ToString().ToLower().Trim('/'))
                    {
                        this.is_dummy_asset = true;
                        validEntities.Add(entity);
                    }

                string searchResultComparableQualifiedName = string.Join("/", this.Build_Searchable_QualifiedName(entity.qualifiedName)!);
                // If the qualified name does not match, move along
                _logger.LogDebug($"Validating {this.to_compare_QualifiedName} vs {entity.qualifiedName} - Comparing RS FQNs: {this.to_compare_QualifiedName} to {searchResultComparableQualifiedName}");
                if (!QualifiedNames_Match_After_Normalizing(this.to_compare_QualifiedName, searchResultComparableQualifiedName))
                {
                    _logger.LogDebug($"Validating {this.to_compare_QualifiedName} vs {entity.qualifiedName} - The searchResultComparableQualifiedName of {searchResultComparableQualifiedName} is not match");
                    continue;
                }

                if (IsBlobOrDataLakeResourceSet_Entity(entity.entityType))
                {
                    _logger.LogDebug($"Validating {this.to_compare_QualifiedName} vs {entity.qualifiedName} - RS FQN comparison is true");
                    // In case where the search score is the same for multiple values, we cannot trust Microsoft Purview's ordering.
                    // For example if we have a period in the resource set name or there are multiple assets with very similar names,
                    // it's pushing the resource set of interest lower in the results (since ordering could be entirely random).
                    // Only insert the first resource set seen and trust that it is ordered appropriately
                    if (config!.prioritizeFirstResourceSet && !(matchingResourceSetHasBeenSeen)){
                        _logger.LogDebug($"Validating {this.to_compare_QualifiedName} vs {entity.qualifiedName} - RS {entity.qualifiedName} has been inserted first");
                        validEntities.Insert(0,entity);
                        // Assuming the order of entities are sorted by highest likely match,
                        // if we 've seen any resource set, it should be the most likely to have matched.
                        matchingResourceSetHasBeenSeen = true;
                    } else{
                        _logger.LogDebug($"Validating {this.to_compare_QualifiedName} vs {entity.qualifiedName} - RS {entity.qualifiedName} has been added to the list");
                        validEntities.Add(entity);
                    }
                    
                }
                else if (IsBlobOrDataLakeFS_Entity(entity.entityType))
                {
                    // If there are already some entities, we might want to jump the line if
                    // this entity is attached to an adf process in any way.
                    if (validEntities.Count > 0)
                    {
                        JObject folder = await _client.GetByGuid(entity.id);
                        if (IsInputOrOutputOfAzureDataFactoryEntity(folder)){
                            _logger.LogDebug($"Validating {this.to_compare_QualifiedName} vs {entity.qualifiedName} - Discovered entity is part of an ADF process and has been inserted first");
                            validEntities.Insert(0,entity);
                            continue;
                        }
                        // If the first valid entity is the default generic entity, insert
                        // this match into the first position and continue. This helps when
                        // there is a folder matching but the generic entity is higher up in search
                        if (validEntities[0].entityType == EntityType){
                            validEntities.Insert(0,entity);
                        }
                    }
                    // Fall through: We know the qualified name matches but it's either the first
                    // valid entity OR it's not attached to any Azure Data Factory process
                    validEntities.Add(entity);
                }
                else
                {
                    // Fall through: We know the qualified name matches but it's not any of the above special cases
                    validEntities.Add(entity);
                }
            }

            return validEntities;
        }

        private bool Is_Valid_Name(string name)
        {
            Func<string, bool> isNumber = delegate (string num)
             {
                 return Int64.TryParse(num, out long number)!;
             };

            if (name.Trim() == string.Empty)
                return false;
            if (name.Contains('='))
            {
                string[] partsName = name.Split("=");
                foreach (string part in partsName)
                {
                    if (isNumber(part))
                        return false;
                }
            }

            if ((this.properties!["typeName"]!.ToString().ToLower().Trim('/') == "azure_datalake_gen2_resource_set") ||
                (this.properties!["typeName"]!.ToString().ToLower().Trim('/') == "azure_datalake_gen2_path") ||
                (this.properties!["typeName"]!.ToString().ToLower().Trim('/') == "azure_blob_path") ||
                (this.properties!["typeName"]!.ToString().ToLower().Trim('/') == "azure_blob_resource_set")
              )
            {
                if (isNumber(name))
                    return false;
                if ((name.Contains('{')) && (name.Contains('}')))
                    return false;

                if ((name.ToLower().IndexOf(".csv") > -1) || (name.ToLower().IndexOf(".parquet") > -1))
                {
                    foreach (char character in name.ToCharArray())
                    {
                        if (isNumber(character.ToString()))
                            return false;
                    }
                }
            }
            return true;
        }
        private List<string>? Build_Searchable_QualifiedName(string qualifiedName)
        {
            string[] names = qualifiedName.Split('/');
            List<string> validNames = new List<string>();
            int indexCount = 0;
            foreach (string name in names)
            {
                if (indexCount > 2)
                {
                    if (this.Is_Valid_Name(name))
                    {
                        validNames.Add(name);
                    }
                }
                else
                {
                    if (indexCount != 1)
                        validNames.Add(name);
                }
                indexCount++;
            }

            return validNames;
        }
        private string to_compare_QualifiedName
        {
            get
            {
                if (this.qNames!.Count == 0)
                {
                    this.qNames.AddRange(this.properties!["attributes"]!["qualifiedName"]!.ToString().Split('/'));
                }
                return string.Join("/", this.qNames!);
            }
        }

        // For resource sets, since Microsoft Purview cannot register the same storage account for
        // both ADLS G2 and Blob Storage, we need to match against either pattern (dfs.core.windows.net
        // or blob.core.windows.net) since we cannot be certain which one the end user has scanned.
        private bool QualifiedNames_Match_After_Normalizing(string entityOfInterestQualifiedName, string candidateQualifiedName)
        {
            string _entityOfInterestFQN = entityOfInterestQualifiedName.ToLower().Trim().Replace(".dfs.core.windows.net","").Replace(".blob.core.windows.net","").Trim('/');
            string _candidateFQN = candidateQualifiedName.ToLower().Trim().Replace(".dfs.core.windows.net","").Replace(".blob.core.windows.net","").Trim('/');
            return _entityOfInterestFQN == _candidateFQN;
        }

        // Given an entity (presumably a blob or data lake folder), check to see if it has
        // relationship attributes that indicate the entity is the input to or output of
        // an azure data factory (adf_) process.
        private bool IsInputOrOutputOfAzureDataFactoryEntity(JObject folder)
        {
            if (!folder.ContainsKey("entity")){
                return false;
            }

            // TODO Refactor this to look across both inputs and outputs from one list
            if (((JArray)folder!["entity"]!["relationshipAttributes"]!["inputToProcesses"]!).Count > 0)
            {
                foreach (JObject val in ((JArray)folder!["entity"]!["relationshipAttributes"]!["inputToProcesses"]!))
                {
                    if (val!["typeName"]!.ToString().ToLower().IndexOf("adf_") > -1)
                    {
                        return true;
                    }
                }
            }
            else if (((JArray)folder!["entity"]!["relationshipAttributes"]!["outputFromProcesses"]!).Count > 0)
            {
                foreach (JObject val in ((JArray)folder!["entity"]!["relationshipAttributes"]!["outputFromProcesses"]!))
                {
                    if (val!["typeName"]!.ToString().ToLower().IndexOf("adf_") > -1)
                    {
                        return true;
                    }
                }
            }
            // Fall through - If we didn't have an adf_ relationship
            return false;
        }
    }

    /// <summary>
    /// Class that make the connection and submit request to Microsoft Purview API
    /// </summary>
    public class PurviewClient
    {
        private Int64 initGuid = -1000;
        private readonly PurviewClientHelper PurviewclientHelper;
        private readonly IHttpClientManager httpclientManager;
        ILogger _logger;
        private AppConfigurationSettings? config = new AppConfigurationSettings();
        private MemoryCache _cache = MemoryCache.Default;
        private String TOKEN_CACHE_KEY = "token";
        /// <summary>
        /// Create Object passing the logger class
        /// </summary>
        /// <param name="logger">Used to log events during execution (ILogger)</param>
        public PurviewClient(ILogger logger)
        {
            _logger = logger;
            httpclientManager = new HttpClientManager(logger);
            this.PurviewclientHelper = new PurviewClientHelper(httpclientManager, logger);
        }

        /// <summary>
        /// Search in Microsoft Purview using search/advanced API
        /// </summary>
        /// <param name="QualifiedName">Qualified Name to search on</param>
        /// <param name="typeName">Type to search on</param>
        /// <returns>EntityModel found</returns>
        public async Task<EntityModel> search_entities(string QualifiedName, string typeName)
        {
            var correlationId = Guid.NewGuid().ToString();
            var token = await this.GetToken();

            var purviewSearchEndpoint = $"{config!.purviewApiEndPoint!.ToString().Replace("{ResourceUri}", config!.PurviewApiBaseUrl())}{config!.purviewApiSearchAdvancedMethod!}";


            PurviewEntitySearchResponseModel entityObjectModel = new PurviewEntitySearchResponseModel();
            try
            {
                var filtervalue = new { typeName = typeName };
                if (typeName != String.Empty)
                {
                    _logger.LogTrace($"Searching entity using qualifiedName:{QualifiedName} typeName:{typeName}");
                    entityObjectModel = await this.PurviewclientHelper.GetEntitiesFromPurview(
                        correlationId
                        , QualifiedName
                        , purviewSearchEndpoint
                        , token,
                        filtervalue
                        );
                    if (entityObjectModel.entities.Count > 0)
                    {
                        _logger.LogTrace($"Found {entityObjectModel.entities.Count} Entities for qualifiedName:{QualifiedName} typeName:{typeName}");
                        var foundEntity = new Function.Domain.Models.EntityModel();
                        foreach (var entity in entityObjectModel.entities)
                        {
                            foundEntity = entity;
                            if (entity.entityType == typeName)
                            {
                                break;
                            }

                            _logger.LogTrace($"Found entity typeName:{entity.entityType}");
                        }
                        return foundEntity;
                    }
                }
                else
                {
                    _logger.LogTrace($"Searching entity using qualifiedName:{QualifiedName}");
                    entityObjectModel = await this.PurviewclientHelper.GetEntitiesFromPurview(
                        correlationId
                        , QualifiedName
                        , purviewSearchEndpoint
                        , token
                        );
                    var foundEntity = new Function.Domain.Models.EntityModel();
                    _logger.LogTrace($"Found {entityObjectModel.entities.Count} Entities for qualifiedName:{QualifiedName} typeName:{typeName}");
                    foreach (var entity in entityObjectModel.entities)
                    {
                        foundEntity = entity;
                        if (entity.entityType == "purview_custom_connector_generic_entity_with_columns")
                            break;
                        _logger.LogTrace($"Found entity typeName:{entity.entityType}");
                    }
                    return foundEntity;
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
            }

            return new EntityModel();
        }

        /// <summary>
        /// Delete unused Placeholder entities with specific Qualified Name
        /// </summary>
        /// <param name="qualifiedName">Qualified Name to Delete</param>
        /// <param name="typeName">Type to delete</param>
        /// <returns>Boolean</returns>
        public async Task<bool> Delete_Unused_Entity(string qualifiedName, string typeName)
        {
            var correlationId = Guid.NewGuid().ToString();
            var token = await this.GetToken();
            var purviewSearchEndpoint = $"{config!.purviewApiEndPoint.ToString().Replace("{ResourceUri}", config!.PurviewApiBaseUrl())}{config!.purviewApiEntityByTypeMethod!}"; ;

            var deleteEndPoint = $"{config!.purviewApiEndPoint!.ToString().Replace("{ResourceUri}", config!.PurviewApiBaseUrl())}{config!.purviewApiEntityByGUIDMethod!}";


            List<Asset> entities = await PurviewclientHelper.GetEntityFromPurview(correlationId, qualifiedName.Trim('/').ToLower(), purviewSearchEndpoint, token, typeName);

            foreach (var entity in entities)
            {
                bool isUnused = false;
                if (entity!.relationshipAttributes!.ContainsKey("outputFromProcesses"))
                    if (((JArray)entity.relationshipAttributes!["outputFromProcesses"]).Count == 0)
                        isUnused = false;
                    else
                        isUnused = true;

                if (!isUnused)
                {
                    if (entity!.relationshipAttributes!.ContainsKey("inputToProcesses"))
                        if (((JArray)entity.relationshipAttributes!["inputToProcesses"]).Count == 0)
                            isUnused = false;
                        else
                            isUnused = true;
                }

                if (!isUnused)
                    await PurviewclientHelper.DeleteEntityByGuidInPurview(correlationId, token, entity.guid, deleteEndPoint);
            }

            return true;
        }
        /// <summary>
        /// Search in Microsoft Purview using search/advanced API
        /// </summary>
        /// <param name="QualifiedName">Qualified Name to search on</param>
        /// <returns>EntityModel found</returns>
        public async Task<EntityModel> search_entities(string QualifiedName)
        {
            return await search_entities(QualifiedName, String.Empty);
        }
        private async Task<string> GetToken()
        {
            // If the Memory Cache already has a token stored
            // test to see if it's actually expired (because the token might expire soon)
            if (_cache.Contains(TOKEN_CACHE_KEY))
            {
                var cachedAuth = (AuthenticationResult)_cache.Get(TOKEN_CACHE_KEY);
                // If the cached auth expires later than NOW + 3 minutes
                // We are good to use the token for the next operation
                if (cachedAuth.ExpiresOn > DateTime.UtcNow.AddMinutes(3)){
                    _logger.LogInformation("PurviewClient-GetToken: Token cache hit, no need to refresh token");
                    return cachedAuth.AccessToken;
                }
                // Else, fall through and get a new token because it's about to expire
            }
            _logger.LogWarning("PurviewClient-GetToken: Purview Client Token doesn't exist or will expire soon, attempt refresh");

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

            app.AddInMemoryTokenCache();

            // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator. 
            string[] scopes = new string[] { $"https://{config.AuthenticationUri}/.default" }; // Generates a scope -> "https://purview.azure.net/.default"

            AuthenticationResult? result;
            try
            {
                result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                // Mitigation: change the scope to be as expected
                _logger.LogError("Error getting Purview API Authentication Token");
                return String.Empty;
            }
            catch (Exception coreex)
            {

                _logger.LogError($"Error getting Purview API Authentication Token - Invalid scope. The scope has to be of the form {scopes}");
                _logger.LogError(coreex.Message);
                return String.Empty;
            }

            _logger.LogInformation($"PurviewClient-GetToken: Purview Client Token refresh successful. Adding to cache and will expire in {config!.tokenCacheTimeInHours}");
            var cacheItem = new CacheItem(TOKEN_CACHE_KEY, result);
            // We need to recreate this CacheItemPolicy every time
            // If we set the policy only once, the AbsoluteExpiration won't get updated
            // This should only be a problem in long standing PurviewClient applications
            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(config!.tokenCacheTimeInHours)
            };
            // Need to use Set in case the token already exists
            // We expect that a token that expires in three minutes will be updated
            // so the token will exist already
            _cache.Set(cacheItem, cacheItemPolicy);
            return result.AccessToken;
        }
        /// <summary>
        /// Send Entity/Entities to purview to be created or updated
        /// </summary>
        /// <param name="payloadJson">List of Entities</param>
        /// <returns>HTTP Response Message</returns>
        public async Task<HttpResponseMessage> Send_to_Purview(string payloadJson)
        {
            var correlationId = Guid.NewGuid().ToString();
            var token = await this.GetToken();
            if (token == String.Empty)
            {
                _logger.LogError("Unable to get Token to Purview");
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }


            _logger.LogInformation($"Sending this payload to Purview: {payloadJson}");

            var purviewApiEntityBulkMethod = config!.purviewApiEntityBulkMethod!;
            var purviewEntityEndpoint = $"{config.purviewApiEndPoint.ToString().Replace("{ResourceUri}", config.PurviewApiBaseUrl())}{purviewApiEntityBulkMethod}";


            HttpResponseMessage? response = await this.PurviewclientHelper.PostEntitiesToPurview(
                correlationId,
                token,
                JObject.Parse(payloadJson),
                purviewEntityEndpoint
            ) ?? new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);
            return response;
        }

        /// <summary>
        /// Search entities in Microsoft Purview using search/query?api-version=2021-05-01-preview
        /// </summary>
        /// <param name="filter">Filter to be applied to the search</param>
        /// <returns>List of QueryValeuModel</returns>
        public async Task<List<QueryValeuModel>> Query_entities(object filter)
        {
            var correlationId = Guid.NewGuid().ToString();
            var token = await this.GetToken();

            var purviewSearchEndpoint = $"{config!.purviewApiEndPoint!.ToString().Replace("{ResourceUri}", config!.PurviewApiBaseUrl())}{config!.purviewApiEntityQueryMethod!}";

            List<QueryValeuModel> returnValue = await PurviewclientHelper.QueryEntities(correlationId, token, purviewSearchEndpoint, filter);

            return returnValue;
        }

        /// <summary>
        /// Retrieved Entity in Microsoft Purview by GUI using entity/guid API
        /// </summary>
        /// <param name="guid">GUI to search by</param>
        /// <returns>Json Object</returns>
        public async Task<JObject> GetByGuid(string guid)
        {
            var token = await this.GetToken();

            var purviewSearchEndpoint = $"{config!.purviewApiEndPoint!.ToString().Replace("{ResourceUri}", config!.PurviewApiBaseUrl())}{config!.purviewApiEntityByGUIDMethod!}";

            JObject entity = await PurviewclientHelper.GetEntityByGuid(token, purviewSearchEndpoint, guid);
            return entity;
        }

        /// <summary>
        /// Get Entity in Microsoft Purview using bulk/uniqueAttribute/type API
        /// </summary>
        /// <param name="qualifiedName">Qualified Name to Search by</param>
        /// <param name="typeName">Type Name to search by</param>
        /// <returns></returns>
        public async Task<List<Asset>> Get_Entity(string qualifiedName, string typeName)
        {
            var correlationId = Guid.NewGuid().ToString();
            var token = await this.GetToken();

            var purviewSearchEndpoint = $"{config!.purviewApiEndPoint!.ToString().Replace("{ResourceUri}", config!.PurviewApiBaseUrl())}{config!.purviewApiEntityByTypeMethod!}";

            List<Asset> entities = await PurviewclientHelper.GetEntityFromPurview(correlationId, qualifiedName.Trim('/').ToLower(), purviewSearchEndpoint, token, typeName);
            return entities;
        }

        /// <summary>
        /// Function to generate new GUIDs and keep track of the sequence during the execution
        /// </summary>
        /// <returns>new GUID (Int64)</returns>
        public Int64 NewGuid()
        {

            return initGuid--;
        }
    }

}
