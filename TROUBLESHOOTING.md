# Troubleshooting

### Connector Issues
* [How to file a new issue](#new-issue)
* [How to debug log files](#debug-logs)

### Common Issues
* [I don't see lineage in Microsoft Purview](#no-lineage)

### Demo Deployment Issues
* [$'\r': command not found](#demo-command-not-found)
* [OpenLineage jar did not upload completely to Cloud Shell](#demo-openlineage-jar)
* [No policy exists for Purview Collection during deployment](#demo-no-policy)
* [Databricks Notebooks do not exist](#demo-no-notebooks)

### Other Issues
* [VS Code Pop-Up: "Some projects have trouble loading" when opening the folder](#vscode-popup)

-----

## <a id="new-issue" />How to file a new issue
**When filing a new issue, [please include associated log message(s) from Azure Functions](#debug-logs).** This will allow the core team to debug within our test environment to validate the issue and develop a solution. Before submitting a [new issue](https://github.com/microsoft/Purview-ADB-Lineage-Solution-Accelerator/issues), please review the known issues below, as well as the [limitations](./LIMITATIONS.md) which affect what sort of lineage can be collected.


## <a id="debug-logs" />How to debug log files
1. Open Azure Portal > Resource Group > Function App > Functions
1. Select either the `OpenLineageIn` or `PurviewOut` function
1. Click `Monitor` in the left-hand menu
    1. Click the linked timestamps within `Inovacation Traces` to view details of past events
    1. Click the `Logs` tab to view the live events. (*Ensure both connected and timestamped welcome messages appear.*)

## <a id="no-lineage" />I don't see lineage in Microsoft Purview

* ### Try Refreshing the Page

    Microsoft Purview tends to cache the results to improve query response time. You may not see the results in the Purview UI immediately.

    **Solution**: Refresh the page using the Refresh button in the Purview UI. You may consider returning to the page after a minute or two and attempting the refresh again.

* ### Check Azure Key Vault References Are Active

    The connector uses [Key Vault References](https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references) inside the Azure Functions used to translate OpenLineage to Apache Atlas standards. When first launching the services, the Key Vault references may not have activated / synced.  You will see red "x" marks in the [Function App's Configuration menu](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings?tabs=portal).

    **Solution**: Wait two to five minutes after deploying the connector and check back to confirm that the Function can communicate with the Key Vault. If the connection still shows red "x" marks, confirm that the [Key Vault has an access policy](https://docs.microsoft.com/en-us/azure/key-vault/general/assign-access-policy?tabs=azure-portal) for your Azure Function's managed identity.

* ### Confirm that Custom Types Are Available

    If you have deployed the demo or connector solution, the custom types used by the connector may not have been uploaded.

    **Solution**: Follow the [post-installation step](./deploy-base.md#post-installation) and confirm that the types are created. If the types have not been created already, you will receive a JSON payload of the created types (e.g. `{"enumDefs":[], "classificationDefs":[], "entityDefs":[...], ... }`).  If the types already exist, you will receive a message indicating they exist already.

* ### Confirm that you are NOT using Spark Streaming

    Spark Structured Streaming and Spark DStreams are [not supported in this release](./LIMITATIONS.md#spark-streaming) of the solution accelerator.

## <a id="demo-command-not-found" />Demo Deployment: $'\r': command not found

When running the `openlineage-deployment.sh` script, you received either ``$'\r': command not found` or `syntax error near unexpected token $'\r'`. This can occur when you have used git to clone the shell script (.sh) to a Windows OS file system with [git core.autocrlf enabled](https://docs.github.com/en/get-started/getting-started-with-git/configuring-git-to-handle-line-endings). This will only occur if you cloned the repo locally rather than using the Bash cloud shell.

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
