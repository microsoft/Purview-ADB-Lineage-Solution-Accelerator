@description('Cluster Name for Azure Data Explorer')
param clusterName string = uniqueString('adx', resourceGroup().id)

@description('Database Name for Azure Data Explorer Cluster')
param databaseName string = uniqueString('adxdatabase', resourceGroup().id)

@description('Location for all resources.')
param location string = resourceGroup().location

resource symbolicname 'Microsoft.Kusto/clusters@2022-11-11' = {
  name: clusterName
  location: location
  sku: {
    capacity: 1
    name: 'Dev(No SLA)_Standard_D11_v2'
    tier: 'Basic'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    enableAutoStop: true
    engineType: 'V3'
    publicIPType: 'IPv4'
    publicNetworkAccess: 'Enabled'
  }
  resource symbolicname 'databases@2022-11-11' = {
    name: databaseName
    location: location
    kind: 'ReadWrite'
    // For remaining properties, see clusters/databases objects
  }
}
