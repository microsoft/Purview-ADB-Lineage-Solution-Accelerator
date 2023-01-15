# Deploying the Test Environment

## Deploying the Connector

## Deploying the Data Sources

```
az deployment group create \
--template-file ./tests/environment/sources/adlsg2.bicep \
--resource-group db2pvsasources

```

## Manual Steps

Create a config.ini file:

```ini
databricks_workspace_host_id = adb-workspace.id
databricks_personal_access_token = PERSONAL_ACCESS_TOKEN
databricks_spark3_cluster = CLUSTER_ID
databricks_spark2_cluster = CLUSTER_ID
```

Assign Service Principal Storage Blob Data Contributor to the main ADLS G2 instance

Add Service Principal as user in Databricks.

Enable mount points with `./tests/environment/dbfs/mounts.py`

Install mysql:mysql-connector-java:8.0.30 (version may vary based on cluster config) on the cluster.
Create/reuse a Service Principal for Azure Data Explorer Authentication. Create and save a secret locally.

Add Key Vault Secrets
  * `tenant-id`
  * `storage-service-key`
  * `azuresql-username`
  * `azuresql-password`
  * `azuresql-jdbc-conn-str` should be of the form `jdbc:sqlserver://SERVER_NAME.database.windows.net:1433;database=DATABASE_NAME;encrypt=true;trustServerCertificate=false;hostNameInCertificate=*.database.windows.net;loginTimeout=30;`
  * `synapse-storage-key`
  * `synapse-query-username`
  * `synapse-query-password`
  * `mysql-username` of the form `username@servername`
  * `mysql-password`
  * `mysql-hostname` the server name of the Azure MySQL resource
  * `postgres-admin-user` should be of the form `username@servername`
  * `postgres-admin-password`
  * `postgres-host` - the server name of the deployed postgres server
  * `azurekusto-appid`
  * `azurekusto-appsecret`
  * `azurekusto-uri`
  
  * `azurecosmos-endpoint`
  * `azurecosmos-key`
* Update SQL Db and Synapse Server with AAD Admin
* Add Service Principal for Databricks to connect to SQL sources
* Assign the Service Principal admin role on the ADX cluster. [Guide](https://learn.microsoft.com/en-us/azure/data-explorer/provision-azure-ad-app#grant-the-service-principal-access-to-an-azure-data-explorer-database)

Set the following system environments:

* `SYNAPSE_SERVICE_NAME`
* `STORAGE_SERVICE_NAME`
* `SYNAPSE_STORAGE_SERVICE_NAME`

Install the version of the [kusto spark connector](https://github.com/Azure/azure-kusto-spark) that matches the cluster Scala and Spark versions from Maven Central.

Upload notebooks in `./tests/integration/spark-apps/notebooks/` to dbfs' `/Shared/examples/`
* Manually for now. TODO: Automate this in Python

Install the following libraries on the compute cluster (versions to match the Spark and Scala versions of the cluster) (TODO: Automate):
* Cosmos spark connector


Compile the following apps and upload them to `/dbfs/FileStore/testcases/`

* `./tests/integration/spark-apps/jarjobs/abfssInAbfssOut/` with `./gradlew build`
* `./tests/integration/spark-apps/pythonscript/pythonscript.py` by just uploading.
* `./tests/integration/spark-apps/wheeljobs/abfssintest/` with `python -m build`

Upload the job definitions using the python script `python .\tests\environment\dbfs\create-job.py`

## Github Actions

* AZURE_CLIENT_ID
* AZURE_CLIENT_SECRET
* AZURE_TENANT_ID
* INT_AZ_CLI_CREDENTIALS
  ```json
  {
    "clientId": "xxxx",
    "clientSecret": "yyyy",
    "subscriptionId": "zzzz",
    "tenantId": "μμμμ",
    "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
    "resourceManagerEndpointUrl": "https://management.azure.com/",
    "activeDirectoryGraphResourceId": "https://graph.windows.net/",
    "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
    "galleryEndpointUrl": "https://gallery.azure.com/",
    "managementEndpointUrl": "https://management.core.windows.net/"
  }
  ```
* INT_DATABRICKS_ACCESS_TOKEN
* INT_DATABRICKS_WKSP_ID: adb-xxxx.y
* INT_FUNC_NAME
* INT_PUBLISH_PROFILE from the Azure Function's publish profile XML
* INT_PURVIEW_NAME
* INT_RG_NAME
* INT_SUBSCRIPTION_ID
* INT_SYNAPSE_SQLPOOL_NAME
* INT_SYNAPSE_WKSP_NAME
* INT_SYNAPSE_WKSP_NAME

## config.json

```json
{
  "datasets":{
    "datasetName": {
      "schema": [
        "field1",
        "field2"
      ],
      "data": [
        [
          "val1",
          "val2"
        ]
      ]
    }
  },
  "jobs": {
    "job-name": [
      [
        ("storage"|"sql"|"noop"),
        ("csv"|"delta"|"azuresql"|"synapse"),
        "rawdata/testcase/one/",
        "exampleInputA"
      ]
    ]
  }
}

```
