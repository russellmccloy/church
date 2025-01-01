@description('The name ofthe CosmosDb account.')
param cosmosDbName string

@description('Optional. The region where the resources live.')
@allowed([
  'australiaeast'
  'australiasoutheast'
  'australiacentral'
  'australiasoutheast'
])
param location string = 'australiaeast'

resource cosmosdb 'Microsoft.DocumentDB/databaseAccounts@2021-04-15' = {
  name: cosmosDbName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: 'Australia East'
        failoverPriority: 0
      }
    ]
    enableFreeTier: true
  }
}
