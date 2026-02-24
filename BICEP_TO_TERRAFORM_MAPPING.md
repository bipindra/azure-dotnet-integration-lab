# Infrastructure as Code - Bicep to Terraform Mapping

This document provides a quick reference for the Bicep and Terraform infrastructure files in this repository.

## File Locations

| Project | Bicep File | Terraform File | Status |
|---------|-----------|----------------|---------|
| **00-Auth-KeyVault** | `src/00-Auth-KeyVault/infra/main.bicep` | `src/00-Auth-KeyVault/infra/main.tf` | ? Complete |
| **01-Storage-Blob** | `src/01-Storage-Blob/infra/main.bicep` | `src/01-Storage-Blob/infra/main.tf` | ? Complete |
| **02-Db-Cosmos** | `src/02-Db-Cosmos/infra/main.bicep` | `src/02-Db-Cosmos/infra/main.tf` | ? Complete |
| **03-Db-AzureSql-EFCore** | `src/03-Db-AzureSql-EFCore/infra/main.bicep` | `src/03-Db-AzureSql-EFCore/infra/main.tf` | ? Complete |
| **04-Messaging-ServiceBus** | `src/04-Messaging-ServiceBus/infra/main.bicep` | `src/04-Messaging-ServiceBus/infra/main.tf` | ? Complete |

## Resource Comparison

### 00-Auth-KeyVault
| Resource Type | Bicep | Terraform |
|--------------|-------|-----------|
| Key Vault | `Microsoft.KeyVault/vaults@2023-07-01` | `azurerm_key_vault` |
| Role Assignment | `Microsoft.Authorization/roleAssignments@2022-04-01` | `azurerm_role_assignment` |
| Secret | `Microsoft.KeyVault/vaults/secrets@2023-07-01` | `azurerm_key_vault_secret` |

### 01-Storage-Blob
| Resource Type | Bicep | Terraform |
|--------------|-------|-----------|
| Storage Account | `Microsoft.Storage/storageAccounts@2023-01-01` | `azurerm_storage_account` |
| Blob Service | `Microsoft.Storage/storageAccounts/blobServices@2023-01-01` | (implicit) |
| Container | `Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01` | `azurerm_storage_container` |
| Role Assignment | `Microsoft.Authorization/roleAssignments@2022-04-01` | `azurerm_role_assignment` |

### 02-Db-Cosmos
| Resource Type | Bicep | Terraform |
|--------------|-------|-----------|
| Cosmos DB Account | `Microsoft.DocumentDB/databaseAccounts@2023-04-15` | `azurerm_cosmosdb_account` |
| Role Assignment | `Microsoft.Authorization/roleAssignments@2022-04-01` | `azurerm_role_assignment` |

### 03-Db-AzureSql-EFCore
| Resource Type | Bicep | Terraform |
|--------------|-------|-----------|
| SQL Server | `Microsoft.Sql/servers@2023-05-01-preview` | `azurerm_mssql_server` |
| Firewall Rule | `Microsoft.Sql/servers/firewallRules@2023-05-01-preview` | `azurerm_mssql_firewall_rule` |
| SQL Database | `Microsoft.Sql/servers/databases@2023-05-01-preview` | `azurerm_mssql_database` |
| Role Assignment | `Microsoft.Authorization/roleAssignments@2022-04-01` | `azurerm_role_assignment` |

### 04-Messaging-ServiceBus
| Resource Type | Bicep | Terraform |
|--------------|-------|-----------|
| Service Bus Namespace | `Microsoft.ServiceBus/namespaces@2022-10-01-preview` | `azurerm_servicebus_namespace` |
| Queue | `Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview` | `azurerm_servicebus_queue` |
| Role Assignment | `Microsoft.Authorization/roleAssignments@2022-04-01` | `azurerm_role_assignment` |

## Key Differences

### 1. Syntax and Structure
- **Bicep:** Uses declarative ARM template syntax with `@` decorators
- **Terraform:** Uses HCL (HashiCorp Configuration Language) with blocks

### 2. Parameter vs Variable
```bicep
# Bicep
@description('The name of the project')
param projectName string = '00-Auth-KeyVault'
```

```hcl
# Terraform
variable "project_name" {
  description = "The name of the project"
  type        = string
  default     = "00-Auth-KeyVault"
}
```

### 3. Resource Declaration
```bicep
# Bicep
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: { ... }
}
```

```hcl
# Terraform
resource "azurerm_key_vault" "kv" {
  name     = "kv-ailab-${local.name_suffix}"
  location = var.location
  ...
}
```

### 4. Functions and Expressions
| Purpose | Bicep | Terraform |
|---------|-------|-----------|
| Unique String | `uniqueString(resourceGroup().id)` | `substr(md5(data.azurerm_resource_group.rg.id), 0, 13)` |
| Current User | `az.user()` | `data.azurerm_client_config.current` |
| Resource Group | `resourceGroup()` | `data.azurerm_resource_group.rg` |
| String Interpolation | `'${variable}'` | `"${var.variable}"` |
| To Lower | `toLower(string)` | `lower(string)` |

### 5. Outputs
```bicep
# Bicep
output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
```

```hcl
# Terraform
output "key_vault_name" {
  value = azurerm_key_vault.kv.name
}

output "key_vault_uri" {
  value = azurerm_key_vault.kv.vault_uri
}
```

## Deployment Commands

### Bicep
```bash
# Create resource group
az group create --name rg-ailab-dev --location eastus

# Deploy
az deployment group create \
  --resource-group rg-ailab-dev \
  --template-file main.bicep \
  --parameters environment=dev

# Delete
az group delete --name rg-ailab-dev
```

### Terraform
```bash
# Create resource group
az group create --name rg-ailab-dev --location eastus

# Initialize
terraform init

# Deploy
terraform plan
terraform apply

# Delete
terraform destroy
```

## Feature Parity

Both Bicep and Terraform implementations provide:
- ? Same resources with equivalent configurations
- ? RBAC role assignments for current user
- ? Cost-optimized SKUs (Basic, Standard LRS, Serverless)
- ? Security best practices (TLS 1.2, HTTPS only)
- ? Consistent naming conventions
- ? Tagging with Project and Environment
- ? Output values for integration with applications

## Choosing Between Bicep and Terraform

### Use Bicep if:
- You're exclusively working with Azure
- You prefer native Azure tooling
- You want Azure-native features first (new services, preview features)
- Your team is familiar with ARM templates

### Use Terraform if:
- You need multi-cloud support
- You want a mature ecosystem with many providers
- You prefer HCL syntax
- Your team is already using Terraform
- You need advanced state management features

## Next Steps

1. Review the [TERRAFORM_GUIDE.md](TERRAFORM_GUIDE.md) for detailed usage instructions
2. Choose either Bicep or Terraform based on your requirements
3. Customize variables/parameters for your environment
4. Deploy infrastructure for your project
5. Update application configuration (connection strings, endpoints, etc.)

## Support

For issues or questions:
- **Bicep:** [Azure Bicep Documentation](https://docs.microsoft.com/azure/azure-resource-manager/bicep/)
- **Terraform:** [Terraform Azure Provider Documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
