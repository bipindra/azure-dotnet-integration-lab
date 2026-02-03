@description('The name of the project')
param projectName string = '03-Db-AzureSql-EFCore'

@description('The environment name (dev, staging, prod)')
param environment string = 'dev'

@description('The Azure region to deploy resources')
param location string = resourceGroup().location

@description('Random suffix for unique resource names')
param nameSuffix string = uniqueString(resourceGroup().id)

@description('SQL Server administrator login')
@secure()
param sqlAdminLogin string = 'sqladmin'

@description('SQL Server administrator password')
@secure()
param sqlAdminPassword string = 'ChangeMe123!'

// SQL Server
var sqlServerName = 'sql-ailab-${toLower(nameSuffix)}'
var databaseName = 'demo-db'

// Get current user for RBAC assignment
var currentUser = az.user()

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
  tags: {
    Project: projectName
    Environment: environment
  }
}

// Firewall rule: Allow Azure services
resource firewallRuleAzure 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// SQL Database (Basic tier for cost savings)
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: 'Basic' // Lowest cost tier
    tier: 'Basic'
    capacity: 5 // 2 DTU (minimum)
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648 // 2 GB
    requestedBackupStorageRedundancy: 'Local'
  }
  tags: {
    Project: projectName
    Environment: environment
  }
}

// Assign SQL DB Contributor role to current user
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sqlServer.id, 'SqlDbContributor', currentUser.objectId)
  scope: sqlServer
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '9b7fa4d4-37aa-4173-89f0-9e9b9d7b7c7b') // SQL DB Contributor
    principalId: currentUser.objectId
    principalType: 'User'
  }
}

output sqlServerName string = sqlServer.name
output sqlServerFqdn string = '${sqlServer.name}.database.windows.net'
output databaseName string = sqlDatabase.name
output resourceGroupName string = resourceGroup().name
output note string = 'Add your IP to the firewall for local development: az sql server firewall-rule create --resource-group <rg> --server <server> --name AllowMyIP --start-ip-address <your-ip> --end-ip-address <your-ip>'
