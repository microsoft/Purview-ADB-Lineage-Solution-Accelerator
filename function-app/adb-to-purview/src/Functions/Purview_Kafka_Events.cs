using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Function.Domain.Models;
using Function.Domain.Helpers;
using Function.Domain.Models.Purview;
using Microsoft.Azure.Functions.Worker;
using Function.Domain.Models.Settings;

namespace AdbToPurview.Function
{
    public class Purview_Kafka_Events
    {
        private PurviewClient? _purviewClient;
        private readonly ILogger<Purview_Kafka_Events?> _logger;
        private readonly ILogger _log;
        private FilterExeption? filterExeptions;
        private AppConfigurationSettings config = new AppConfigurationSettings();
        public Purview_Kafka_Events(ILogger<Purview_Kafka_Events> logger)
        {
            logger.LogInformation("Start Purview_Kafka_Events");
            filterExeptions = JsonConvert.DeserializeObject<FilterExeption>(config!.FilterExeption!);
            _logger = logger;
            _log = logger;
            _purviewClient = new PurviewClient(_log);
        }

        private async Task Update_Relatioship_InputorOutPut(Purview_Kafka_Return messageReturn, Asset item, JArray relatioships, string relType)
        {

            bool scannedItemFound = false;
            foreach (JObject relationship in relatioships)
            {
                JObject process = await _purviewClient!.GetByGuid(relationship!["guid"]!.ToString());
                scannedItemFound = false;
                foreach (JObject relatioship in process["entity"]!["attributes"]![relType]!)
                {
                    if (relatioship["guid"]!.ToString() == item.guid)
                    {
                        relatioship["guid"] = messageReturn!.message!.entity!.guid;
                        relatioship["typeName"] = messageReturn!.message!.entity!.typeName;
                        relatioship["uniqueAttributes"]!["qualifiedName"] = messageReturn!.message!.entity!.attributes!.qualifiedName;
                        scannedItemFound = true;
                        break;
                    }
                }
                if (scannedItemFound)
                {
                    _logger.LogInformation($"Processed guid:{relationship!["guid"]!} will be updated with the new scanned Data entity guid:{messageReturn!.message!.entity!.guid}");
                    process!["entity"]!["relationshipAttributes"]!.SelectToken(relType)!.Parent!.Remove();
                    string payLoad = "{\"entities\": [" + process!["entity"]!.ToString() + "]}";
                    var updateProcess = await _purviewClient.Send_to_Purview(payLoad);
                    _logger.LogInformation($"Processed guid:{relationship!["guid"]!} updated");
                    bool deleted = await _purviewClient.Delete_Unused_Entity(item!.attributes!["qualifiedName"]!.ToString()!, "purview_custom_connector_generic_entity_with_columns");
                    if (deleted)
                        _logger.LogInformation($"Deleted guid:{item.guid}");
                    break;
                }
            }

        }
        private async Task Update_Process_Relatioship(PurviewCustomType sourceEntity, Purview_Kafka_Return messageReturn)
        {
            List<Asset> sourceAsset = await sourceEntity.GetEntity();
            await Update_Process_Relatioship(sourceAsset, messageReturn);
        }
        private async Task Update_Process_Relatioship(List<Asset> sourceAsset, Purview_Kafka_Return messageReturn)
        {
            foreach (Asset item in sourceAsset)
            {
                JArray inputs = (JArray)item.relationshipAttributes!["inputToProcesses"];
                JArray outputs = (JArray)item.relationshipAttributes!["outputFromProcesses"];
                if (inputs.Count > 0)
                    await Update_Relatioship_InputorOutPut(messageReturn!, item!, inputs!, "inputs");
                if (outputs.Count > 0)
                    await Update_Relatioship_InputorOutPut(messageReturn!, item!, outputs!, "outputs");
            }

        }

        private bool Validade_ADF_Relationship(JObject folder)
        {
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
            return false;

        }

        [Function("Purview_Kafka_Events")]
        public async Task Run([EventHubTrigger("%KafkaName%", IsBatched = false, Connection = "ListenToMessagesFromPurviewKafka", ConsumerGroup = "%KafkaConsumerGroup%")] string messageBody)
        {
            var exceptions = new List<Exception>();
            bool gotMessageBody = false;
            bool gotmessageReturn = false;
            try
            {
                gotMessageBody = true;
                Purview_Kafka_Return messageReturn = JsonConvert.DeserializeObject<Purview_Kafka_Return>(messageBody)!;
                gotmessageReturn = true;
                _logger.LogInformation($"Event Hub trigger function got a message for qualifiedName:{messageReturn!.message!.entity!.attributes!.qualifiedName}");

                if ((messageReturn.message.operationType == "ENTITY_CREATE") || (messageReturn.message.operationType == "ENTITY_UPDATE"))
                {
                    if ((!filterExeptions!.IsExeption(messageReturn!.message!.entity!.typeName!)) && (messageReturn.message.entity.typeName != "spark_process") && (messageReturn.message.entity.typeName != "purview_custom_connector_generic_entity_with_columns"))
                    {
                        string qualifiedName = messageReturn!.message!.entity!.attributes!.qualifiedName!.ToLower().Trim('/');
                        string Name = messageReturn!.message!.entity!.attributes!.name!;
                        string typename = "purview_custom_connector_generic_entity_with_columns";
                        PurviewCustomType sourceEntity = new PurviewCustomType(Name!
                             , typename
                             , qualifiedName
                             , messageReturn!.message!.entity!.typeName!
                             , $"Data Assets {Name}"
                             , -1000
                             , _log
                             , _purviewClient!);
                        QueryValeuModel returns = await sourceEntity.QueryInPurview("purview_custom_connector_generic_entity_with_columns");
                        if (returns.id != null)
                        {
                            await Update_Process_Relatioship(sourceEntity, messageReturn);
                        }
                        else
                        {
                            if ((messageReturn!.message!.entity!.typeName == "azure_datalake_gen2_resource_set") ||
                            (messageReturn!.message!.entity!.typeName == "azure_blob_resource_set"))
                            {
                                // In case of 
                                sourceEntity.Properties["typeName"] = messageReturn!.message!.entity!.typeName;
                                QueryValeuModel allreturns = await sourceEntity!.QueryInPurview();
//                                if (allreturns.Count > 0)
//                                {
//                                    foreach (QueryValeuModel returnValue in allreturns)
//                                    {
                                        JObject currentEntity = await _purviewClient!.GetByGuid(allreturns.id);
                                        if (!Validade_ADF_Relationship(currentEntity))
                                        {
                                            List<Asset> listEntities = new List<Asset>();
                                            listEntities.Add(JsonConvert.DeserializeObject<Asset>(currentEntity["entity"]!.ToString())!);
                                            await Update_Process_Relatioship(listEntities, messageReturn);
                                        }
//                                    }
//                                }
                            }
                        }

                    }
                }
                await Task.Yield();
            }
            catch (Exception e)
            {
                // We need to keep processing the rest of the batch - capture this exception and continue.
                // Also, consider capturing details of the message that failed processing so it can be processed again later.
                if (!gotMessageBody)
                    exceptions.Add(new Exception("Can't get eventData messageBody"));
                if (!gotmessageReturn)
                    exceptions.Add(new Exception("Can't DeserializeObject<Purview_Kafka_Return>(messageBody)"));

                exceptions.Add(e);
            }


            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
            {
                _logger.LogInformation($"Error reading from Purview kafka: {(new AggregateException(exceptions)).Message}");
                throw new AggregateException(exceptions);
            }

            if (exceptions.Count == 1)
                _logger.LogInformation($"Error reading from Purview kafka: {exceptions.Single().Message}");
        }
    }
}
