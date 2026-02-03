#!/bin/bash
# Bash deployment script for 00-Auth-KeyVault

set -e

ENVIRONMENT="${1:-dev}"
LOCATION="${2:-eastus}"
ACTION="${3:-deploy}"

PROJECT_NAME="00-Auth-KeyVault"
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
KEY_VAULT_NAME=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --query 'properties.outputs.keyVaultName.value' -o tsv)

KEY_VAULT_URI=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --query 'properties.outputs.keyVaultUri.value' -o tsv)

echo "Key Vault Name: ${KEY_VAULT_NAME}"
echo "Key Vault URI: ${KEY_VAULT_URI}"
echo ""
echo "Next steps:"
echo "1. Set the Key Vault URI in appsettings.json or environment variable:"
echo "   export KEYVAULT__VAULTURI=${KEY_VAULT_URI}"
echo "2. Run the application: cd src/00-Auth-KeyVault && dotnet run"
