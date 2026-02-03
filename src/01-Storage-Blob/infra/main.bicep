@description('The name of the project')
param projectName string = '01-Storage-Blob'

@description('The environment name (dev, staging, prod)')
param environment string = 'dev'

@description('The Azure region to deploy resources')
param location string = resourceGroup().location

@description('Random suffix for unique resource names')
param nameSuffix string = uniqueString(resourceGroup().id)

// Storage Account
var storageAccountName = 'stailab${toLower(nameSuffix)}' // Storage account names must be lowercase and 3-24 chars
var containerName = 'demo-container'

// Get current user for RBAC assignment
var currentUser = az.user()

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS' // Lowest cost option
  }
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    networkAcls: {
      defaultAction: 'Allow' // Allow all for dev; restrict in prod
      bypass: 'AzureServices'
    }
  }
  tags: {
    Project: projectName
    Environment: environment
  }
}

// Blob service
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

// Container
resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: containerName
  properties: {
    publicAccess: 'None'
    metadata: {
      Project: projectName
      Environment: environment
    }
  }
}

// Assign Storage Blob Data Contributor role to current user
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, 'StorageBlobDataContributor', currentUser.objectId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // Storage Blob Data Contributor
    principalId: currentUser.objectId
    principalType: 'User'
  }
}

output storageAccountName string = storageAccount.name
output storageAccountResourceId string = storageAccount.id
output containerName string = container.name
output resourceGroupName string = resourceGroup().name
