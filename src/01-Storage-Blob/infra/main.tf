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
  features {}
}

variable "project_name" {
  description = "The name of the project"
  type        = string
  default     = "01-Storage-Blob"
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
  name_suffix      = substr(md5(data.azurerm_resource_group.rg.id), 0, 13)
  container_name   = "demo-container"
}

resource "azurerm_storage_account" "storage" {
  name                     = "stailab${lower(local.name_suffix)}"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"
  access_tier              = "Hot"
  
  https_traffic_only_enabled       = true
  min_tls_version                  = "TLS1_2"
  allow_nested_items_to_be_public  = false
  shared_access_key_enabled        = true

  network_rules {
    default_action             = "Allow"
    bypass                     = ["AzureServices"]
  }

  tags = {
    Project     = var.project_name
    Environment = var.environment
  }
}

resource "azurerm_storage_container" "container" {
  name                  = local.container_name
  storage_account_name  = azurerm_storage_account.storage.name
  container_access_type = "private"
  
  metadata = {
    Project     = var.project_name
    Environment = var.environment
  }
}

resource "azurerm_role_assignment" "storage_blob_contributor" {
  scope                = azurerm_storage_account.storage.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azurerm_client_config.current.object_id
}

output "storage_account_name" {
  value = azurerm_storage_account.storage.name
}

output "storage_account_resource_id" {
  value = azurerm_storage_account.storage.id
}

output "container_name" {
  value = azurerm_storage_container.container.name
}

output "resource_group_name" {
  value = var.resource_group_name
}
