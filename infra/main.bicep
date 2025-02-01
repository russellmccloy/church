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
    {
          name: 'AzureAd__Instance'
          value: 'https://login.microsoftonline.com/'
    }
    {
          name: 'AzureAd__Domain'
          value: 'russellmccloygooglemail.onmicrosoft.com'
    }
    {
          name: 'AzureAd__TenantId'
          value: '574dbe58-968a-4a3a-b963-a15dfe350359'
    }
    {
          name: 'AzureAd__ClientId'
          value: '49f8d511-c0fd-491a-845b-d947fbf286a4'
    }
    {
          name: 'AzureAd__CallbackPath'
          value: '/signin-oidc'
    }
    {
          name: 'AzureAd__ClientSecret'
          value: 'uf.8Q~ka25zS0PceMXo5Sa-nCc0JkveacVg15c-a'
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

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
//     cors: contains(cors, 'blob') && !empty(cors.blob) ? cors.blob : null
  }
}

resource imagesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: 'images'
  parent: blobServices
  properties: {
    publicAccess: 'None'  // Change this if you need public access (e.g., 'Blob' or 'Container')
  }
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
      linuxFxVersion: 'DOTNETCORE|8.0'
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
