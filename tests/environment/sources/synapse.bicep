@description('The Synapse Workspace name.')
param workspaceName string = uniqueString('synwksp', resourceGroup().id)

@description('Location for all resources.')
param location string = resourceGroup().location

@description('The administrator username of the SQL logical server.')
@secure()
param administratorLogin string

@description('The administrator password of the SQL logical server.')
@secure()
param administratorLoginPassword string

var supportingStorageName = '${workspaceName}sa'

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: supportingStorageName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties:{
    isHnsEnabled: true
  }
  
}

resource rawdataContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-08-01' = {
  name: '${storageAccount.name}/default/defaultcontainer'
}

resource tempContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-08-01' = {
  name: '${storageAccount.name}/default/temp'
}

resource synapseWorkspace 'Microsoft.Synapse/workspaces@2021-06-01' = {
  name: workspaceName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    azureADOnlyAuthentication: false
    defaultDataLakeStorage: {
      accountUrl: 'https://${storageAccount.name}.dfs.core.windows.net'
      createManagedPrivateEndpoint: false
      filesystem: 'synapsefs'
      resourceId: resourceId('Microsoft.Storage/storageAccounts/', storageAccount.name)
    }
    managedResourceGroupName: '${workspaceName}rg'
    
    publicNetworkAccess: 'Enabled'
    sqlAdministratorLogin: administratorLogin
    sqlAdministratorLoginPassword: administratorLoginPassword
    trustedServiceBypassEnabled: true
  }
}

resource symbolicname 'Microsoft.Synapse/workspaces/sqlPools@2021-06-01' = {
  name: 'sqlpool1'
  location: location
  sku: {
    name: 'DW100c'
    capacity: 0
  }
  parent: synapseWorkspace
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    createMode: 'Default'

    storageAccountType: 'LRS'
  }
}
