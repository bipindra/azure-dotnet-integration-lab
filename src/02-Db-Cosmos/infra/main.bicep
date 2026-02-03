@description('The name of the project')
param projectName string = '02-Db-Cosmos'

@description('The environment name (dev, staging, prod)')
param environment string = 'dev'

@description('The Azure region to deploy resources')
param location string = resourceGroup().location

@description('Random suffix for unique resource names')
param nameSuffix string = uniqueString(resourceGroup().id)

// Cosmos DB Account
var cosmosAccountName = 'cosmos-ailab-${toLower(nameSuffix)}'

// Get current user for RBAC assignment
var currentUser = az.user()

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session' // Lowest cost consistency level
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: false
    enableFreeTier: false
    capabilities: [
      {
        name: 'EnableServerless' // Use serverless for cost savings
      }
    ]
    ipRules: []
    isVirtualNetworkFilterEnabled: false
    enableMultipleWriteLocations: false
    enableCassandraConnector: false
    connectorOffer: ''
    disableKeyBasedMetadataWriteAccess: false
    keyVaultKeyUri: ''
    defaultIdentity: 'FirstPartyIdentity' // Use managed identity
    publicNetworkAccess: 'Enabled'
    enableAnalyticalStorage: false
    networkAclBypass: 'None'
    networkAclBypassResourceIds: []
  }
  tags: {
    Project: projectName
    Environment: environment
  }
}

// Assign Cosmos DB Built-in Data Contributor role to current user
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(cosmosAccount.id, 'CosmosDataContributor', currentUser.objectId)
  scope: cosmosAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '00000000-0000-0000-0000-000000000002') // Cosmos DB Built-in Data Contributor
    principalId: currentUser.objectId
    principalType: 'User'
  }
}

output cosmosAccountName string = cosmosAccount.name
output cosmosAccountEndpoint string = cosmosAccount.properties.documentEndpoint
output resourceGroupName string = resourceGroup().name
