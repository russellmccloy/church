using '../main.bicep'

param location = 'australiaeast'
param storageAccountName = 'churchdevst01'
param webAppName = 'church-dev-webapp-01'
param appServicePlanName = 'church-dev-asp-01'
param keyVaultName = 'church-dev-kv-01'
param appInsightsName = 'church-dev-ain-01'

param azureAdInstance = 'https://login.microsoftonline.com/'
param azureAdDomain = 'russellmccloygooglemail.onmicrosoft.com'
param azureAdTenantId = '574dbe58-968a-4a3a-b963-a15dfe350359'
param azureAdClientId = '49f8d511-c0fd-491a-845b-d947fbf286a4'
param azureAdCallbackPath = '/signin-oidc'

param googleSheetSpreadsheetId = '162vKat_JS1TIpkZVXO6OCdsqGUvhfltXNCFCeqcw_GM'
param googleSheetApplicationName = 'PrayerCards'

param storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=churchdevst01;AccountKey=N5OxEMHX9zSUe+O2oI8PqGfDyNaGenuKETjg4PdfDJ2h6Ty823o56NE6D9g1e0bcyNRXggLwO2UP+AStVB7acg==;EndpointSuffix=core.windows.net'