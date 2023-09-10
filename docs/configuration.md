# Advanced Configuration Settings

## App Settings

The following are the stable app settings that should be reviewed.

| App Setting| Default Value|Required in App Settings| Note|
|----|----|----|----|
|PurviewAccountName| Empty String | Yes| The name of the Purview instance that will receive Databricks lineage|
|TenantId|Defaults to Resource Group tenant during ARM deployment|Yes||
|ClientID|Provided during ARM deployment|Yes|The Application Id of the service principal with permission to read call Databricks and Microsoft Purview APIs|
|ClientSecret|Provided during ARM deployment|*Yes|The secret value for the service principal provided in ClientID|
|Certificate|Not Applicable|*No|Should be a JSON object: `{"SourceType": "KeyVault","KeyVaultUrl": "https://akv-name.vault.azure.net/","KeyVaultCertificateName": "certificateName"}`
|ResourceUri | <https://purview.azure.com> | No| |
|AuthEndPoint| <https://login.microsoftonline.com/>| No| |
|AuthenticationUri| purview.azure.net| No| |
|EventHubName|Not Applicable|Yes| |
|ListenToMessagesFromEventHub|Not Applicable|Yes| |
|SendMessagesToEventHub|Not Applicable|Yes| |
|EventHubConsumerGroup|read|Yes| The name of the consumer group that triggers the azure function|
|OlToPurviewMappings|Not Applicable|Yes| |

* \* ClientSecret is required if Certificate is not provided.
  * If Certificate is provided, it will supersede the ClientSecret app setting.

### Experimental App Settings

The following app settings are experimental and may be removed in future releases.

| App Setting| Default Value in Code| Note|
|----|----|----|
|useResourceSet|true|Experimental feature|
|maxQueryPlanSize|null|If the query plan bytes is greater than this value it will be removed from the databricks_process|
|prioritizeFirstResourceSet|true|When matching against existing assets, the first resource set found will be prioritized over other assets like folders or purview custom connector entities.|
|Spark_Entities|databricks_workspace;databricks_job;databricks_notebook;databricks_notebook_task||
|Spark_Process|databricks_process||
|purviewApiEndPoint|{ResourceUri}/catalog/api||
|purviewApiSearchAdvancedMethod|/atlas/v2/search/advanced||
|purviewApiEntityByGUIDMethod|/atlas/v2/entity/guid/||
|purviewApiEntityByTypeMethod|/atlas/v2/entity/bulk/uniqueAttribute/type/||
|purviewApiEntityBulkMethod|/atlas/v2/entity/bulk||
|purviewApiQueryMethod|/search/query?api-version=2021-05-01-preview||

## Spark Configuration

For Openlineage-Spark 1.1.0+:

|Configuration|Value|Config Location| Note|
|----|----|----|---|
|spark.openlineage.transport.type| http |*Cluster Spark Config or Init Script||
|spark.openlineage.transport.url| <https://FUNCTION_APP_NAME.azurewebsites.net>|Cluster Spark Config or Init Script||
|spark.openlineage.transport.endpoint| http |*Cluster Spark Config or Init Script||
|spark.openlineage.transport.urlParams.code| Function Key |*Cluster Spark Config or Init Script||
|spark.openlineage.namespace| WORKSPACE_ID#CLUSTER_ID |Cluster Spark Config or Init Script||

For OpenLineage 0.18.0 or lower:

|Configuration|Value|Config Location| Note|
|----|----|----|---|
|spark.openlineage.transport.urlParams.code| Function Key |*Cluster Spark Config or Init Script||
|spark.openlineage.host| <https://FUNCTION_APP_NAME.azurewebsites.net>|Cluster Spark Config or Init Script||
|spark.openlineage.namespace| WORKSPACE_ID#CLUSTER_ID |Cluster Spark Config or Init Script||
|spark.openlineage.version| v1 |Cluster Spark Config or Init Script| Should be v1 for OpenLineage jar 0.9.0+. Should be 1 for OpenLineage jar 0.8.2 and earlier.|

* \* If you are using a Databricks secret for your function key, the `url.param.code` must be set in the Cluster Spark Config.
  * Databricks does not support using Databricks secrets in a global init script.
