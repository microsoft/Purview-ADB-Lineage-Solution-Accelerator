# Troubleshooting

### Connector Issues
* [How to file a new issue](#new-issue)
* [How to debug log files](#debug-logs)

### Common Issues
* [I don't see lineage in Microsoft Purview](#no-lineage)
* [Databricks Cluster Won't Start](#cluster-fails)
* [Databricks Driver Logs: EventEmitter Could Not Emit Lineage](#driver-log-eventemitter)
* [Error Loading to Purview: 403 Forbidden](#pureviewout-load2purview)
* [Missing OpenLineage to Purview mapping data for this source](#purviewout-olmapping)
* [Error getting Authentication Token for Databricks API](#purviewout-dbr-auth)
* [Function Application Code Did Not Deploy](#appcode-not-deployed)

### Demo Deployment Issues
* [$'\r': command not found](#demo-command-not-found)
* [OpenLineage jar did not upload completely to Cloud Shell](#demo-openlineage-jar)
* [No policy exists for Purview Collection during deployment](#demo-no-policy)
* [Databricks Notebooks do not exist](#demo-no-notebooks)

### Other Issues
* [VS Code Pop-Up: "Some projects have trouble loading" when opening the folder](#vscode-popup)
* [Driver Crashing with OpenLineage Installed](#driver-crash)

-----

## <a id="new-issue" />How to file a new issue
**When filing a new issue, [please include associated log message(s) from Azure Functions](#debug-logs).** This will allow the core team to debug within our test environment to validate the issue and develop a solution. Before submitting a [new issue](https://github.com/microsoft/Purview-ADB-Lineage-Solution-Accelerator/issues), please review the known issues below, as well as the [limitations](./LIMITATIONS.md) which affect what sort of lineage can be collected.


## <a id="debug-logs" />How to debug log files
1. Open Azure Portal > Resource Group > Function App > Functions
1. Select either the `OpenLineageIn` or `PurviewOut` function
1. Click `Monitor` in the left-hand menu
    1. Click the linked timestamps within `Invocation Traces` to view details of past events
    1. Click the `Logs` tab to view the live events. (*Ensure both connected and timestamped welcome messages appear.*)

## <a id="cluster-fails" />Databricks Cluster Won't Start

* ### Init Script Fails

    When installing the OpenLineage init script and Jar, the Databricks cluster will not start. You may receive a Databricks Event Log event indicating the init script failed.

    **Solution**: Ensure that the init script uploaded to Databricks uses Line Feed (LF) and does not use Carriage Returns (CRLF). If you are using Windows, your development environment may default to CRLF which is not accepted on Databricks. To fix this, download / edit your init script and use an IDE that supports changing the line endings to LF. For example, VS Code and Notepad++ indicate the line endings in the bottom right corner of the window. Select `CRLF` and change it to `LF`. Save the file and upload to DBFS.

    If the file is already LF, confirm that the OpenLineage Jar was uploaded properly. Ensure its location matches your init script (expected location is `/dbfs/databricks/openlineage`) and that the file pattern is correct (default is `openlineage-spark-*.jar`). If you uploaded the OpenLineage jar via the Databricks UI / DBFS UI, it may have replaced hyphens (-) with underscores (_) causing the wildcard pattern to fail.

    In this case, use the databricks CLI to upload the jar to the expected location to avoid changes in the file name.

* ### Internal Error Resolving Secrets

    For the demo deployment, if your cluster fails and returns the error "Internal Error resolving secrets" and "Failed to fetch secrets referred to in Spark Conf", the deployment script may have failed to add an Access Policy to the Azure Key Vault or the secret scope was not created.

    **Solution**: Update the values in the below script and execute it in the cloud shell. This script deletes the demo deployment's secret scope and then recreates it. After executing the script, you should see an access policy for "AzureDatabricks" in your Azure Key Vault.

    ```bash
    adb_ws_url=adb-DATABRICKS_WORKSPACE.ID.azuredatabricks.net
    global_adb_token=$(az account get-access-token --resource 2ff814a6-3304-4ab8-85cb-cd0e6f879c1d -o tsv --query '[accessToken]')
    adb_ws_id=/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.Databricks/workspaces/DATABRICKS_WORKSPACE_NAME
    subscription_id=123acb-456-def
    akv_name=AKV_NAME
    akv_resource_id=/subscriptions/SUBSCRIPTION_ID/resourceGroups/RESOURCE_GROUP_NAME/providers/Microsoft.KeyVault/vaults/AKV_NAME

    # Remove the Secret Scope if it exists
    cat << EOF > delete-scope.json
    {
    "scope": "purview-to-adb-kv"
    }
    EOF

    curl \
    -X POST https://$adb_ws_url/api/2.0/secrets/scopes/delete \
    -H "Authorization: Bearer $global_adb_token" \
    -H "X-Databricks-Azure-Workspace-Resource-Id: $adb_ws_id" \
    --data @delete-scope.json

    # If the above fails, that's okay
    # Ultimately, we just need a clean slate

    cat << EOF > create-scope.json
    {
    "scope": "purview-to-adb-kv",
    "scope_backend_type": "AZURE_KEYVAULT",
    "backend_azure_keyvault":
    {
        "resource_id": "$akv_resource_id",
        "dns_name": "https://$akv_name.vault.azure.net/"
    },
    "initial_manage_principal": "users"
    }
    EOF


    curl \
    -X POST https://$adb_ws_url/api/2.0/secrets/scopes/create \
    -H "Authorization: Bearer $global_adb_token" \
    -H "X-Databricks-Azure-Workspace-Resource-Id: $adb_ws_id" \
    --data @create-scope.json
    ```

## <a id="no-lineage" />I don't see lineage in Microsoft Purview

* ### Try Refreshing the Page

    Microsoft Purview tends to cache the results to improve query response time. You may not see the results in the Purview UI immediately.

    **Solution**: Refresh the page using the Refresh button in the Purview UI. You may consider returning to the page after a minute or two and attempting the refresh again.

* ### Confirm the Azure Function Code Was Deployed

    Navigate to the Azure Function and check if you are receiving the alert `Microsoft.Azure.WebJobs.Extensions.FunctionMetadataLoader: The file 'C:\home\site\wwwroot\worker.config.json' was not found.`.
    
    **Solution**: Restart or start and stop the function to resolve the issue. If the issue persists, consider deploying the function code via [VS Code](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs-code?tabs=csharp) or the [Azure CLI](https://docs.microsoft.com/en-us/azure/azure-functions/deployment-zip-push#cli).

* ### Check Azure Key Vault References Are Active

    The connector uses [Key Vault References](https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references) inside the Azure Functions used to translate OpenLineage to Apache Atlas standards. When first launching the services, the Key Vault references may not have activated / synced.  You will see red "x" marks in the [Function App's Configuration menu](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings?tabs=portal).

    **Solution**: Wait two to five minutes after deploying the connector and check back to confirm that the Function can communicate with the Key Vault. If the connection still shows red "x" marks, confirm that the [Key Vault has an access policy](https://docs.microsoft.com/en-us/azure/key-vault/general/assign-access-policy?tabs=azure-portal) for your Azure Function's managed identity.

* ### Confirm that Custom Types Are Available

    If you have deployed the demo or connector solution, the custom types used by the connector may not have been uploaded.

    **Solution**: Follow the [post-installation step](./deploy-base.md#post-installation) and confirm that the types are created. If the types have not been created already, you will receive a JSON payload of the created types (e.g. `{"enumDefs":[], "classificationDefs":[], "entityDefs":[...], ... }`).  If the types already exist, you will receive a message indicating they exist already.

* ### Confirm that you are NOT using Spark Streaming

    Spark Structured Streaming and Spark DStreams are [not supported in this release](./LIMITATIONS.md#spark-streaming) of the solution accelerator.

## <a id="driver-log-eventemitter" />Databricks Driver Logs: EventEmitter Could Not Emit Lineage

When reviewing the Driver logs, you see an error in the Log4j output that indicates the EventEmitter class had an exception and could not emit lineage.  You do not see any events in `OpenLineageIn` or `PurviewOut` functions either.

**Solution**: This indicates a problem connecting to the Azure Function from Databricks.

* If you are using OpenLineage 1.1.0 or higher, make sure the following spark configs are set:

    ```
    spark.openlineage.transport.type http
    spark.openlineage.transport.endpoint api/v1/lineage
    spark.openlineage.namespace <ADB-WORKSPACE-ID>#<DB_CLUSTER_ID>
    spark.openlineage.transport.url https://<FUNCTION_APP_NAME>.azurewebsites.net
    spark.openlineage.transport.urlParams.code <FUNCTION_APP_DEFAULT_HOST_KEY>
    ```

* If you are using Openlineage 0.18.0 or lower, Confirm `spark.openlineage.transport.urlParams.code` and `spark.openlineage.host` values are set and correct.
* Confirm that the Azure Function is currently on and has the correct API routes for OpenLineageIn
* Confirm that `spark.openlineage.version` is set correctly.

    |SA Release|OpenLineage Jar|spark.openlineage.version|spark.openlineage.transport.endpoint|
    |-----|----|----|
    |1.0.x|0.8.2|1|not set|
    |1.1.x|0.8.2|1|not set|
    |2.0.x - 2.3.1|0.11.0 - 0.18.0|v1|not set|
    |2.4.x or newer|1.1.0 or newer|not set|api/v1/lineage|

## <a id="pureviewout-load2purview" />PurviewOut Logs: Error Loading to Purview: 403 Forbidden

When reviewing the Purview Out function logs, you see an error that indicates there was an error loading assets to Microsoft Purview. The errors looks similar to `Error Loading to Purview: Return Code: 403 - Reason:Forbidden`

**Solution**: This indicates authorization is not correct for the Service Principal.

* Your ClientId, ClientSecret or Certificate values are correct
* Certificate should be of the form: `{"SourceType": "KeyVault","KeyVaultUrl": "https://akv-service-name.vault.azure.net/","KeyVaultCertificateName": "myCertName"}`
* You have given the Service Principal permission to access Microsoft Purview ( [auth using a service principal](https://docs.microsoft.com/en-us/azure/purview/tutorial-using-rest-apis#set-up-authentication-using-service-principal) )

## <a id="purviewout-olmapping" />PurviewOut Logs: Missing Ol to Purview mapping data for this source

You may be working with data sources that are supported by OpenLineage but not supported by the Solution Accelerator for ingestion into Purview.

**Solution**: 
* Confirm that the OlToPurviewMappings app setting is populated and matches your release’s version of [OlToPurviewMappings.json](https://github.com/microsoft/Purview-ADB-Lineage-Solution-Accelerator/blob/release/1.1/deployment/infra/OlToPurviewMappings.json)
* Review the Databricks Driver Logs and identify the namespace authority (e.g. jdbc, abfss, dbfs) for the OpenLineage Inputs/Outputs being emitted
* Look for the json after the EventEmitter logging statement. Then look at the inputs / outputs field in the emitted JSON.
* If the authority is not found in the OlToPurviewMappings JSON, it is an unsupported type. Consider [modifying the OlToPurviewMappings to your need](./extending-source-support.md).

## <a id="purviewout-dbr-auth" />Error getting Authentication Token for Databricks API

The Service Principal is unable to retrieve an access token for Databricks.

**Solution**: Ensure the Service Principal is a user in the Databricks workspace.
* [Add your Service Principal to Databricks](https://docs.microsoft.com/en-us/azure/databricks/dev-tools/api/latest/scim/scim-sp#add-service-principal)
* [Assign the Service Principal as a contributor to the Databricks Workspace](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-assignments-portal?tabs=current)


## <a id="appcode-not-deployed" />Function Application Code Did Not Deploy

When running the `openlineage-deployment.sh` script or `newdeploymenttemp.json`, your deployment succeeds but the Azure Function deployed does not include the OpenLineageIn or PurviewOut functions.

Solution: First, attempt the deployment a second time. It may just be an intermittent issue. Secondly, if the Function service deployed successfully (including the app settings), you may consider manually deploying the Azure Function application code.

* Download the latest release zip file (see list of all releases on Github).
* Using the Azure CLI to deploy the zip file to the Function service: 
  ```
  az functionapp deployment source config-zip \
  -g <RESOURCE_GROUP_NAME> \
  -n <FUNCTION_SERVICE_NAME> \
  --src <PATH_TO_RELEASE_ZIP_FILE>
  ```

## <a id="demo-command-not-found" />Demo Deployment: $'\r': command not found

When running the `openlineage-deployment.sh` script, you received either `$'\r': command not found` or `syntax error near unexpected token $'\r'`. This can occur when you have used git to clone the shell script (.sh) to a Windows OS file system with [git core.autocrlf enabled](https://docs.github.com/en/get-started/getting-started-with-git/configuring-git-to-handle-line-endings). This will only occur if you cloned the repo locally rather than using the Bash cloud shell.

**Solution**: Use the [Azure Cloud shell](https://docs.microsoft.com/en-us/azure/cloud-shell/overview) to do your deployment. Alternatively, if you must use a Windows OS machine, use your preferred file editor to remove `\r\n` and replace with `\n`. Lastly, you may consider using the zip file download rather than the git clone option provided by GitHub.

## <a id="demo-openlineage-jar" />Demo Deployment: OpenLineage jar did not upload completely to Cloud Shell

When uploading the OpenLineage jar to the cloud shell, it may not complete the upload due to a transient issue. You may receive a message such as `cannot access '*.jar': No such file or directory` when running `openlineage-deployment.sh`. 

**Solution**: Attempt the jar upload to the cloud shell again and confirm the jar has successfully uploaded. Re-run the `openlineage-deployment.sh` script.

## <a id="demo-no-policy" />Demo Deployment: No policy exists for Purview Collection during deployment

This means the current person running the script could not be added as a Purview Collection Admin, either because insufficient permission or because the Purview is still being deployed with some additional resources in backend.

**Solution**: If you don't have the correct permissions, consider requesting an existing Purview Collection Admin to [add you as a collection admin](https://docs.microsoft.com/en-us/azure/purview/how-to-create-and-manage-collections#add-role-assignments).

## <a id="demo-no-notebook" />Demo Deployment: No notebooks exist after deployment

The demo deployment appears to be successful but when reviewing the sample notebook in the Databricks workspace they are either missing or return a 404: Not Found in the Databricks UI.

**Solution**: Manually [import the notebook](https://docs.microsoft.com/en-us/azure/databricks/notebooks/notebooks-manage#import-a-notebook) found at the `deployment\deployment-assets\openlineage_sample.scala` location.

## <a id="vscode-popup" />VS Code Pop-Up: "Some projects have trouble loading" when opening the folder.

When opening the cloned repo in VS Code, you may see a pop up saying "Some projects have trouble loading". This is due to Auth libraries not yet being specifically available for dotnet 6.0.

**Solution**: You can safely ignore these warnings.

## <a id="driver-crash" />Driver Crashes When Using OpenLineage

### When using SaveAsTable and Overwrite Mode

When using OpenLineage `0.11.0` with Databricks Runtime `10.4` and executing a command like `df.write.mode("overwrite").saveAsTable("default.mytable")`, the driver crashes. This is due to a bug in OpenLineage which did not separate out certain commands for Spark 3.2 vs Spark 3.1.

**Solution**: Upgrade to OpenLineage `0.13.0` which has a fix for the issue.
