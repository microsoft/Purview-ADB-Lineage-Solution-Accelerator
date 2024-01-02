# Add Service Principal to Databricks Workspace for Job and Data Factory Support

When extracting lineage for Databricks Jobs (a.k.a. Workflows), you will need the Solution Accelerator to be able to read Databricks Job information. This also applies to Data Factory calling Databricks Notebooks or Python Files as Activities in a Pipeline. In order to read this information, the Solution Accelerator Service Principal (the one that has been granted the Data Curator role in Microsoft Purview) must be added to the Databricks Workspace as a user.

Given that this solution does not support Unity Catalog, it is assumed that you are using the Workspace Admin settings and not the Account Console to add and manage Service Principals.

For best experience, follow the [official documentation on how to add a service principal to Databricks Workspace and users group](https://learn.microsoft.com/en-us/azure/databricks/administration-guide/users-groups/service-principals#--add-a-service-principal-to-a-workspace-using-the-workspace-admin-settings) via the UI.

The remainder of this page provides sample code for adding a service principal via REST API calls.

## Generate a Databricks Access Token

This sample uses [Databricks Personal Access Tokens](https://learn.microsoft.com/en-us/azure/databricks/dev-tools/auth/pat). Generate a token for use in the code below.

## Add your Service Principal to Databricks as a User and Add to Users Group

[Databricks Workspace REST API: Service Principals - Create](https://docs.databricks.com/api/azure/workspace/serviceprincipals/create)

* Find the `users`` group id by executing the `groups` Databricks API and extracting the group id.
    ```bash
    curl -X GET \
    https://<databricks-instance>/api/2.0/preview/scim/v2/Groups \
    --header 'Authorization: Bearer <DATABRICKS_ACCESS_TOKEN>' \
    | jq .
    ```
    You may use the users group id or create a separate group to isolate the service principal. 
* Create a file named `add-service-principal.json` that contains the below payload with the users group id.
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
* Execute the following bash command after the file above has been created and populated.
    ```bash
    curl -X POST \
    https://<databricks-instance>/api/2.0/preview/scim/v2/ServicePrincipals \
    --header 'Content-type: application/scim+json' \
    --header 'Authorization: Bearer <DATABRICKS_ACCESS_TOKEN>' \
    --data @add-service-principal.json \
    | jq .
    ```

## Optional Use the Admin Group

In some cases, you may need to use the Admin group. Repeat the steps above and add the service principal to the Admin group.

## Optional: Assign the Service Principal as a contributor to the Databricks Workspace

The above steps should be sufficient but in some cases, you may need to add the service principal as a contributor to the Databricks Workspace resource.

[How to assign roles to a resource](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-assignments-portal?tabs=current)
