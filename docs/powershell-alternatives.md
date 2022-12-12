# Powershell Alternative Scripts

In some cases, you're not able to use the cloud shell or you don't have access to a machine that can run wsl / curl.  This doc provides alternatives to select 

## Upload Custom Types

Assumes you are in the `deployment/infra` folder of the repo.

```powershell
$purview_endpoint="https://PURVIEW_ACCOUNT_NAME.purview.azure.com"
$TENANT_ID="TENANT_ID" 
$CLIENT_ID="CLIENT_ID" 
$CLIENT_SECRET="CLIENT_SECRET"

$get_token=(Invoke-RestMethod -Method 'Post' -Uri "https://login.microsoftonline.com/$TENANT_ID/oauth2/token"  -Body "resource=https://purview.azure.net&client_id=$CLIENT_ID&client_secret=$CLIENT_SECRET&grant_type=client_credentials")
$token=$get_token.access_token
$body=(Get-Content -Path .\Custom_Types.json)
$headers = @{
'Content-Type'='application/json'
'Authorization'= "Bearer $token"
}

Invoke-RestMethod -Method 'Post' -Uri "$purview_endpoint/catalog/api/atlas/v2/types/typedefs"  -Body $body -Headers $headers

```
