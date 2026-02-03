#!/bin/bash
# Bash deployment script for 03-Db-AzureSql-EFCore

set -e

ENVIRONMENT="${1:-dev}"
LOCATION="${2:-eastus}"
ACTION="${3:-deploy}"

PROJECT_NAME="03-Db-AzureSql-EFCore"
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

# Generate secure password
SQL_PASSWORD=$(openssl rand -base64 16 | tr -d "=+/" | cut -c1-16)

# Deploy Bicep template
echo "Deploying Bicep template..."
echo "Note: SQL Server creation may take a few minutes..."
DEPLOYMENT_NAME="deploy-${PROJECT_NAME}-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --template-file "$BICEP_FILE" \
    --parameters projectName="$PROJECT_NAME" environment="$ENVIRONMENT" location="$LOCATION" sqlAdminPassword="$SQL_PASSWORD"

# Get outputs
echo ""
echo "=== Deployment Outputs ==="
SQL_SERVER_NAME=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --query 'properties.outputs.sqlServerName.value' -o tsv)

SQL_SERVER_FQDN=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --query 'properties.outputs.sqlServerFqdn.value' -o tsv)

DATABASE_NAME=$(az deployment group show \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$DEPLOYMENT_NAME" \
    --query 'properties.outputs.databaseName.value' -o tsv)

echo "SQL Server Name: ${SQL_SERVER_NAME}"
echo "SQL Server FQDN: ${SQL_SERVER_FQDN}"
echo "Database Name: ${DATABASE_NAME}"
echo ""
echo "Next steps:"
echo "1. Add your IP to the firewall (see README for instructions)"
echo "2. Set the SQL Server name in appsettings.json or environment variable:"
echo "   export AZURESQL__SERVERNAME=${SQL_SERVER_FQDN}"
echo "   export AZURESQL__DATABASENAME=${DATABASE_NAME}"
echo "3. Run the application: cd src/03-Db-AzureSql-EFCore && dotnet run"
