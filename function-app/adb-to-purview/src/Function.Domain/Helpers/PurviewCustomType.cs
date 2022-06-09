using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Function.Domain.Providers;
using Function.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Function.Domain.Models.Purview;
using System.Collections.Generic;


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
        private JObject? simpleEntity;
        private JObject? properties;

        public JObject? Fullentity = new JObject();
        bool useResourceSet = bool.Parse(Environment.GetEnvironmentVariable("useResourceSet") ?? "true");
        bool usePurviewTypes = bool.Parse(Environment.GetEnvironmentVariable("usePurviewTypes") ?? "false");
        /// <summary>
        /// Property that contains all Json attributes for the Custom data Entity in Microsoft Purview
        /// </summary>
        public JObject Properties
        {
            get { return properties!; }
        }
        /// <summary>
        /// Creation of a Microsoft Purview Custom Type entity that initialize all attributes needed
        /// </summary>
        /// <param name="name">Name of the Data Entity</param>
        /// <param name="typeName">Type of the Data Entity</param>
        /// <param name="qualified_name">Qualified Name  of the Data Entity</param>
        /// <param name="data_type">Orignal Type in case Data entity can't be found (Not scanned yet by Microsoft Purview)</param>
        /// <param name="description">Description  of the Data Entity</param>
        /// <param name="guid">GUID  of the Data Entity</param>
        /// <param name="logger">Hand to the logger to be used globally to log information during execution</param>
        /// <param name="client">Client to connect to Microsoft Purview API</param>
        public PurviewCustomType(string name, string typeName, string qualified_name, string data_type, string description, Int64 guid, ILogger logger, PurviewClient client)
        {
            _logger = logger;
            _client = client;
            Init(name
            , typeName
            , qualified_name
            , data_type
            , description
            , guid
            );
            _logger.LogInformation($"New Entity Initialized in the process with a passed Purview Client: Nome:{name} - qualified_name:{qualified_name} - Guid:{guid}");
        }
        /// <summary>
        /// Creation of a Microsoft Purview Custom Type entity that initialize all attributes needed
        /// </summary>
        /// <param name="name">Name of the Data Entity</param>
        /// <param name="typeName">Type of the Data Entity</param>
        /// <param name="qualified_name">Qualified Name  of the Data Entity</param>
        /// <param name="data_type">Orignal Type in case Data entity can't be found (Not scanned yet by Microsoft Purview)</param>
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
            string[]? spark_entities = (Environment.GetEnvironmentVariable("Spark_Entities") ?? "databricks_workspace;databricks_job;databricks_notebook;databricks_notebook_task").Split(";");
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
            string[]? spark_entities = (Environment.GetEnvironmentVariable("Spark_Process") ?? "databricks_process").Split(";");
            var findTypeName = Array.Find<string>(spark_entities!, element => element.Equals(typeName));
            if (findTypeName == typeName)
                return true;
            return false;
        }

        private void Init(string name, string typeName, string qualified_name, string data_type, string description, Int64 guid)
        {
            is_dummy_asset = false;
            properties = new JObject();
            simpleEntity = new JObject();
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
        /// Export basic data entity attribute
        /// </summary>
        /// <value>Json Object</value>
        public JObject SimpleEntity
        {
            get
            {
                //Simple Json format for use in Atlas requests
                simpleEntity = new JObject();
                simpleEntity.Add("typeName", this.Properties["typeName"]!.ToString());
                simpleEntity.Add("guid", this.Properties["guid"]!.ToString());
                simpleEntity.Add("qualifiedName", this.Properties["attributes"]!["qualifiedName"]!.ToString());
                _logger.LogInformation($"Retrived Entity simple Object: {simpleEntity.ToString()}");
                return simpleEntity;
            }
        }
        /// <summary>
        /// Get a list on Data Entities in Microsoft Purview using Entoty Qualified Name and Type
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
        /// <summary>
        /// Add Relationship to entity as columns to tables type
        /// </summary>
        /// <param name="Table">Table to be related to</param>
        /// <returns>Bollean</returns>
        public bool AddToTable(PurviewCustomType Table)
        {
            //Validating if the table attribute exists if not we will initialize
            if (!((JObject)properties!["relationshipAttributes"]!).ContainsKey("table"))
                ((JObject)properties!["relationshipAttributes"]!).Add("table", new JObject());

            _logger.LogInformation($"Entity qualifiedName: {properties["attributes"]!["qualifiedName"]}Table.simpleEntity: {Table!.simpleEntity!.ToString()}");
            properties["relationshipAttributes"]!["table"] = Table.simpleEntity;
            return true;
        }
        /// <summary>
        /// Search an Entity in Microsoft Purview using Qualified Name and Type
        /// </summary>
        /// <param name="typeName">Type to search for</param>
        /// <returns>Json object</returns>
        public async Task<JObject> FindQualifiedNameInPurview(string typeName)
        {
            //Search using search method and qualifiedname attribute. Scape needs to be used on some non safe (web URLs) chars
            EntityModel results = await this._client.search_entities(
                properties!["attributes"]!["qualifiedName"]!.ToString()
                , typeName);

            if (results.qualifiedName == null)
            {
                _logger.LogInformation($"Entity qualifiedName:{properties["attributes"]!["qualifiedName"]!.ToString()} - typeName:{typeName}, not found!");
                this.is_dummy_asset = true;
                properties["typeName"] = EntityType;
                return new JObject();
            }
            var guid = "";
            this.is_dummy_asset = false;
            var _qualifiedName = "";
            //validate if qualifiedname is the same
            _qualifiedName = results.qualifiedName;
            if (results.entityType == "azure_datalake_gen2_resource_set")
                properties["attributes"]!["qualifiedName"] = _qualifiedName;
            if (_qualifiedName.Trim('/').ToLower() == properties["attributes"]!["qualifiedName"]!.ToString().Trim('/').ToLower())
            {
                //search api return quid on ID property
                guid = results.id;
                properties["typeName"] = results.entityType;
                properties!["attributes"]!["qualifiedName"] = results.qualifiedName;
                //break if find a non dummy entity with the qualified name
                if (results.entityType == EntityType)
                {
                    //log.debug("search_entity_by_qualifiedName: entity \'{name}\' is Dummy Entity");
                    //mark entity as dummy to be created
                    this.is_dummy_asset = true;
                }
                _logger.LogInformation($"Entity qualifiedName:{properties["attributes"]!["qualifiedName"]!.ToString()} - typeName:{typeName} - guid:{guid}, found!");
            }

            if (!this.is_dummy_asset)
                properties!["attributes"]!["qualifiedName"] = results.qualifiedName;

            var content = new JObject();
            content.Add(_qualifiedName, new JObject());
            properties["guid"] = guid;
            ((JObject)content[_qualifiedName]!).Add("guid", guid);
            ((JObject)content[_qualifiedName]!).Add("qualifiedName", properties!["attributes"]!["qualifiedName"]!.ToString());

            return content;
        }
        /// <summary>
        /// Remove any unused Dummy entities
        /// </summary>
        /// <returns>Boolean</returns>
        public async Task<bool> CleanUnusedCustomEntities()
        {
            return await this._client.Delete_Unused_Entity(
                properties!["attributes"]!["qualifiedName"]!.ToString()
                , properties!["typeName"]!.ToString());
        }

        private List<string>? qNames = new List<string>();
        /// <summary>
        /// Search the entoty in Microsoft Purview using Query API, only using Qualified Name
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
            JObject filter = new JObject();
            this.is_dummy_asset = true;

            foreach (string name in qNames!)
            {
                if (!filter.ContainsKey("filter"))
                {
                    filter.Add("filter", new JObject());
                    ((JObject)filter!["filter"]!).Add("and", new JArray());
                }
                //SUpport for the cases when user use wasbs protocall for a ADLS GEN 2
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
                if (TypeName != string.Empty)
                {
                    var condition = new JObject();
                    condition.Add("entityType", TypeName);
                    ((JArray)((JObject)filter!["filter"]!)["and"]!).Add(condition);
                }
            }

            List<QueryValeuModel> results = await this._client.Query_entities(filter["filter"]!);
            if (results.Count == 1)
            {
                if (IsSpark_Entity(results[0].entityType))
                    if (results[0].qualifiedName.ToLower().Trim('/') != this.properties!["attributes"]!["qualifiedName"]!.ToString().ToLower().Trim('/'))
                    {
                        this.is_dummy_asset = true;
                        return obj;
                    }
                properties["guid"] = results[0].id;
                properties["typeName"] = results[0].entityType;
                properties!["attributes"]!["qualifiedName"] = results[0].qualifiedName;
                this.Fullentity = await this._client.GetByGuid(results[0].id);
                this.is_dummy_asset = false;
                return results[0];
            }
            if (results.Count > 0)
            {
                List<QueryValeuModel> validentity = await SelectReturnEntity(results);
                if (validentity.Count > 0)
                {
                    obj = validentity[0];
                    properties["guid"] = validentity[0].id;
                    properties["typeName"] = validentity[0].entityType;
                    properties!["attributes"]!["qualifiedName"] = validentity[0].qualifiedName;
                    this.Fullentity = await this._client.GetByGuid(validentity[0].id);
                    this.is_dummy_asset = false;
                }
            }
            else
            {
                properties["typeName"] = EntityType;
            }
            return obj;
        }
        private async Task<List<QueryValeuModel>> SelectReturnEntity(List<QueryValeuModel> results)
        {
            List<QueryValeuModel> validEntities = new List<QueryValeuModel>();
            foreach (QueryValeuModel entity in results)
            {
                string qualifiednameToCompera = string.Join("/", this.Build_Searchable_QualifiedName(entity.qualifiedName)!);
                if ((entity.entityType == "azure_datalake_gen2_resource_set") || (entity.entityType == "azure_blob_resource_set"))
                {
                    if (qualifiednameToCompera.ToLower().Trim() == this.to_compare_QualifiedName.ToLower().Trim())
                    {
                        validEntities.Add(entity);
                    }
                }
                else
                {
                    if (qualifiednameToCompera.ToLower().Trim('/') == this.to_compare_QualifiedName.ToLower().Trim('/'))
                    {
                        if ((entity.entityType.ToLower() == "azure_blob_path") || (entity.entityType.ToLower() == "azure_datalake_gen2_path") || (entity.entityType.ToLower() == "azure_datalake_gen2_filesystem"))
                        {
                            if (validEntities.Count > 0)
                            {
                                JObject folder = await _client.GetByGuid(entity.id);
                                if (folder.ContainsKey("entity"))
                                {
                                    if (((JArray)folder!["entity"]!["relationshipAttributes"]!["inputToProcesses"]!).Count > 0)
                                    {
                                        foreach (JObject val in ((JArray)folder!["entity"]!["relationshipAttributes"]!["inputToProcesses"]!))
                                        {
                                            if (val!["typeName"]!.ToString().ToLower().IndexOf("adf_") > -1)
                                            {
                                                validEntities = new List<QueryValeuModel>();
                                                validEntities.Add(entity);
                                                return validEntities;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (((JArray)folder!["entity"]!["relationshipAttributes"]!["outputFromProcesses"]!).Count > 0)
                                        {
                                            foreach (JObject val in ((JArray)folder!["entity"]!["relationshipAttributes"]!["outputFromProcesses"]!))
                                            {
                                                if (val!["typeName"]!.ToString().ToLower().IndexOf("adf_") > -1)
                                                {
                                                    validEntities = new List<QueryValeuModel>();
                                                    validEntities.Add(entity);
                                                    return validEntities;
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                        }
                        validEntities.Add(entity);
                    }
                }
            }
            return validEntities;

        }

        private string Name_To_Search(string Name)
        {
            Func<string, bool> isNumber = delegate (string num)
             {
                 return Int64.TryParse(num, out long number)!;
             };

            Func<char, string> newName = delegate (char separator)
             {
                 string[] partsName = Name.Split(separator);
                 int index = 0;
                 foreach (string? part in partsName)
                 {
                     if (isNumber(part))
                         partsName[index] = "{N}";
                     index++;
                 }
                 return string.Join(separator, partsName)!;
             };

            if (isNumber(Name))
            {
                return "{N}";
            }

            if (Name.Contains('='))
            {
                return newName('=');
            }

            if (Name.Contains('-'))
            {
                return newName('-');
            }

            if (Name.Contains('_'))
            {
                return newName('_');
            }

            return Name;
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
                    foreach (char charactere in name.ToCharArray())
                    {
                        if (isNumber(charactere.ToString()))
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
        private AuthenticationResult? _token;

        /// <summary>
        /// Create Object passing the logger class
        /// </summary>
        /// <param name="logger">Used to log events during execution (ILogger)</param>
        public PurviewClient(ILogger logger)
        {
            _logger = logger;
            httpclientManager = new HttpClientManager(logger);
            this.PurviewclientHelper = new PurviewClientHelper(httpclientManager, logger);
            GetToken();
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
            var token = this.GetToken();
            var baseurl = $"https://{Environment.GetEnvironmentVariable("PurviewAccountName") ?? ""}.catalog.purview.azure.com/api";
            var purviewSearchEndpoint = $"{baseurl}/atlas/v2/search/advanced";
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
        /// Delete unused Dumies entities with specific Qualified Name
        /// </summary>
        /// <param name="quilifiedName">Qualified Name to Delete</param>
        /// <param name="typeName">Type to delete</param>
        /// <returns>Boolean</returns>
        public async Task<bool> Delete_Unused_Entity(string quilifiedName, string typeName)
        {
            var correlationId = Guid.NewGuid().ToString();
            var token = this.GetToken();
            var baseurl = $"https://{Environment.GetEnvironmentVariable("PurviewAccountName") ?? ""}.purview.azure.com/catalog/api";
            var purviewSearchEndpoint = $"{baseurl}/atlas/v2/entity/bulk/uniqueAttribute/type/";
            var deleteEndPoint = $"{baseurl}/atlas/v2/entity/guid/";
            List<Asset> entities = await PurviewclientHelper.GetEntityFromPurview(correlationId, quilifiedName.Trim('/').ToLower(), purviewSearchEndpoint, token, typeName);

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
        private string GetToken()
        {

            if (_token != null)
            {
                if (_token.ExpiresOn > DateTimeOffset.Now)
                {
                    _logger.LogTrace($"Token to access Purview was reused");
                    return _token.AccessToken;
                }
            }

            //The _client id that Azure AD created when you registered your _client app.
            string _clientID = Environment.GetEnvironmentVariable("ClientID") ?? "";
            string _clientSecret = Environment.GetEnvironmentVariable("ClientSecret") ?? "";

            string TenantId = Environment.GetEnvironmentVariable("TenantId") ?? "";

            string resourceUri = Environment.GetEnvironmentVariable("ResourceUri") ?? "";
            string AuthEndPoint = Environment.GetEnvironmentVariable("AuthEndPoint") ?? "";

            AuthEndPoint = $"{AuthEndPoint}{TenantId}";

            ClientCredential cc = new ClientCredential(_clientID, _clientSecret);
            var context = new AuthenticationContext(string.Format(AuthEndPoint, TenantId));
            var result = context.AcquireTokenAsync(resourceUri, cc);
            if (result == null)
            {
                _logger.LogError("Error geting Authentication Token ofr Purview API");
                return String.Empty;
            }
            if (result.Result == null)
            {
                _logger.LogError($"Error geting Authentication Token ofr Purview API: {result!.Exception!.Message}");
                return String.Empty;
            }
            _token = result.Result;
            _logger.LogInformation($"Token to access Purview was generated");
            return _token.AccessToken;
        }
        /// <summary>
        /// Send Entity/Entities to purview to be created or updated
        /// </summary>
        /// <param name="payloadJson">List of Entities</param>
        /// <returns>HTTP Response Message</returns>
        public async Task<HttpResponseMessage> Send_to_Purview(string payloadJson)
        {
            var correlationId = Guid.NewGuid().ToString();
            var token = this.GetToken();
            if (token == String.Empty)
            {
                _logger.LogError("Unable to get Token to Purview");
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }


            _logger.LogInformation($"Sending this payload to Purview: {payloadJson}");

            var baseurl = $"https://{Environment.GetEnvironmentVariable("PurviewAccountName") ?? ""}.catalog.purview.azure.com/api";
            var purviewEntityEndpoint = $"{baseurl}/atlas/v2/entity/bulk";

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
            var token = this.GetToken();
            var baseurl = $"https://{Environment.GetEnvironmentVariable("PurviewAccountName") ?? "purview-to-adb-demo-openlineage-purview"}.catalog.purview.azure.com/api";
            var purviewSearchEndpoint = $"{baseurl}/search/query?api-version=2021-05-01-preview";

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
            var token = this.GetToken();
            var baseurl = $"https://{Environment.GetEnvironmentVariable("PurviewAccountName") ?? "purview-to-adb-demo-openlineage-purview"}.purview.azure.com/catalog/api";
            var purviewSearchEndpoint = $"{baseurl}/atlas/v2/entity/guid/";
            JObject entity = await PurviewclientHelper.GetEntityByGuid(token, purviewSearchEndpoint, guid);
            return entity;
        }

        /// <summary>
        /// Get Entity in Microsoft Purview using bulk/uniqueAttribute/type API
        /// </summary>
        /// <param name="quilifiedName">Qualified Name to Search by</param>
        /// <param name="typeName">Type Name to search by</param>
        /// <returns></returns>
        public async Task<List<Asset>> Get_Entity(string quilifiedName, string typeName)
        {
            var correlationId = Guid.NewGuid().ToString();
            var token = this.GetToken();
            var baseurl = $"https://{Environment.GetEnvironmentVariable("PurviewAccountName") ?? ""}.purview.azure.com/catalog/api";
            var purviewSearchEndpoint = $"{baseurl}/atlas/v2/entity/bulk/uniqueAttribute/type/";
            var deleteEndPoint = $"{baseurl}/atlas/v2/entity/guid/";
            List<Asset> entities = await PurviewclientHelper.GetEntityFromPurview(correlationId, quilifiedName.Trim('/').ToLower(), purviewSearchEndpoint, token, typeName);
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
