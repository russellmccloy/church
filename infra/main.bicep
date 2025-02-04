// comment 25d
param location string = resourceGroup().location
param storageAccountName string
param webAppName string
param appServicePlanName string
param keyVaultName string 
param appInsightsName string

param azureAdInstance string
param azureAdDomain string
param azureAdTenantId string
param azureAdClientId string
param azureAdCallbackPath string

param googleSheetSpreadsheetId string
param googleSheetApplicationName string

param storageConnectionString string

@description('Specifies whether Azure Virtual Machines are permitted to retrieve certificates stored as secrets from the key vault.')
param enabledForDeployment bool = false

@description('Specifies whether Azure Disk Encryption is permitted to retrieve secrets from the vault and unwrap keys.')
param enabledForDiskEncryption bool = false

@description('Specifies whether Azure Resource Manager is permitted to retrieve secrets from the key vault.')
param enabledForTemplateDeployment bool = false

@description('Specifies the Azure Active Directory tenant ID that should be used for authenticating requests to the key vault. Get it by using Get-AzSubscription cmdlet.')
param tenantId string = subscription().tenantId

@description('Specifies the object ID of a user, service principal or security group in the Azure Active Directory tenant for the vault. The object ID must be unique for the list of access policies. Get it by using Get-AzADUser or Get-AzADServicePrincipal cmdlets.')
param objectId string = '4c368958-f197-455b-a6f0-4cb56d4a97a9'

@description('Specifies the permissions to secrets in the vault. Valid values are: all, get, list, set, delete, backup, restore, recover, and purge.')
param secretsPermissions array = [
  'get', 'list', 'Set'
]

@description('Specifies whether the key vault is a standard vault or a premium vault.')
@allowed([
  'standard'
  'premium'
])
param skuName string = 'standard'

var requiredAppSettings = [
    {
        name: 'ChurchStorage__AccountName'
        value: storageAccountName
    }
    {
        name: 'ChurchStorage__ConnectionString'
        value: storageConnectionString 
    }
   {
          name: 'ChurchStorage__ContainerName'
          value: 'images'  
    }
    {
          name: 'AzureAd__Instance'
          value: azureAdInstance
    }
    {
          name: 'AzureAd__Domain'
          value: azureAdDomain
    }
    {
          name: 'AzureAd__TenantId'
          value: azureAdTenantId
    }
    {
          name: 'AzureAd__ClientId'
          value: azureAdClientId
    }
    {
          name: 'AzureAd__CallbackPath'
          value: azureAdCallbackPath
    }
    {
          name: 'AzureAd__ClientSecret'
          value: '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}.vault.azure.net/secrets/azureAdClientSecret)' 
    }
    {
          name: 'GoogleSheet__SpreadsheetId'
          value: googleSheetSpreadsheetId
    }
    {
          name: 'GoogleSheet__ApplicationName'
          value: googleSheetApplicationName
    }
    { 
      name: 'GoogleSheet__CredentialsJson'
      value: '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}.vault.azure.net/secrets/googleSheetCredentialsJson)' 
    }
    {
        name: 'ApplicationInsights__ConnectionString'
        value: appInsightsInstance.properties.ConnectionString
    }
    {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: 'Development'
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

resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    enabledForDeployment: enabledForDeployment
    enabledForDiskEncryption: enabledForDiskEncryption
    enabledForTemplateDeployment: enabledForTemplateDeployment
    tenantId: tenantId
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    accessPolicies: [
      {
        objectId: objectId
        tenantId: tenantId
        permissions: {
          secrets: secretsPermissions
        }
      }
    {
      tenantId: tenantId
      objectId: webApp.identity.principalId // Grants web app access to KV
      permissions: {
        secrets: secretsPermissions
      }
    }
    ]
    sku: {
      name: skuName
      family: 'A'
    }
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

resource appInsightsInstance 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
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
