#!/bin/bash
# Bash deployment script for 02-Db-Cosmos

set -e

ENVIRONMENT="${1:-dev}"
LOCATION="${2:-eastus}"
ACTION="${3:-deploy}"

PROJECT_NAME="02-Db-Cosmos"
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
echo "Note: Cosmos DB account creation may take 5-10 minutes..."
DEPLOYMENT_NAME="deploy-${PROJECT_NAME}-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --template-file "$BICEP_FILE" \
    --parameters projectName="$PROJECT_NAME" environment="$ENVIRONMENT" location="$LOCATION"

# Get outputs
echo ""
echo "=== Deployment Outputs ==="
COSMOS_ACCOUNT_NAME=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --query 'properties.outputs.cosmosAccountName.value' -o tsv)

COSMOS_ENDPOINT=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --query 'properties.outputs.cosmosAccountEndpoint.value' -o tsv)

echo "Cosmos DB Account Name: ${COSMOS_ACCOUNT_NAME}"
echo "Cosmos DB Endpoint: ${COSMOS_ENDPOINT}"
echo ""
echo "Next steps:"
echo "1. Set the Cosmos DB endpoint in appsettings.json or environment variable:"
echo "   export COSMOSDB__ACCOUNTENDPOINT=${COSMOS_ENDPOINT}"
echo "2. Run the application: cd src/02-Db-Cosmos && dotnet run"
