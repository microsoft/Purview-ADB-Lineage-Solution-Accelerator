<!-- markdownlint-disable MD033 - HTML rule -->
# Deployment of the Purview ADB Lineage Solution Accelerator

When installed as a working connector, your data sources, Azure Databricks, and Microsoft Purview are assumed to be setup and running.

## Services Installed
  
![high-level-architecture.svg](./assets/img/deploy/BaseSvcsInstalled.svg)

## Prerequisites
Installing the base connector requires that you have already configured [Databricks CLI with the Azure Databricks platform](https://docs.microsoft.com/en-us/azure/databricks/dev-tools/cli/).

## Deployment Steps
1. [Clone the repository into Azure cloud shell](#clone-repo)
1. [Run the installation script](#run-script)
1. [Post Installation](#post-install)
1. [Download and configure OpenLineage Spark agent with your Azure Databricks clusters](#download-openlineage)
1. [Install OpenLineage on Your Databricks Cluster](#configure-openlineage)
1. [Support Extracting Lineage from Databricks Jobs](#jobs-lineage)
1. Optional [Configure Global Init Script](#global-init)

## <a id="clone-repo" />Clone the repository into Azure cloud shell

From the [Azure Portal](https://portal.azure.com)

1. At the top of the page, click the Cloud Shell icon

    ![ShellIcon.png](./assets/img/deploy/ShellIcon.png)

    ![SelectPowershell.png](./assets/img/deploy/SelectPowerShell.png) 

    Click "Confirm" if the "Switch to PowerShell in Cloud Shell" pop up appears.

    ![CloudShellConfirm.png](./assets/img/deploy/CloudShellConfirm.png)

1. Change directory and clone this repository into the `clouddrive` directory using the latest [release tag](https://github.com/microsoft/Purview-ADB-Lineage-Solution-Accelerator/tags) (i.e. `2.x.x`). *If this directory is not available please follow these steps to [mount a new clouddrive](https://docs.microsoft.com/en-us/azure/cloud-shell/persisting-shell-storage#mount-a-new-clouddrive)*

    ```powershell
    cd clouddrive
    git clone -b <release_tag> https://github.com/microsoft/Purview-ADB-Lineage-Solution-Accelerator.git
    ```

    > **Note**:<br/>
    > We highly recommend cloning from the [release tags listed here](https://github.com/microsoft/Purview-ADB-Lineage-Solution-Accelerator/tags).
    >
    > **Clone the *main* branch only when using nightly builds.**
    > By using a nightly build (i.e. the latest commit on main), you gain access to newer / experimental features, however those features may change before the next official release. If you are testing a deployment for production, please clone using [release tags](https://github.com/microsoft/Purview-ADB-Lineage-Solution-Accelerator/tags).

-----



## <a id="run-script" />Run the installation script

1. Set the Azure subscription you want to use:
    ```powershell
    az account set --subscription <SubscriptionID>
    ```

1. *If needed, create a new working Resource Group:*
    ```powershell
    az group create --name <ResourceGroupName> --location <ResourceGroupLocation>
    ```

1. Change into the deployment directory (you should already be in the ~/clouddrive directory):
    ```powershell
    cd Purview-ADB-Lineage-Solution-Accelerator/deployment/infra/
    ```
    > **Note**:<br/>
    > If your organization requires private endpoints for Azure Storage and Azure Event Hubs, you may need to follow the [private endpoint guidance](./docs/private-endpoints.md) and modify the provided arm template.

1. Deploy solution resources:
    ```powershell
    az deployment group create \
    --resource-group <ResourceGroupName> \
    --template-file "./newdeploymenttemp.json" \
    --parameters purviewName=<ExistingPurviewServiceName>
    ```
    * A prompt will ask you to provide the following:
        * Prefix (this is added to service names)
        * Client ID & Secret (from the App ID required as a [prerequisite](https://github.com/microsoft/Purview-ADB-Lineage-Solution-Accelerator#prerequisites))
        * Resource Tags (optional, in the following format: `{"Name":"Value","Name2":"Value2"}`, otherwise leave blank)
        * This deployment will take approximately 5 minutes.

> **Note**:<br/>
> At this point, you should confirm resources deployed successfully.
> In particular, check the Azure Function and inside its Functions tab, you should see an OpenLineageIn and PurviewOut function. If you have an error like `Microsoft.Azure.WebJobs.Extensions.FunctionMetadataLoader: The file 'C:\home\site\wwwroot\worker.config.json' was not found.` please restart or start and stop the function to resolve the issue.
> Lastly check the Azure Function Configuration tab and check if all the Key Vault Referenced app settings have a green checkmark. If not, wait an additional 2-5 minutes and refresh the screen. If Key Vault references are not all green, check that the Key Vault has an access policy referencing the Azure Function.

## <a id="post-install" />Post Installation

1. If needed, change into the deployment directory:
    ```bash
    cd ~clouddrive/Purview-ADB-Lineage-Solution-Accelerator/deployment/infra/
    ```

1. *(Manual Configuration)* After the installation finishes, you will need to add the service principal to the data curator role in your Purview resource.  Follow this documentation to [Set up Authentication using Service Principal](https://docs.microsoft.com/en-us/azure/purview/tutorial-using-rest-apis#set-up-authentication-using-service-principal) using the Application Identity you created as a prerequisite to installation.

1. Install necessary types into your Purview instance by running the following commands in Bash.

    You will need:
    * your Tenant ID
    * your Client ID - used when you ran the installation script above
    * your Client Secret - used when you ran the installation script above

  ```bash
  purview_endpoint="https://<enter_purview_account_name>.purview.azure.com"
  TENANT_ID="<TENANT_ID>" 
  CLIENT_ID="<CLIENT_ID>" 
  CLIENT_SECRET="<CLIENT_SECRET>"

  acc_purview_token=$(curl https://login.microsoftonline.com/$TENANT_ID/oauth2/token --data "resource=https://purview.azure.net&client_id=$CLIENT_ID&client_secret=$CLIENT_SECRET&grant_type=client_credentials" -H Metadata:true -s | jq -r '.access_token')
  purview_type_resp_custom_type=$(curl -s -X POST $purview_endpoint/catalog/api/atlas/v2/types/typedefs \
          -H "Authorization: Bearer $acc_purview_token" \
          -H "Content-Type: application/json" \
          -d @Custom_Types.json )

  echo $purview_type_resp_custom_type
  ```
  
  If you need a Powershell alternative, see the [docs](./docs/powershell-alternatives.md#upload-custom-types).

## <a id="download-openlineage" />Download the OpenLineage Spark agent and configure with your Azure Databricks clusters

You will need the default API / Host key configured on your Function app. To retrieve this:

1. To retrieve the ADB-WORKSPACE-ID, in the Azure portal, navigate to the Azure DataBricks (ADB) service in your resource group. In the overview section, copy the ADB workspace identifier from the URL (as highlighted below).

    ![GetAdbEndpoint.png](./assets/img/deploy/GetAdbEndpoint.png)

1. To retrieve the FUNCTION_APP_NAME, go back to the resource group view and click on the Azure Function. On the Overview tab, copy the `URL` and save it for the next steps.
1. To retrieve the FUNCTION_APP_DEFAULT_HOST_KEY, go back to the resource group view and click on the Azure Function. In the right pane go to 'App Keys', then click on the show values icon and copy the `default` key.

    ![FunctionKeys.png](./assets/img/deploy/FunctionKeys.png)

### <a id="configure-openlineage" />Install OpenLineage on Your Databricks Cluster

Follow the instructions below and refer to the [OpenLineage Databricks Install Instructions](https://github.com/OpenLineage/OpenLineage/tree/main/integration/spark/databricks#databricks-install-instructions) to enable OpenLineage in Databricks.
1. Download the [OpenLineage-Spark 0.18.0 jar](https://repo1.maven.org/maven2/io/openlineage/openlineage-spark/0.18.0/openlineage-spark-0.18.0.jar) from Maven Central
2. Create an init-script named `open-lineage-init-script.sh`

    ```text
    #!/bin/bash
    STAGE_DIR="/dbfs/databricks/openlineage"
    cp -f $STAGE_DIR/openlineage-spark-*.jar /mnt/driver-daemon/jars || { echo "Error copying Spark Listener library file"; exit 1;}
    cat << 'EOF' > /databricks/driver/conf/openlineage-spark-driver-defaults.conf
    [driver] {
        "spark.extraListeners" = "io.openlineage.spark.agent.OpenLineageSparkListener"
    }
    EOF
    ```

    > **Warning**
    > If you are using a windows machine, be sure that you save the init script with linux Line Feed (LF) ending and NOT Microsoft Windows Carriage Return and Line Feed (CRLF) endings.
    > Using a tool like VS Code or Notepad++, you may change the line endings by selecting CRLF/LF in the bottom right hand corner of the editor.
    > If you do not have line feed endings, your cluster will fail to start due to an init script error.

3. Upload the init script and jar to dbfs using the [Databricks CLI](https://docs.microsoft.com/en-us/azure/databricks/dev-tools/cli/)

    ```text
    dbfs mkdirs dbfs:/databricks/openlineage
    dbfs cp --overwrite ./openlineage-spark-*.jar               dbfs:/databricks/openlineage/
    dbfs cp --overwrite ./open-lineage-init-script.sh           dbfs:/databricks/openlineage/open-lineage-init-script.sh
    ```

    > **Note**
    > If you choose to use the Databricks Filestore UI instead of the CLI to upload the jar, the UI will replace hyphens (-) with underscores (_). This will cause the init script to fail as it expects hyphens in the jar's file name.
    > Either use the Databricks CLI to ensure the file name is consistent or update the init script to reflect the underscores in the jar name.

4. Create or modify an interactive or job cluster in your Databricks Workspace. Under Advanced Options, add this config to the [Spark Configuration](https://docs.microsoft.com/en-us/azure/databricks/clusters/configure#spark-configuration):

    ```text
    spark.openlineage.version v1 
    spark.openlineage.namespace <ADB-WORKSPACE-ID>#<DB_CLUSTER_ID>
    spark.openlineage.host https://<FUNCTION_APP_NAME>.azurewebsites.net
    spark.openlineage.url.param.code <FUNCTION_APP_DEFAULT_HOST_KEY>
    ```

    1. The ADB-WORKSPACE-ID value should be the first part of the URL when navigating to Azure Databricks, not the workspace name. For example, if the URL is: _https://adb-4630430682081461.1.azuredatabricks.net/_, the ADB-WORKSPACE-ID should be _adb-4630430682081461.1_.
    1. You should store the FUNCTION_APP_DEFAULT_HOST_KEY in a secure location. If you will be configuring individual clusters with the OpenLineage agent, you can use Azure Databricks secrets to store the key in Azure KeyVault and retrieve it as part of the cluster initialization script.  For more information on this, see the [Azure documentation](https://docs.microsoft.com/en-us/azure/databricks/security/secrets/secret-scopes#--databricks-backed-scopes)

    After configuring the secret storage, the API key for OpenLineage can be configured in the Spark config, as in the following example:
    `spark.openlineage.url.param.code {{secrets/secret_scope/Ol-Output-Api-Key}}`
    1. Add a reference to the uploaded init script `dbfs:/databricks/openlineage/open-lineage-init-script.sh` on the [Init script section](https://docs.microsoft.com/en-us/azure/databricks/clusters/init-scripts#configure-a-cluster-scoped-init-script-using-the-ui) of the Advanced Options.

5. At this point, you can run a Databricks notebook on an "all-purpose cluster" in your configured workspace and observe lineage in Microsoft Purview once the Databricks notebook has finished running all cells.

6. If you do not see any lineage please follow the steps in the [troubleshooting guide](./TROUBLESHOOTING.md).

7. To support Databricks lineage from Databricks jobs, see the following section below.

### <a id="jobs-lineage" />Support Extracting Lineage from Databricks Jobs

To support Databricks Jobs, you must add the service principal to your Databricks workspace. To use the below scripts, you must authenticate to Azure Databricks using either [access tokens](https://docs.microsoft.com/en-us/azure/databricks/dev-tools/api/latest/authentication) or [AAD tokens](https://docs.microsoft.com/en-us/azure/databricks/dev-tools/api/latest/aad/). The snippets below assume you have generated an access token.

1. [Add your Service Principal to Databricks as a User](https://docs.microsoft.com/en-us/azure/databricks/dev-tools/api/latest/scim/scim-sp#add-service-principal)
    * Create a file named `add-service-principal.json` that contains
      ```json
      {
        "schemas": [ "urn:ietf:params:scim:schemas:core:2.0:ServicePrincipal" ],
        "applicationId": "<azure-application-id>",
        "displayName": "<display-name>",
        "groups": [
            {
            "value": "<group-id>"
            }
        ],
        "entitlements": [
            {
            "value":"allow-cluster-create"
            }
        ]
      }
      ```
    * Provide a group id by executing the `groups` Databricks API and extracting a group id.
      ```bash
      curl -X GET \
      https://<databricks-instance>/api/2.0/preview/scim/v2/Groups \
      --header 'Authorization: Bearer DATABRICKS_ACCESS_TOKEN' \
      | jq .
      ```
      You may use the admin group id or create a separate group to isolate the service principal.

    * Execute the following bash command after the file above has been created and populated.
      ```bash
      curl -X POST \
      https://<databricks-instance>/api/2.0/preview/scim/v2/ServicePrincipals \
      --header 'Content-type: application/scim+json' \
      --header 'Authorization: Bearer DATABRICKS_ACCESS_TOKEN' \
      --data @add-service-principal.json \
      | jq .
      ```
2. [Assign the Service Principal as a contributor to the Databricks Workspace](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-assignments-portal?tabs=current)

3. At this point, you can run a Databricks job on a "job cluster" in your configured workspace and observe lineage in Microsoft Purview once the Databricks job has finished.

4. If you do not see any lineage please follow the steps in the [troubleshooting guide](./TROUBLESHOOTING.md).

### <a id="global-init"/>Global Init Scripts

You can also configure the OpenLineage listener to run globally, so that any cluster which is created automatically runs the listener.  To do this, you can utilize a [global init script](https://docs.microsoft.com/en-us/azure/databricks/clusters/init-scripts#global-init-scripts).

**Note**: Global initialization cannot currently use values from Azure Databricks KeyVault integration mentioned above. If using global initialization scripts, this key would need to be retrieved in the notebooks themselves, or hardcoded into the global init script.
