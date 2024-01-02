#!/bin/bash
# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.
# Script to start and stop a given ADX Cluster
# manage-adx-cluster.sh [start|stop] subscriptionId resourceGroupName clusterName
# If you use stop and the pool is resuming, exit 201
# If you use start and the pool is pausing, exit 200

manage_option=$1
subscriptionId=$2
resourceGroupName=$3
clusterName=$4

TOKEN_RESP=$(curl -X POST -H 'Content-Type: application/x-www-form-urlencoded' \
-d "client_id=$AZURE_CLIENT_ID" \
-d "client_secret=$AZURE_CLIENT_SECRET" \
-d 'grant_type=client_credentials' \
-d 'resource=https://management.azure.com' \
"https://login.microsoftonline.com/$AZURE_TENANT_ID/oauth2/token")

ACCESS_TOKEN=$(echo $TOKEN_RESP | jq -r '.access_token')

CURRENT_STATUS=$(curl -H "Authorization: Bearer $ACCESS_TOKEN" \
"https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Kusto/clusters/$clusterName?api-version=2023-08-15" | jq -r '.properties.state')

echo "ADX Cluster is $CURRENT_STATUS"


if [[ $manage_option == "start" ]]
then
    if [[ $CURRENT_STATUS == "Stopped" ]]
    then
        RESUME_COMMAND=$(curl -X POST -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Length: 0" \
    "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Kusto/clusters/$clusterName/start?api-version=2023-08-15")
        echo $($RESUME_COMMAND)
        echo "Executed Resume command"
    elif [[ $CURRENT_STATUS == "Running" ]]
    then
        echo "The ADX Cluster is already active"
    elif [[ $CURRENT_STATUS == "Starting" ]]
    then
        echo "The ADX Cluster is resuming"
    else
        echo "The ADX Cluster is in a state that it can't be activated from"
        exit 200
    fi
elif [[ $manage_option == "stop" ]]
then
    if [[ $CURRENT_STATUS == "Running" ]]
    then
        RESUME_COMMAND=$(curl -X POST -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Length: 0" \
    https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Kusto/clusters/$clusterName/stop?api-version=2023-08-15)
        echo $($RESUME_COMMAND)
        echo "Executed Stop command"
    elif [[ $CURRENT_STATUS == "Stopped" ]]
    then
        echo "The ADX Cluster is already paused"
    elif [[ $CURRENT_STATUS == "Stopping" ]]
    then
        echo "The ADX Cluster is pausing"
    else
        echo "The ADX Clsuter is in a state that it can't be paused during"
        exit 201
    fi
fi
