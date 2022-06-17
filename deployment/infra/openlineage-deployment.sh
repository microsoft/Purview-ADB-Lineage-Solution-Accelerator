#!/bin/bash
az config set extension.use_dynamic_install=yes_without_prompt
source ./settings.sh
# read parameters
rg=${rg:-}
prefix=${prefix:-}
clientid=${clientid:-}
clientsecret=${clientsecret:-}
purviewName=${purviewName:-}
adb=${adb:-}
adbtoken=${adbtoken:-}
purview=${purview:-}
tenantid=${tenantid}
purviewlocation=${purviewlocation:-}
githubpersonalaccesstoken=${githubpersonalaccesstoken:-}
resourceTagValues=${resourcetagtalues:-}



while [ $# -gt 0 ]; do

   if [[ $1 == *"--"* ]]; then
        param="${1/--/}"
        declare $param="$2"
        # echo $1 $2 // Optional to see the parameter:value result
   fi

  shift
done

info() {
    echo "$(date +"%Y-%m-%d %T") [INFO]"
}

# declare all variables
if [[ $rg == "" ]]; then
    echo "rgname is required in order to deploy the resources to assgined Resource Group"
    exit 1
else
    RG_NAME=$rg
    cleanedRGName=`echo $rg |  sed 's/_//g' | sed 's/-//g'`
fi

if [[ $clientid == "" ]]; then
    echo "clientid is missing"
    exit 1
fi

if [[ $clientsecret == "" ]]; then
    echo "clientsecret is missing"
    exit 1
fi

if [[ $prefix == "" ]]; then
    prefixName="oldemo"
else
    prefixName=$prefix
fi

if [[ $resourceTagValues == "" ]]; then
    resourceTagArm="{}"
    resourceTagNonArm=""
else
    #resourceTagArm=${resourceTagValues//=/:}
	resourceTagArm=$resourceTagValues
    resourceTagNonArm=`echo $resourceTagValues | sed "s/[\{\}\']//g" | sed "s/,/ /g" | sed "s/:/=/g" | sed "s/\"//g"`
fi

if [[ $purviewName == "" ]]; then
    purview_account_name="${cleanedRGName}-openlineage-purview"
    purview_managed_group_name="$purview_account_name-managed-rg"
else
    purview_account_name=$purviewName
    purview_managed_group_name="$purview_account_name-managed-rg"
fi

if [[ $adb == "" ]]; then
    ADB_WS_NAME="${cleanedRGName}-adb"
else
    ADB_WS_NAME=$adb
fi

if [[ $purviewlocation == "" ]]; then
    purviewlocation=$RGLOCATION
fi
# echo $RG_NAME
# echo $purview_account_name

# init check whether resource group exists, exit if not
if $(az group exists --resource-group $RG_NAME); then
    echo "$(info) start deployment in $RG_NAME"
else
    echo "$(info) resource group does not exist in this subscription or you do not have permission, please double check" 
    exit 1
fi

acc_detail=$(az account show)
subscription_id=$(jq -r '.id' <<< $acc_detail)
tenant_id=$(jq -r '.tenantId' <<< $acc_detail)

echo "$(info) start deploying all openlineage required resources"
echo "including: FunctionApp, EventHub, StorageAccount, etc."
ol_demo_resources_resp=$(az deployment group create --name OpenLineageDemoResourcesDeployment \
        --resource-group $RG_NAME \
        --template-file newdeploymenttemp.json \
        --parameters prefixName="$prefixName" \
        --parameters clientid="$clientid" \
        --parameters clientsecret="$clientsecret" \
        --parameters purviewName="$purview_account_name"\
        --parameters resourceTagValues="$resourceTagArm" )

ol_demo_resources_outputs=$(jq -r '.properties.outputs' <<< $ol_demo_resources_resp)

# echo $ol_resources_resp
FUNNAME=$(jq -r '.functionAppName.value' <<< $ol_demo_resources_outputs)
KVNAME=$(jq -r '.kvName.value' <<< $ol_demo_resources_outputs)
ADLSNAME=$(jq -r '.storageAccountName.value' <<< $ol_demo_resources_outputs)
RGLOCATION=$(jq -r '.resourcegroupLocation.value' <<< $ol_demo_resources_outputs)
echo "$(info) deploying all openlineage required resources FINISHED"

adls_keys=$(az storage account keys list -g $RG_NAME -n $ADLSNAME)
ADLSKEY=$(jq -r '.[1].value' <<< $adls_keys)

CLUSTERNAME="openlineage-demo"

### Download Jar File
curl -O -L https://repo1.maven.org/maven2/io/openlineage/openlineage-spark/0.8.2/openlineage-spark-0.8.2.jar
###
az storage container create -n rawdata --account-name $ADLSNAME --account-key $ADLSKEY
sampleA_resp=$(az storage blob upload --account-name $ADLSNAME --account-key $ADLSKEY -f exampleInputA.csv -c rawdata -n examples/data/csv/exampleInputA/exampleInputA.csv)
sampleB_resp=$(az storage blob upload --account-name $ADLSNAME --account-key $ADLSKEY -f exampleInputB.csv -c rawdata -n examples/data/csv/exampleInputB/exampleInputB.csv)

# managed_identity_id=$(jq -r '.properties.outputs.managedIdentityResourceId.value' <<< "${output_test}")
# echo  $managed_identity_id
# location=$(jq -r '.properties.outputs.location.value' <<< "${output_test}")
location=$RGLOCATION
echo "$(info) start deploying databricks workspace"
# adb_creation=$(az databricks workspace create --location $location --name $ADB_WS_NAME --resource-group $RG_NAME --sku standard --no-wait)
adb_details=$(az databricks workspace list --resource-group $RG_NAME)
# result=$(jq '.[] | select(.name == "$ADB_WS_NAME")' <<< "{$adb_details}")

# result=$(echo $adb_details | jq -r '.[] | select(.name == "$ADB_WS_NAME")')
adb_result=$(echo $(jq -r --arg ADB_WS_NAME "$ADB_WS_NAME" '
    .[] 
    | select(.name==$ADB_WS_NAME)' <<< $adb_details))

if [[ $adb_result == "" ]]; then
    echo "$(info) start deployinig new databricks workspace"
	if [[ $resourceTagNonArm == "" ]]; then 
	    adb_result=$(az databricks workspace create --location $RGLOCATION --name $ADB_WS_NAME --resource-group $RG_NAME --sku standard)
	else
		adb_result=$(az databricks workspace create --location $RGLOCATION --name $ADB_WS_NAME --resource-group $RG_NAME --tags $eval $resourceTagNonArm --sku standard)
	fi
	
    sleep 60
else
    echo "$(info) databricks workspace named $ADB_WS_NAME already exists"
fi
echo "$(info) databricks workspace $ADB_WS_NAME has been created (or already exists), continue..."

adb_detail=$(az databricks workspace show --resource-group $RG_NAME --name $ADB_WS_NAME)
adb_ws_url=$(echo $(jq -r '.workspaceUrl' <<< $adb_detail))
adb_ws_id=$(echo $(jq -r '.id' <<< $adb_detail))
echo $adb_ws_id
echo $adb_ws_url

if [[ $adbtoken == "" ]]; then
    echo "trying to get retrieve databricks token if you have permission"
    sleep 10
    global_adb_token=$(az account get-access-token --resource 2ff814a6-3304-4ab8-85cb-cd0e6f879c1d -o tsv --query '[accessToken]')
    az_token=$(az account get-access-token --resource https://management.core.windows.net/ -o tsv --query '[accessToken]')
else
    global_adb_token=$adbtoken
    az_token=""
fi

# creating json for cluster configuration
cat << EOF > create-cluster.json
{
    "cluster_name": "$CLUSTERNAME",
    "spark_version": "9.1.x-scala2.12",
    "node_type_id": "Standard_DS3_v2",
    "num_workers": 1,
    "spark_conf": {
        "spark.openlineage.version" : 1,
        "spark.openlineage.namespace" : "adbpurviewol1#default",
        "spark.openlineage.host" : "https://$FUNNAME.azurewebsites.net",
        "spark.openlineage.url.param.code": "{{secrets/purview-to-adb-kv/Ol-Output-Api-Key}}"
    },
    "spark_env_vars": {
        "PYSPARK_PYTHON" : "/databricks/python3/bin/python3"
    },
    "autotermination_minutes": 15,
    "enable_elastic_disk": true,
    "cluster_source": "UI",
    "init_scripts": [
        {
            "dbfs":{
                "destination": "dbfs:/databricks/openlineage/open-lineage-init-script.sh"
            }
        }
    ],
    "libraries": [
        {
            "maven": {
                "coordinates": "com.microsoft.azure:spark-mssql-connector_2.12:1.2.0"
            }
        }
    ]
}
EOF

STAGE_DIR="/dbfs/databricks/openlineage"
cat << OUTEREND > ol_init_script.sh
#!/bin/bash

STAGE_DIR="/dbfs/databricks/openlineage"

echo "BEGIN: Upload Spark Listener JARs"
cp -f $STAGE_DIR/openlineage-spark-*.jar /mnt/driver-daemon/jars || { echo "Error copying Spark Listener library file"; exit 1;}
echo "END: Upload Spark Listener JARs"

echo "BEGIN: Modify Spark config settings"
cat << 'EOF' > /databricks/driver/conf/openlineage-spark-driver-defaults.conf
[driver] {
  "spark.extraListeners" = "io.openlineage.spark.agent.OpenLineageSparkListener"
  "spark.openlineage.url.param.codetest" = "testing"
  "spark.openlineage.samplestorageaccount" = "$ADLSNAME"
  "spark.openlineage.samplestoragecontainer" = "rawdata"
}
EOF
echo "END: Modify Spark config settings"
OUTEREND

curl -s -X POST https://$adb_ws_url/api/2.0/dbfs/mkdirs \
    --header 'Accept: application/json' \
    -H "Authorization: Bearer $global_adb_token" \
    --data '{ "path": "/databricks/openlineage" }'

init_script_base64=$(base64 -w 0 ol_init_script.sh)

curl -s -X POST https://$adb_ws_url/api/2.0/dbfs/put \
    -H "Authorization: Bearer $global_adb_token" \
    --data "{ \"path\": \"/databricks/openlineage/open-lineage-init-script.sh\", 
    \"contents\": \"$init_script_base64\", 
    \"overwrite\": true }"

PATTERN="./openlineage-jarxx*"
if ls $PATTERN 1> /dev/null 2>&1; then
    rm $PATTERN
fi

files=($(ls openlineage*.jar | sort -r))
latest_v_jar=$(echo ${files[0]})
echo $latest_v_jar
jarbase64=$(base64 -w 0 $latest_v_jar)
cat << EOF > openlineage-jar-encoded.txt
$jarbase64
EOF
split -b 20k openlineage-jar-encoded.txt openlineage-jarxx

cat << OUTEREND > adb_create_jar.json
{ "path": "/databricks/openlineage/$latest_v_jar", "overwrite": true }
OUTEREND

stream_handle_response=$(curl -s -X POST https://$adb_ws_url/api/2.0/dbfs/create \
    -H "Authorization: Bearer $global_adb_token" \
    -H "X-Databricks-Azure-Workspace-Resource-Id": $adb_ws_id \
    -d @adb_create_jar.json)
stream_handle=$(echo $(jq -r '.handle' <<< $stream_handle_response))
echo $stream_handle

echo "$(info) start uploading jar file"
for file in $(ls -d openlineage-jarxx*)
do
    jar_data=$(<$file)
    curl -s -X POST https://$adb_ws_url/api/2.0/dbfs/add-block -H "Authorization: Bearer $global_adb_token" -d "{\"data\" : \"$jar_data\", \"handle\":$stream_handle}"
done

sleep 3
echo "$(info) jar file uploaded"

jar_upload_close_stream_response=$(curl -s -X POST https://$adb_ws_url/api/2.0/dbfs/close -H "Authorization: Bearer $global_adb_token" -d "{\"handle\":$stream_handle}")
## close stream connection

sleep 3
rm ./openlineage-jarxx*

notebook_base64=$(base64 -w 0 abfss-in-abfss-out-olsample.scala)
curl -X POST -H 'Accept: application/json' \
    -H "Authorization: Bearer $global_adb_token" \
    -H "X-Databricks-Azure-SP-Management-Token: $az_token" \
    -H "X-Databricks-Azure-Workspace-Resource-Id: $adb_ws_id" \
    -d "{ \"path\": \"/Shared/abfss-in-abfss-out-olsample\", \"language\": \"SCALA\", \"overwrite\": true,
    \"content\": \"$notebook_base64\" }" \
    https://$adb_ws_url/api/2.0/workspace/import

echo "notebooks uploaded"

cat << EOF > create-scope.json
{
  "scope": "purview-to-adb-kv",
  "scope_backend_type": "AZURE_KEYVAULT",
  "backend_azure_keyvault":
  {
    "resource_id": "/subscriptions/$subscription_id/resourceGroups/$RG_NAME/providers/Microsoft.KeyVault/vaults/$KVNAME",
    "dns_name": "https://$KVNAME.vault.azure.net/"
  },
  "initial_manage_principal": "users"
}
EOF
## End of databricks workspace deployment

echo "$(info) start deploying purview account"
purview_details=$(az purview account list --resource-group $RG_NAME)
purview_result=$(echo $(jq -r --arg purview_acc_name "$purview_account_name" '
    .[] 
    | select(.name==$purview_acc_name)' <<< $purview_details))

if [[ $purview_result == "" ]]; then
	if [[ $resourceTagNonArm == "" ]]; then 	
    	purview_creation_result=$(az purview account create --location $purviewlocation --name $purview_account_name --resource-group $RG_NAME --managed-group-name $purview_managed_group_name)
	else
    	purview_creation_result=$(az purview account create --location $purviewlocation --name $purview_account_name --resource-group $RG_NAME --managed-group-name $purview_managed_group_name --tags $eval $resourceTagNonArm)
	fi
    sleep 210
else
    echo "$(info) purview account [$purview_account_name] already exists"
fi
echo "$(info) purview account [$purview_account_name] has been created (or already exists), continue..."

# purview_detail=$(az purview account show --resource-group $RG_NAME --name $purview_account_name)
purview_endpoint="https://$purview_account_name.purview.azure.com"

## below is all deployment require AAD
user_detail=$(az ad signed-in-user show)
user_object_id=$(echo $(jq -r '.id' <<< $user_detail))
echo "objectId"
echo "$user_object_id"

kv_add_adb_ap=$(az keyvault set-policy --name $KVNAME --secret-permissions get list --object-id e84e8963-b732-473a-a15f-45b1206c3884)
kv_add_user_ap=$(az keyvault set-policy --name $KVNAME --secret-permissions get list --object-id $user_object_id)

if [[ $az_token == "" ]]; then
    adb_scope_creation_resp=$(echo $(curl -s \
        -X POST https://$adb_ws_url/api/2.0/secrets/scopes/create \
        -H "Authorization: Bearer $global_adb_token" \
        -H "X-Databricks-Azure-Workspace-Resource-Id: $adb_ws_id" \
        --data @create-scope.json))
    adb_scope_creation_error_code=$(echo $(jq -r '.error_code' <<< $adb_scope_creation_resp))
    # echo $adb_scope_creation_error_code

    adb_scope_creation_resp_checker='html'
    if grep -q "$adb_scope_creation_resp_checker" <<< "$adb_scope_creation_resp"; then
        echo $adb_scope_creation_resp
        echo ""
        echo "$(info) something is wrong with you permission; please check with your admin"
    else
        echo "$(info) adb azure-based scope is created; or something is wrong, please check the message"
        echo $adb_scope_creation_resp
    fi
else
    adb_scope_creation_resp=$(echo $(curl -s \
        -X POST https://$adb_ws_url/api/2.0/secrets/scopes/create \
        -H "Authorization: Bearer $global_adb_token" \
        -H "X-Databricks-Azure-SP-Management-Token: $az_token" \
        -H "X-Databricks-Azure-Workspace-Resource-Id: $adb_ws_id" \
        --data @create-scope.json))
    echo $adb_scope_creation_resp
fi

if [[ $az_token == "" ]]; then
    curl -X POST https://$adb_ws_url/api/2.0/clusters/create \
        -H "Authorization: Bearer $global_adb_token" \
        -H 'X-Databricks-Azure-Workspace-Resource-Id': $adb_ws_id \
        -d @create-cluster.json
else
    curl -X POST https://$adb_ws_url/api/2.0/clusters/create \
        -H "Authorization: Bearer $global_adb_token" \
        -H "X-Databricks-Azure-SP-Management-Token: $az_token" \
        -H "X-Databricks-Azure-Workspace-Resource-Id: $adb_ws_id" \
        -d @create-cluster.json
fi
echo ""
echo "$(info) cluster created"

az purview account add-root-collection-admin --account-name $purview_account_name --resource-group $RG_NAME --object-id $user_object_id

spID=$(az resource list -n $purview_account_name --query [*].identity.principalId --out tsv)
storageId=$(az storage account show -n $ADLSNAME -g $RG_NAME --query id --out tsv)
az role assignment create --assignee $spID --role 'Storage Blob Data Reader' --scope $storageId


read -p "Please add your Client Identity (created as a prerequisite to installation) to the Data Curator Role in Purview. Hit Enter to continue"

# purview_detail=$(az purview account show --resource-group $RG_NAME --name $purview_account_name)
purview_endpoint="https://$purview_account_name.purview.azure.com"

acc_purview_token=$(curl https://login.microsoftonline.com/$tenantid/oauth2/token --data "resource=https://purview.azure.net&client_id=$clientid&client_secret=$clientsecret&grant_type=client_credentials" -H Metadata:true -s | jq -r '.access_token')

purview_type_resp_spark_type=""
subCheck="ENTITY"
subCheckUpload="enumDefs"
subStr="already exists"
for i in {1..6}
do
    if [ "$acc_purview_token" == null ]; then
        echo "There was a problem getting the Purview Token. Please check your Client ID and Client Secret"
        break
    elif [[ "${purview_type_resp_spark_type}" == "" && $i -eq 1 ]] || [[ $retry_check == "retry" && $i -le 5 ]]; then
        echo "Uploading Solution Accelerator Custom Types"
        purview_type_resp_spark_type=$(curl -s -X POST $purview_endpoint/catalog/api/atlas/v2/types/typedefs \
            -H "Authorization: Bearer $acc_purview_token" \
            -H "Content-Type: application/json" \
            -d @Custom_Types.json )
        sub_Check=$purview_type_resp_spark_type
        
        if [[ "$sub_Check" == *"$subStr"* ]]; then
            echo "Custom Types already exist."
        fi


        upload_good="yes"

        if [[ "$purview_type_resp_spark_type" == "" ]]; then
            upload_good="no"
        fi
	    retry_check=""
    elif [[ "${purview_type_resp_spark_type}" == "" && $i -le 5 ]]; then
        echo "Retying Type Upload" 
        sleep 15
        retry_check="retry"
    elif [[ "$upload_good" == "yes" ]]; then
        echo "$(info) Purview Spark types deployment finished"
        echo "$(info) end of deployment"
        break 
    elif [[ "${purview_type_resp_spark_type}" == "" && $i -gt 5 ]]; then
        echo "There was a problem uploading Purview Types."
        break
    else
        echo "There was a Problem uploading Purview types. Please check your your settings.sh file and verify your inputs." 
        break
    fi
done
