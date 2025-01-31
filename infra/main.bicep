param location string = resourceGroup().location
param storageAccountName string
param webAppName string
param appServicePlanName string

var requiredAppSettings = [
   {
     name: 'ChurchStorage__AccountName'
     value: storageAccountName
   }
  {
    name: 'ChurchStorage__ContainerName'
    value: 'images'
  }
]

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
   properties: {
    reserved: true
   }
   sku: {
     name: 'F1'
   }
   kind: 'linux'
 }

resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: webAppName
  location: location
  identity: {
      type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: requiredAppSettings
    }
  }
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(webApp.id, 'StorageBlobDataContributor')
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // Storage Blob Data Contributor role ID
    principalId: webApp.identity.principalId
  }
}
