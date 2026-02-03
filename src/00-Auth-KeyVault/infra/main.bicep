@description('The name of the project')
param projectName string = '00-Auth-KeyVault'

@description('The environment name (dev, staging, prod)')
param environment string = 'dev'

@description('The Azure region to deploy resources')
param location string = resourceGroup().location

@description('Random suffix for unique resource names')
param nameSuffix string = uniqueString(resourceGroup().id)

// Key Vault
var keyVaultName = 'kv-ailab-${nameSuffix}'

// Get current user for RBAC assignment
var currentUser = az.user()

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard' // Free tier
    }
    accessPolicies: []
    enabledForDeployment: false
    enabledForTemplateDeployment: false
    enabledForDiskEncryption: false
    enableRbacAuthorization: true // Use RBAC instead of access policies
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enablePurgeProtection: false // Disable for cost savings in dev
    publicNetworkAccess: 'Enabled'
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

// Assign Key Vault Secrets User role to current user
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, 'KeyVaultSecretsUser', currentUser.objectId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: currentUser.objectId
    principalType: 'User'
  }
}

// Create a sample secret (optional)
resource testSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'SampleSecret'
  properties: {
    value: 'This is a sample secret value'
    contentType: 'text/plain'
  }
}

output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
output resourceGroupName string = resourceGroup().name
