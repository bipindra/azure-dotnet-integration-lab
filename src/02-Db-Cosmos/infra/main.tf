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
  default     = "02-Db-Cosmos"
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

resource "azurerm_cosmosdb_account" "cosmos" {
  name                = "cosmos-ailab-${lower(local.name_suffix)}"
  location            = var.location
  resource_group_name = var.resource_group_name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  
  consistency_policy {
    consistency_level = "Session"
  }
  
  geo_location {
    location          = var.location
    failover_priority = 0
  }
  
  capabilities {
    name = "EnableServerless"
  }
  
  public_network_access_enabled     = true
  is_virtual_network_filter_enabled = false
  enable_automatic_failover         = false
  enable_multiple_write_locations   = false
  
  tags = {
    Project     = var.project_name
    Environment = var.environment
  }
}

resource "azurerm_role_assignment" "cosmos_contributor" {
  scope                = azurerm_cosmosdb_account.cosmos.id
  role_definition_name = "Cosmos DB Built-in Data Contributor"
  principal_id         = data.azurerm_client_config.current.object_id
}

output "cosmos_account_name" {
  value = azurerm_cosmosdb_account.cosmos.name
}

output "cosmos_account_endpoint" {
  value = azurerm_cosmosdb_account.cosmos.endpoint
}

output "resource_group_name" {
  value = var.resource_group_name
}
