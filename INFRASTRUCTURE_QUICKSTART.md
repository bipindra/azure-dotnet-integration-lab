# Quick Start: Infrastructure Deployment

This guide helps you quickly deploy Azure infrastructure for any project in this repository using either Bicep or Terraform.

## Prerequisites

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed
- Azure subscription with appropriate permissions
- For Terraform: [Terraform CLI](https://www.terraform.io/downloads.html) installed

## Option 1: Deploy with Bicep (Azure Native)

### Step 1: Login to Azure
```bash
az login
```

### Step 2: Create Resource Group
```bash
az group create --name rg-ailab-dev --location eastus
```

### Step 3: Deploy
```bash
# Navigate to project directory (example: 00-Auth-KeyVault)
cd src/00-Auth-KeyVault/infra

# Deploy
az deployment group create \
  --resource-group rg-ailab-dev \
  --template-file main.bicep \
  --parameters environment=dev
```

### Step 4: View Outputs
```bash
az deployment group show \
  --resource-group rg-ailab-dev \
  --name main \
  --query properties.outputs
```

### Step 5: Clean Up
```bash
az group delete --name rg-ailab-dev --yes --no-wait
```

## Option 2: Deploy with Terraform (Multi-Cloud)

### Step 1: Login to Azure
```bash
az login
```

### Step 2: Create Resource Group
```bash
az group create --name rg-ailab-dev --location eastus
```

### Step 3: Initialize Terraform
```bash
# Navigate to project directory (example: 00-Auth-KeyVault)
cd src/00-Auth-KeyVault/infra

# Initialize Terraform
terraform init
```

### Step 4: Create Variables File
Create `terraform.tfvars`:
```hcl
resource_group_name = "rg-ailab-dev"
location            = "eastus"
environment         = "dev"
```

### Step 5: Deploy
```bash
# Review planned changes
terraform plan

# Apply changes
terraform apply
```

### Step 6: View Outputs
```bash
terraform output
```

### Step 7: Clean Up
```bash
terraform destroy
```

## Project-Specific Quick Commands

### 00-Auth-KeyVault
```bash
# Bicep
az deployment group create --resource-group rg-ailab-dev --template-file main.bicep

# Terraform
terraform apply -var="resource_group_name=rg-ailab-dev"
```

### 01-Storage-Blob
```bash
# Bicep
az deployment group create --resource-group rg-ailab-dev --template-file main.bicep

# Terraform
terraform apply -var="resource_group_name=rg-ailab-dev"
```

### 02-Db-Cosmos
```bash
# Bicep
az deployment group create --resource-group rg-ailab-dev --template-file main.bicep

# Terraform
terraform apply -var="resource_group_name=rg-ailab-dev"
```

### 03-Db-AzureSql-EFCore
```bash
# Bicep
az deployment group create --resource-group rg-ailab-dev --template-file main.bicep \
  --parameters sqlAdminPassword='YourSecurePassword123!'

# Terraform
terraform apply -var="resource_group_name=rg-ailab-dev" \
  -var="sql_admin_password=YourSecurePassword123!"
```

**Note:** Don't forget to add your IP to the SQL Server firewall:
```bash
# Get your public IP
MY_IP=$(curl -s ifconfig.me)

# Add firewall rule
az sql server firewall-rule create \
  --resource-group rg-ailab-dev \
  --server <sql-server-name> \
  --name AllowMyIP \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

### 04-Messaging-ServiceBus
```bash
# Bicep
az deployment group create --resource-group rg-ailab-dev --template-file main.bicep

# Terraform
terraform apply -var="resource_group_name=rg-ailab-dev"
```

## Common Issues and Solutions

### Issue: "Resource provider not registered"
**Solution:** Register the required provider
```bash
az provider register --namespace Microsoft.KeyVault
az provider register --namespace Microsoft.Storage
az provider register --namespace Microsoft.DocumentDB
az provider register --namespace Microsoft.Sql
az provider register --namespace Microsoft.ServiceBus
```

### Issue: "Insufficient permissions"
**Solution:** Ensure you have the following roles:
- Contributor role on the resource group
- User Access Administrator (for RBAC assignments)

### Issue: "Name already exists"
**Solution:** The unique suffix should prevent this, but if it occurs:
- Delete the existing resource, or
- Use a different resource group

### Issue: Terraform state lock
**Solution:**
```bash
# Force unlock (use with caution)
terraform force-unlock <lock-id>
```

## Best Practices

### 1. Use Separate Resource Groups per Environment
```bash
az group create --name rg-ailab-dev --location eastus
az group create --name rg-ailab-staging --location eastus
az group create --name rg-ailab-prod --location eastus
```

### 2. Use Parameter/Variable Files
**Bicep parameters file (`parameters.dev.json`):**
```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "environment": {
      "value": "dev"
    },
    "location": {
      "value": "eastus"
    }
  }
}
```

**Terraform variables file (`dev.tfvars`):**
```hcl
resource_group_name = "rg-ailab-dev"
environment         = "dev"
location            = "eastus"
```

### 3. Never Commit Secrets
Use environment variables or Azure Key Vault:
```bash
# Using environment variables with Terraform
export TF_VAR_sql_admin_password="YourSecurePassword123!"
terraform apply
```

### 4. Tag Your Resources
Both tools automatically tag resources with:
- Project name
- Environment

### 5. Review Before Applying
Always review changes before applying:
```bash
# Bicep - use what-if
az deployment group what-if \
  --resource-group rg-ailab-dev \
  --template-file main.bicep

# Terraform
terraform plan
```

## Deployment Time Estimates

| Project | Approximate Time |
|---------|-----------------|
| 00-Auth-KeyVault | ~1-2 minutes |
| 01-Storage-Blob | ~1-2 minutes |
| 02-Db-Cosmos | ~3-5 minutes |
| 03-Db-AzureSql-EFCore | ~3-5 minutes |
| 04-Messaging-ServiceBus | ~2-3 minutes |

## Getting Help

- **Bicep Documentation:** https://docs.microsoft.com/azure/azure-resource-manager/bicep/
- **Terraform Azure Provider:** https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs
- **Azure CLI Reference:** https://docs.microsoft.com/cli/azure/

## Next Steps

1. ? Deploy infrastructure for your project
2. ? Note the output values (endpoints, names, etc.)
3. ? Update your application's `appsettings.json` with the values
4. ? Run your .NET application
5. ? Clean up resources when done

For detailed information, see:
- [TERRAFORM_GUIDE.md](TERRAFORM_GUIDE.md) - Comprehensive Terraform guide
- [BICEP_TO_TERRAFORM_MAPPING.md](BICEP_TO_TERRAFORM_MAPPING.md) - Detailed comparison
