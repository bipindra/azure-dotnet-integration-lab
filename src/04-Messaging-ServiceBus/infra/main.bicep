@description('The name of the project')
param projectName string = '04-Messaging-ServiceBus'

@description('The environment name (dev, staging, prod)')
param environment string = 'dev'

@description('The Azure region to deploy resources')
param location string = resourceGroup().location

@description('Random suffix for unique resource names')
param nameSuffix string = uniqueString(resourceGroup().id)

// Service Bus Namespace
var serviceBusNamespaceName = 'sb-ailab-${toLower(nameSuffix)}'
var queueName = 'demo-queue'

// Get current user for RBAC assignment
var currentUser = az.user()

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Basic' // Lowest cost tier
    tier: 'Basic'
    capacity: 1
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
  tags: {
    Project: projectName
    Environment: environment
  }
}

// Queue
resource queue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: queueName
  properties: {
    maxSizeInMegabytes: 1024 // 1 GB (Basic tier max)
    defaultMessageTimeToLive: 'PT24H' // 24 hours
    lockDuration: 'PT30S' // 30 seconds
    requiresDuplicateDetection: false
    requiresSession: false
    deadLetteringOnMessageExpiration: true
    maxDeliveryCount: 10
    enableBatchedOperations: true
  }
}

// Assign Azure Service Bus Data Owner role to current user
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBusNamespace.id, 'ServiceBusDataOwner', currentUser.objectId)
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '090c5cfd-751d-490a-894a-3ce6f1109419') // Azure Service Bus Data Owner
    principalId: currentUser.objectId
    principalType: 'User'
  }
}

output serviceBusNamespaceName string = serviceBusNamespace.name
output serviceBusNamespaceFqdn string = '${serviceBusNamespace.name}.servicebus.windows.net'
output queueName string = queue.name
output resourceGroupName string = resourceGroup().name
