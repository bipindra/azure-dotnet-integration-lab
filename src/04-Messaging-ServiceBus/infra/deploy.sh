#!/bin/bash
# Bash deployment script for 04-Messaging-ServiceBus

set -e

ENVIRONMENT="${1:-dev}"
LOCATION="${2:-eastus}"
ACTION="${3:-deploy}"

PROJECT_NAME="04-Messaging-ServiceBus"
RESOURCE_GROUP_NAME="rg-ailab-${ENVIRONMENT}"
BICEP_FILE="$(dirname "$0")/main.bicep"

echo "=== Azure Integration Lab - ${PROJECT_NAME} ==="
echo "Environment: ${ENVIRONMENT}"
echo "Location: ${LOCATION}"
echo "Action: ${ACTION}"
echo ""

if [ "$ACTION" = "destroy" ]; then
    echo "Destroying resources..."
    az group delete --name "$RESOURCE_GROUP_NAME" --yes --no-wait
    echo "Resource group deletion initiated."
    exit 0
fi

# Check if logged in
if ! az account show &>/dev/null; then
    echo "Not logged in to Azure. Please run 'az login'"
    exit 1
fi

# Create resource group if it doesn't exist
echo "Ensuring resource group exists..."
az group create --name "$RESOURCE_GROUP_NAME" --location "$LOCATION" --output none

# Deploy Bicep template
echo "Deploying Bicep template..."
DEPLOYMENT_NAME="deploy-${PROJECT_NAME}-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --template-file "$BICEP_FILE" \
    --parameters projectName="$PROJECT_NAME" environment="$ENVIRONMENT" location="$LOCATION"

# Get outputs
echo ""
echo "=== Deployment Outputs ==="
SERVICE_BUS_NAMESPACE=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --query 'properties.outputs.serviceBusNamespaceName.value' -o tsv)

SERVICE_BUS_FQDN=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --query 'properties.outputs.serviceBusNamespaceFqdn.value' -o tsv)

QUEUE_NAME=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --query 'properties.outputs.queueName.value' -o tsv)

echo "Service Bus Namespace: ${SERVICE_BUS_NAMESPACE}"
echo "Service Bus FQDN: ${SERVICE_BUS_FQDN}"
echo "Queue Name: ${QUEUE_NAME}"
echo ""
echo "Next steps:"
echo "1. Set the Service Bus namespace in appsettings.json or environment variable:"
echo "   export SERVICEBUS__NAMESPACE=${SERVICE_BUS_NAMESPACE}"
echo "   export SERVICEBUS__QUEUENAME=${QUEUE_NAME}"
echo "2. Run the sender: cd src/04-Messaging-ServiceBus && dotnet run sender"
echo "3. Run the receiver (in another terminal): dotnet run receiver"
