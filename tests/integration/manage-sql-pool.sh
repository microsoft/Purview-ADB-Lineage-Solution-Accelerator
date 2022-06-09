#!/bin/bash
# Script to start and stop a given SQL Pool
# manage-sql-pool.sh [start|stop] subscriptionId resourceGroupName workspaceName sqlPoolName
# If you use stop and the pool is resuming, exit 201
# If you use start and the pool is pausing, exit 200

manage_option=$1
subscriptionId=$2
resourceGroupName=$3
workspaceName=$4
sqlPoolName=$5

TOKEN_RESP=$(curl -X POST -H 'Content-Type: application/x-www-form-urlencoded' \
-d "client_id=$AZURE_CLIENT_ID" \
-d "client_secret=$AZURE_CLIENT_SECRET" \
-d 'grant_type=client_credentials' \
-d 'resource=https://management.azure.com' \
"https://login.microsoftonline.com/$AZURE_TENANT_ID/oauth2/token")

ACCESS_TOKEN=$(echo $TOKEN_RESP | jq -r '.access_token')

CURRENT_STATUS=$(curl -H "Authorization: Bearer $ACCESS_TOKEN" \
https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Synapse/workspaces/$workspaceName/sqlPools/$sqlPoolName?api-version=2021-06-01 | jq -r '.properties.status')

echo "Synapse pool is $CURRENT_STATUS"


if [[ $manage_option == "start" ]]
then
    if [[ $CURRENT_STATUS == "Paused" ]]
    then
        RESUME_COMMAND=$(curl -X POST -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Length: 0" \
    "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Synapse/workspaces/$workspaceName/sqlPools/$sqlPoolName/resume?api-version=2021-06-01")
        echo $($RESUME_COMMAND)
        echo "Executed Resume command"
    elif [[ $CURRENT_STATUS == "Online" ]]
    then
        echo "The Sql Pool is already active"
    elif [[ $CURRENT_STATUS == "Resuming" ]]
    then
        echo "The Sql Pool is resuming"
    else
        echo "The Sql Pool is in a state that it can't be activated from"
        exit 200
    fi
elif [[ $manage_option == "stop" ]]
then
    if [[ $CURRENT_STATUS == "Online" ]]
    then
        RESUME_COMMAND=$(curl -X POST -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Length: 0" \
    https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Synapse/workspaces/$workspaceName/sqlPools/$sqlPoolName/pause?api-version=2021-06-01)
        echo $($RESUME_COMMAND)
        echo "Executed Pause command"
    elif [[ $CURRENT_STATUS == "Paused" ]]
    then
        echo "The Sql Pool is already paused"
    elif [[ $CURRENT_STATUS == "Pausing" ]]
    then
        echo "The Sql Pool is pausing"
    else
        echo "The Sql Pool is in a state that it can't be paused during"
        exit 201
    fi
fi
