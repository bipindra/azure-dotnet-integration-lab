# Terraform Infrastructure Files

This directory contains Terraform configurations that are equivalent to the Bicep files in this repository. Each project's `infra` folder now contains both Bicep (`main.bicep`) and Terraform (`main.tf`) files.

## Prerequisites

- [Terraform](https://www.terraform.io/downloads.html) (>= 1.0)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- An active Azure subscription

## Getting Started

### 1. Authenticate with Azure

```bash
az login
```

### 2. Create a Resource Group

Before deploying any infrastructure, create a resource group:

```bash
az group create --name rg-ailab-dev --location eastus
```

### 3. Initialize Terraform

Navigate to the project's `infra` directory and initialize Terraform:

```bash
cd src/00-Auth-KeyVault/infra
terraform init
```

### 4. Deploy Infrastructure

Create a `terraform.tfvars` file to customize variables:

```hcl
resource_group_name = "rg-ailab-dev"
location            = "eastus"
environment         = "dev"
```

Then deploy:

```bash
terraform plan
terraform apply
```

### 5. Destroy Infrastructure

When you're done, clean up resources:

```bash
terraform destroy
```

## Project-Specific Notes

### 00-Auth-KeyVault
- Creates an Azure Key Vault with RBAC authorization
- Assigns Key Vault Secrets User role to the current user
- Creates a sample secret for testing

### 01-Storage-Blob
- Creates a Storage Account with a blob container
- Uses Standard LRS replication for cost savings
- Assigns Storage Blob Data Contributor role to the current user

### 02-Db-Cosmos
- Creates a Cosmos DB account with serverless capability
- Uses Session consistency level (lowest cost)
- Assigns Cosmos DB Built-in Data Contributor role to the current user

### 03-Db-AzureSql-EFCore
- Creates an Azure SQL Server and Database (Basic tier)
- Allows Azure services through firewall
- **Note:** Add your IP address to the firewall for local development:
  ```bash
  az sql server firewall-rule create \
    --resource-group <rg-name> \
    --server <server-name> \
    --name AllowMyIP \
    --start-ip-address <your-ip> \
    --end-ip-address <your-ip>
  ```

### 04-Messaging-ServiceBus
- Creates a Service Bus namespace with Basic SKU
- Creates a demo queue
- Assigns Azure Service Bus Data Owner role to the current user

## Differences Between Bicep and Terraform

### Name Generation
- **Bicep:** Uses `uniqueString(resourceGroup().id)` to generate unique suffixes
- **Terraform:** Uses `md5(data.azurerm_resource_group.rg.id)` with substring to generate similar unique suffixes

### Current User Identity
- **Bicep:** Uses `az.user()` function
- **Terraform:** Uses `data.azurerm_client_config.current` data source

### Resource Group
- **Bicep:** Uses `resourceGroup()` function to get context automatically
- **Terraform:** Requires explicit resource group name as a variable and uses data source to reference it

## Common Variables

All Terraform configurations support the following variables:

| Variable | Description | Default |
|----------|-------------|---------|
| `project_name` | The name of the project | (varies by project) |
| `environment` | The environment name | `dev` |
| `location` | The Azure region | `East US` |
| `resource_group_name` | The resource group name | (required) |

## Outputs

Each Terraform configuration provides outputs that match the Bicep outputs:

- Resource names
- Resource endpoints/URIs
- Resource group name
- Additional configuration notes (where applicable)

## Best Practices

1. **Use Remote State:** For team collaboration, configure Terraform to use remote state (Azure Storage Account)
2. **Use Workspaces:** Separate environments using Terraform workspaces
3. **Variable Files:** Use `.tfvars` files for environment-specific configurations
4. **Secrets Management:** Never commit sensitive values; use Azure Key Vault or environment variables
5. **State Locking:** Enable state locking when using remote state to prevent conflicts

## Remote State Configuration (Optional)

To use Azure Storage for remote state:

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-terraform-state"
    storage_account_name = "sttfstate"
    container_name       = "tfstate"
    key                  = "00-auth-keyvault.tfstate"
  }
}
```

## Troubleshooting

### Provider Registration
If you encounter provider registration errors:
```bash
az provider register --namespace Microsoft.KeyVault
az provider register --namespace Microsoft.Storage
az provider register --namespace Microsoft.DocumentDB
az provider register --namespace Microsoft.Sql
az provider register --namespace Microsoft.ServiceBus
```

### Permission Issues
Ensure your Azure user account has sufficient permissions:
- Contributor role on the resource group
- User Access Administrator role for RBAC assignments

### Name Conflicts
If resource names conflict, modify the `name_suffix` logic or manually override resource names using variables.

## Additional Resources

- [Terraform Azure Provider Documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure Naming Conventions](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/naming-and-tagging)
- [Terraform Best Practices](https://www.terraform-best-practices.com/)
