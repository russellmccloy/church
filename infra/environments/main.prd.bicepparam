using '../main.bicep'

param location = 'australiaeast'
param storageAccountName = 'churchprdst01'
param webAppName = 'church-prd-webapp-01'
param appServicePlanName = 'church-prd-asp-01'
param keyVaultName = 'church-prd-kv-01'
param appInsightsName = 'church-prd-ain-01'

param azureAdInstance = 'https://login.microsoftonline.com/'
param azureAdDomain = 'russellmccloygooglemail.onmicrosoft.com'
param azureAdTenantId = '574dbe58-968a-4a3a-b963-a15dfe350359'
param azureAdClientId = '9b4087b6-4225-44d5-b015-c60e08c6b144'
param azureAdCallbackPath = '/signin-oidc'

param googleSheetSpreadsheetId = '1dxKkT5T9WZVUPhFjBahceJo8TXnce-1anxOK_aEKl9g'
param googleSheetApplicationName = 'PrayerCards'

param storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=churchprdst01;AccountKey=6Ny8cCyJCHtKNvOH48A5whvOCPPNeUkST/I2yMAjfFs2rcxSU3cOH7j4sHaR92iypBI4aYCbbS+J+AStJW0KvA==;EndpointSuffix=core.windows.net'

param ASPNETCORE_ENVIRONMENT = 'Production'