using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Function.Domain.Helpers;
using System.Net.Http;
using Function.Domain.Models;

namespace Function.Domain.Services
{

    /// <summary>
    /// Class responsible for interaction with Purview API
    /// </summary>
    public class PurviewIngestion : IPurviewIngestion
    {
        private bool useResourceSet = bool.Parse(Environment.GetEnvironmentVariable("useResourceSet") ?? "true");
        private bool usePurviewTypes = bool.Parse(Environment.GetEnvironmentVariable("usePurviewTypes") ?? "false");
        private PurviewClient _purviewClient;
        private Int64 initGuid = -1000;
        //stores all mappings of columns for Origin and destination assets
        private Hashtable columnmapping = new Hashtable();
        //flag use to mark if a data Asset is a Dummy type
        private Dictionary<string, PurviewCustomType> entities = new Dictionary<string, PurviewCustomType>();
        List<PurviewCustomType> inputs_outputs = new List<PurviewCustomType>();
        private JArray to_purview_Json = new JArray();
        private readonly ILogger<PurviewIngestion> _logger;
        private List<PurviewCustomType> found_entities = new List<PurviewCustomType>();
        
        /// <summary>
        /// Create Object
        /// </summary>
        /// <param name="log">logger object (ILogger<PurviewIngestion>)</param>
        public PurviewIngestion(ILogger<PurviewIngestion> log)
        {
            _logger = log;
            _purviewClient = new PurviewClient(_logger);
            log.LogInformation($"Got Purview Client!");

        }

        /// <summary>
        /// Send to Microsoft Purview API an Array on Data Entities
        /// </summary>
        /// <param name="Processes">Array of Entities</param>
        /// <returns>Array on Entities</returns>
        public async Task<JArray> SendToPurview(JArray Processes)
        {
            foreach (JObject process in Processes)
            {

                if (await SendToPurview(process))
                {
                    return new JArray();
                }
            }
            return new JArray();
        }

        /// <summary>
        /// Send to Microsoft Purview API an single Entity to be inserted or updated
        /// </summary>
        /// <param name="json">Json Object</param>
        /// <returns>Boolean</returns>
        public async Task<bool> SendToPurview(JObject json)
        {
            var entities = get_attribute("entities", json);
            bool hasProcess = false;

            if (entities == null)
            {
                Log("Error", "Not found Attribute entities on " + json.ToString());
                return false;
            }

            foreach (JObject entity in entities)
            {
                if (Validate_Process_Json(entity))
                {
                    JObject new_entity = await Validate_Process_Entities(entity);
                    hasProcess = Process_Json(new_entity);
                    to_purview_Json.Add(new_entity);
                }
                else
                {
                    if (Validate_Entities_Json(entity))
                    {
                        PurviewCustomType new_entity = await Validate_Entities(entity);
                        //Check Entity Relatioship
//                        if (new_entity.is_dummy_asset)
//                            to_purview_Json.Add(new_entity.Properties);
                        
                        string qualifiedName = entity["attributes"]!["qualifiedName"]!.ToString();
                        if (entity.ContainsKey("relationshipAttributes"))
                        {
                            foreach (var rel in entity["relationshipAttributes"]!.Values<JProperty>())
                            {
                                if (((JObject)(entity["relationshipAttributes"]![rel!.Name]!)).ContainsKey("qualifiedName"))
                                {
                                    if (this.entities.ContainsKey(entity["relationshipAttributes"]![rel!.Name]!["qualifiedName"]!.ToString()))
                                    {
                                        entity["relationshipAttributes"]![rel!.Name]!["guid"] = this.entities[entity["relationshipAttributes"]![rel!.Name]!["qualifiedName"]!.ToString()].Properties["guid"];
                                    }
                                    else
                                    {
                                        string qn = entity["relationshipAttributes"]![rel!.Name]!["qualifiedName"]!.ToString();
                                        PurviewCustomType sourceEntity = new PurviewCustomType("search relationship"
                                            , ""
                                            , qn
                                            , ""
                                            , "search relationship"
                                            , NewGuid()
                                            , _logger
                                            , _purviewClient);


                                        QueryValeuModel sourceJson = await sourceEntity.QueryInPurview();

                                        if (!this.entities.ContainsKey(qn))
                                            this.entities.Add(qn, sourceEntity);
                                        entity["relationshipAttributes"]![rel!.Name]!["guid"] = sourceEntity.Properties["guid"];

                                    }

                                }
                            }
                        }
                        to_purview_Json.Add(entity);
                    }
                }
            }

            HttpResponseMessage results;
            string? payload = "";
            if (!hasProcess)
            {
                if (inputs_outputs.Count > 0)
                {
                    JArray tempEntities = new JArray();
                    foreach (var newEntity in inputs_outputs)
                    {
                        if (newEntity.is_dummy_asset)
                        {
                            if (!usePurviewTypes)
                                newEntity.Properties["attributes"]!["qualifiedName"] = newEntity.Properties["attributes"]!["qualifiedName"]!.ToString().ToLower();
                            tempEntities.Add(newEntity.Properties);
                        }
                    }
                    payload = "{\"entities\": " + tempEntities.ToString() + "}";
                    JObject? Jpayload = JObject.Parse(payload);
                    Log("Info", $"Entities to load: {Jpayload.ToString()}");
                    results = await _purviewClient.Send_to_Purview(payload);
                    if (results != null)
                    {
                        if (results.ReasonPhrase != "OK")
                        {
                            Log("Error", $"Error Loading to Purview: Return Code: {results.StatusCode} - Reason:{results.ReasonPhrase}");
                        }
                        else
                        {
                            var data = await results.Content.ReadAsStringAsync();
                            Log("Info", $"Purview Loaded Relationship, Input and Output Entities: Return Code: {results.StatusCode} - Reason:{results.ReasonPhrase} - Content: {data}");
                        }
                    }
                    else
                    {
                        Log("Error", $"Error Loading to Purview!");
                    }
                }
            }
            if (to_purview_Json.Count > 0)
            {
                Log("Debug", to_purview_Json.ToString());
                payload = "{\"entities\": " + to_purview_Json.ToString() + "}";
                JObject? Jpayload = JObject.Parse(payload);
                Log("Info", $"Processes to load: {Jpayload.ToString()}");
                results = await _purviewClient.Send_to_Purview(payload);
                if (results != null)
                {
                    if (results.ReasonPhrase != "OK")
                    {
                        Log("Error", $"Error Loading to Purview: Return Code: {results.StatusCode} - Reason:{results.ReasonPhrase}");
                    }
                }
                else
                {
                    Log("Error", $"Error Loading to Purview!");
                }
                foreach (var entity in this.entities)
                {
                    await _purviewClient.Delete_Unused_Entity(entity.Key, "purview_custom_connector_generic_entity_with_columns");
                }
                return true;
            }
            else
            {
                if (json.Count > 0)
                {
                    Log("INFO", $"Payload: {json}");
                    Log("Error", "Nothing found to load on to Purview, look if the payload is empty.");
                }
                else
                {
                    Log("Error", "No Purview entity to load");
                }
                foreach (var entity in this.entities)
                {
                    await _purviewClient.Delete_Unused_Entity(entity.Key, "purview_custom_connector_generic_entity_with_columns");
                }
                return false;
            }
        }
        private bool Validate_Entities_Json(JObject Process)
        {
            if (!Process.ContainsKey("typeName"))
            {
                return false;
            }
            /*            if (!Process.ContainsKey("guid"))
                        {
                            return false;
                        }*/
            if (!Process.ContainsKey("attributes"))
            {
                return false;
            }
            if (Process["attributes"]!.GetType() != typeof(JObject))
                return false;

            if (!((JObject)Process["attributes"]!).ContainsKey("qualifiedName"))
            {
                return false;
            }
            return true;
        }
        private async Task<PurviewCustomType> Validate_Entities(JObject Process)
        {

            string qualifiedName = Process["attributes"]!["qualifiedName"]!.ToString();
            string Name = Process["attributes"]!["name"]!.ToString();
            string typename = Process["typeName"]!.ToString();
            //string guid = Process["guid"]!.ToString();
            PurviewCustomType sourceEntity = new PurviewCustomType(Name
                , typename
                , qualifiedName
                , typename
                , $"Data Assets {Name}"
                , NewGuid()
                , _logger
                , _purviewClient);


            QueryValeuModel sourceJson = await sourceEntity.QueryInPurview();

            Process["guid"] = sourceEntity.Properties["guid"];
            if (sourceEntity.is_dummy_asset)
            {
                sourceEntity.Properties["typeName"] = Process["typeName"]!.ToString();
                if (!entities.ContainsKey(qualifiedName))
                    entities.Add(qualifiedName, sourceEntity);
                Log("Info", $"Entity: {qualifiedName} Type: {typename}, Not found, Creating Dummy Entity");
                return sourceEntity;
            }
            if (!entities.ContainsKey(qualifiedName))
                entities.Add(qualifiedName, sourceEntity);
            return sourceEntity;
        }

        private async Task<PurviewCustomType> SetOutputInput(JObject outPutInput, string inorout)
        {

            string qualifiedName = outPutInput["uniqueAttributes"]!["qualifiedName"]!.ToString();
            string newqualifiedName = qualifiedName;
            string[] tmpName = qualifiedName.Split('/');
            string Name = tmpName[tmpName.Length - 1];
            if (Name == "")
                Name = tmpName[tmpName.Length - 2];
            string typename = outPutInput["typeName"]!.ToString();
            string originalTypeName = typename;
            PurviewCustomType sourceEntity = new PurviewCustomType(Name
            , typename
            , qualifiedName
            , typename
            , $"Data Assets {Name}"
            , _purviewClient.NewGuid()
            , _logger
            , _purviewClient);

            QueryValeuModel sourceJson = await sourceEntity.QueryInPurview();
            if (sourceEntity.is_dummy_asset)
            {
                if (usePurviewTypes)
                {
                    outPutInput["typeName"] = originalTypeName;
                    sourceEntity.Properties["typeName"] = originalTypeName;
                }
                else
                {
                    outPutInput["typeName"] = sourceEntity.Properties["typeName"];
                    outPutInput["uniqueAttributes"]!["qualifiedName"] = sourceEntity.Properties!["attributes"]!["qualifiedName"]!.ToString().ToLower();
                }
                inputs_outputs.Add(sourceEntity);
                Log("Info", $"{inorout} Entity: {qualifiedName} Type: {typename}, Not found, Creating Dummy Entity");
            }
            else
            {
                outPutInput["uniqueAttributes"]!["qualifiedName"] = sourceEntity.Properties!["attributes"]!["qualifiedName"]!.ToString();
                outPutInput["typeName"] = sourceEntity.Properties!["typeName"]!.ToString();
            }

            if (!entities.ContainsKey(qualifiedName))
                entities.Add(qualifiedName, sourceEntity);

            return sourceEntity;
        }

        private async Task<JObject> Validate_Process_Entities(JObject Process)
        {
            //Validate process
            string qualifiedName = Process["attributes"]!["qualifiedName"]!.ToString();
            string Name = Process["attributes"]!["name"]!.ToString(); ;
            string typename = Process["typeName"]!.ToString();
            PurviewCustomType processEntity = new PurviewCustomType(Name
                                , typename
                                , qualifiedName
                                , typename
                                , $"Data Assets {Name}"
                                , NewGuid()
                                , _logger
                                , _purviewClient);
            QueryValeuModel processModel = await processEntity.QueryInPurview();
            Process["guid"] = processEntity.Properties["guid"];
            Process["attributes"]!["qualifiedName"] = processEntity.Properties["attributes"]!["qualifiedName"]!.ToString();
            //Validate inputs
            foreach (JObject inputs in Process["attributes"]!["inputs"]!)
            {
                PurviewCustomType returnInput = await SetOutputInput(inputs!, "inputs");
            }
            //Validate Outputs
            foreach (JObject outputs in Process["attributes"]!["outputs"]!)
            {
                PurviewCustomType returnOutput = await SetOutputInput(outputs!, "outputs");
            }

            //Validate Relationships
            if (Process.ContainsKey("relationshipAttributes"))
            {
                foreach (var rel in Process["relationshipAttributes"]!.Values<JProperty>())
                {
                    qualifiedName = Process["relationshipAttributes"]![rel!.Name]!["qualifiedName"]!.ToString();
                    string[] tmpName = qualifiedName.Split('/');
                    Name = tmpName[tmpName.Length - 1];
                    typename = "purview_custom_connector_generic_entity_with_columns";
                    if (!entities.ContainsKey(qualifiedName))
                    {

                        PurviewCustomType sourceEntity = new PurviewCustomType(Name
                            , typename
                            , qualifiedName
                            , typename
                            , $"Data Assets {Name}"
                            , NewGuid()
                            , _logger
                            , _purviewClient);


                        var outputObj = await sourceEntity.QueryInPurview();
                        Process["relationshipAttributes"]![rel!.Name]!["guid"] = sourceEntity.Properties["guid"];
                        if (!entities.ContainsKey(qualifiedName))
                            entities.Add(qualifiedName, sourceEntity);
                    }
                    else
                    {
                        Process["relationshipAttributes"]![rel!.Name]!["guid"] = entities[qualifiedName].Properties["guid"];
                    }
                }
            }
            return Process;
        }
        private async Task<PurviewCustomType> Validate_Resource_Set(string qualifiedName)
        {
            string[] tmpName = qualifiedName.Split('/');
            string Name = tmpName[tmpName.Length - 1];
            if (Name == "")
                Name = tmpName[tmpName.Length - 2];
            string typeName = "azure_datalake_gen2_resource_set";
            PurviewCustomType sourceEntity = new PurviewCustomType(Name
            , typeName
            , qualifiedName
            , typeName
            , $"Data Assets {Name}"
            , NewGuid()
            , _logger
            , _purviewClient);

            var outputObj = await sourceEntity.QueryInPurview();
            return sourceEntity;
        }
        private bool Validate_Process_Json(JObject Process)
        {
            var _typename = get_attribute("typeName", Process);
            if (_typename == null)
            {
                Log("Info", "Not found Attribute typename on " + Process.ToString());
                return false;
            }
            var _attributes = get_attribute("attributes", Process);
            if (!_attributes.HasValues)
            {
                Log("Error", "Not found Attribute attributes on " + Process.ToString());
                return false;
            }

            if (!((JObject)Process["attributes"]!).ContainsKey("columnMapping"))
            {
                Log("Info", $"Not found Attribute columnMapping on {Process.ToString()} i is not a Process Entity!");
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Responsible to track and corelate Column Linage
        /// </summary>
        /// <param name="Process">Microsoft Purview Process entity</param>
        /// <returns>Boolean</returns>
        public bool Process_Json(JObject Process)
        {
            Hashtable cols = generate_dummy_columns(Process);
            if (cols.Keys.Count == 0)
                return false;
            return true;
        }
       
       /// <summary>
       /// Get Safe attributes in a Json object without needing to check if exists
       /// </summary>
       /// <param name="attribute_name">Name of the attribute</param>
       /// <param name="json_entity">Json Object</param>
       /// <returns>Attribute Value</returns>
        public JToken get_attribute(string attribute_name, JObject json_entity)
        {
            if (json_entity.SelectToken(attribute_name) != null)
            {
                return json_entity[attribute_name]!;
            }
            return new JObject();
        }

        // Method that looks over the section of the Json
        //that contain the relationship values (Entities and Columns)
        private Int64 NewGuid()
        {

            return initGuid--;
        }

        // 
        //         Read over the json section with the column mappings to determine
        //         all columns needed to be created and the source that each column belongs 
        //         
        private Hashtable generate_dummy_columns(JObject col_json_obj)
        {
            string source = "";
            var sink = "";
            //reference to all column mappings
            columnmapping = new Hashtable();
            PurviewCustomType sourceEntity;
            PurviewCustomType sinkEntity;
            //column mapping attribute is a string but the content is a Json, so
            //we need to load back into a json variable to be able to read it better
            //then having to parse a string.
            var mapping_values = col_json_obj["attributes"]?["columnMapping"];
            if (mapping_values?.ToString() != "")
            {
                var mapping = JArray.Parse(mapping_values!.ToString());
                foreach (var k in mapping)
                {
                    Log("debug", String.Format("generate_dummy_columns - DatasetMapping {0}", k.ToString()));
                    //each section should have a DatasetMapping with information about 
                    //column and Source and Sink.
                    if (k.SelectToken("DatasetMapping") != null)
                    {
                        source = k!["DatasetMapping"]!["Source"]!.ToString();
                        sourceEntity = entities[source];

                        sink = k!["DatasetMapping"]!["Sink"]!.ToString();
                        sinkEntity = entities[sink];

                        //initialize all mappings based on source and sink
                        // so we can attached the columns
                        if (sourceEntity.is_dummy_asset)
                        {
                            //entities.Add(sourceEntity);
                            to_purview_Json.Add(sourceEntity.Properties);
                            if (!columnmapping.ContainsKey(source))
                            {
                                columnmapping.Add(source, new Hashtable());
                                ((Hashtable)columnmapping[source]!).Add("cols", new List<string>());
                            }
                        }


                        if (sinkEntity.is_dummy_asset)
                        {
                            //entities.Add(sinkEntity);
                            to_purview_Json.Add(sinkEntity.Properties);
                            if (!columnmapping.ContainsKey(sink))
                            {
                                columnmapping.Add(sink, new Hashtable());
                                to_purview_Json.Add(sinkEntity.Properties);
                                ((Hashtable)columnmapping[sink]!).Add("cols", new List<string>());
                            }
                        }
                        Log("debug", "generate_dummy_columns - mapping columns");
                        //loop through and get all columns and attache to the Source
                        //or Sink, creating the mapping
                        foreach (var colmap in k["ColumnMapping"]!)
                        {
                            if (sourceEntity.is_dummy_asset)
                            {
                                //Log("debug", String.Format("generate_dummy_columns - mapping: {0}", colmap));
                                if (columnmapping.Contains(source))
                                {
                                    if (!((List<string>)((Hashtable)columnmapping![source!]!)["cols"]!).Contains(colmap["Source"]!.ToString()))
                                    {
                                        //((List<string>)((Hashtable)columnmapping[source])["cols"]).Add(colmap["Source"].ToString());
                                        PurviewCustomType colEntity = new PurviewCustomType(
                                            colmap["Source"]!.ToString(),
                                            "purview_custom_connector_generic_column",
                                            $"{source}#{colmap["Source"]!.ToString()}",
                                            "string",
                                            "This is a dummy column.",
                                            NewGuid()
                                            , _logger
                                            , _purviewClient
                                        );
                                        colEntity.AddToTable(sourceEntity);
                                        to_purview_Json.Add(colEntity.Properties);
                                        //entities.Add(colEntity);
                                        Log("debug", String.Format("generate_dummy_columns - Adding Col {0}", colmap["Source"]));
                                    }
                                }
                            }

                            if (sinkEntity.is_dummy_asset)
                            {
                                if (columnmapping.Contains(sink))
                                {
                                    if (!((List<string>)((Hashtable)columnmapping[sink]!)["cols"]!).Contains(colmap["Sink"]!.ToString()))
                                    {
                                        PurviewCustomType colEntity = new PurviewCustomType(
                                            colmap["Sink"]!.ToString(),
                                            "purview_custom_connector_generic_column",
                                            $"{sink}#{colmap["Sink"]!.ToString()}",
                                            "string",
                                            "This is a dummy column.",
                                            NewGuid()
                                            , _logger
                                            , _purviewClient
                                        );
                                        colEntity.AddToTable(sinkEntity);
                                        to_purview_Json.Add(colEntity.Properties);
                                        //entities.Add(colEntity);
                                        //((List<string>)((Hashtable)columnmapping[sink])["cols"]).Add(colmap["Sink"].ToString());
                                        Log("debug", String.Format("generate_dummy_columns - Adding Col {0}", colmap["Sink"]));
                                    }
                                }
                            }
                        }
                    }
                }
                //Log("info", String.Format("Source {0}, # cols: {1}", source, ((List<string>)((Hashtable)columnmapping[source])["cols"]).Count));
            }
            return columnmapping;
        }

        private void Remove_Unused_Dummy_Entitites()
        {
            foreach (var entity in this.entities)
            {

            }
        }

        private void Log(string type, string msg)
        {
            if (type.ToUpper() == "ERROR")
            { _logger.LogError(msg); return; }
            if (type.ToUpper() == "INFO")
            { _logger.LogInformation(msg); return; }
            if (type.ToUpper() == "DEBUG")
            { _logger.LogDebug(msg); return; }
            if (type.ToUpper() == "WARNING")
            { _logger.LogWarning(msg); return; }
            if (type.ToUpper() == "CRITICAL")
            { _logger.LogCritical(msg); return; }
            if (type.ToUpper() == "TRACE")
            { _logger.LogInformation(msg); return; }
        }

    }

    /// <summary>
    /// Enumeration of the Microsoft Purview Process entity relationships
    /// </summary>
    public enum Relationships_Type
    {
        inputs,
        outputs
    }
}