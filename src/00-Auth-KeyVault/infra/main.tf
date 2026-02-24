terraform {
  required_version = ">= 1.0"
  
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = true
      recover_soft_deleted_key_vaults = false
    }
  }
}

variable "project_name" {
  description = "The name of the project"
  type        = string
  default     = "00-Auth-KeyVault"
}

variable "environment" {
  description = "The environment name (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "location" {
  description = "The Azure region to deploy resources"
  type        = string
  default     = "East US"
}

variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

data "azurerm_client_config" "current" {}

data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

locals {
  name_suffix = substr(md5(data.azurerm_resource_group.rg.id), 0, 13)
}

resource "azurerm_key_vault" "kv" {
  name                            = "kv-ailab-${local.name_suffix}"
  location                        = var.location
  resource_group_name             = var.resource_group_name
  tenant_id                       = data.azurerm_client_config.current.tenant_id
  sku_name                        = "standard"
  enabled_for_deployment          = false
  enabled_for_template_deployment = false
  enabled_for_disk_encryption     = false
  enable_rbac_authorization       = true
  soft_delete_retention_days      = 7
  purge_protection_enabled        = false
  public_network_access_enabled   = true
  
  network_acls {
    default_action             = "Allow"
    bypass                     = "AzureServices"
  }

  tags = {
    Project     = var.project_name
    Environment = var.environment
  }
}

resource "azurerm_role_assignment" "kv_secrets_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = data.azurerm_client_config.current.object_id
}

resource "azurerm_key_vault_secret" "sample_secret" {
  name         = "SampleSecret"
  value        = "This is a sample secret value"
  content_type = "text/plain"
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_role_assignment.kv_secrets_user]
}

output "key_vault_name" {
  value = azurerm_key_vault.kv.name
}

output "key_vault_uri" {
  value = azurerm_key_vault.kv.vault_uri
}

output "resource_group_name" {
  value = var.resource_group_name
}
