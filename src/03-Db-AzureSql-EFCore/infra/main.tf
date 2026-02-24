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
  default     = "03-Db-AzureSql-EFCore"
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

variable "sql_admin_login" {
  description = "SQL Server administrator login"
  type        = string
  default     = "sqladmin"
  sensitive   = true
}

variable "sql_admin_password" {
  description = "SQL Server administrator password"
  type        = string
  default     = "ChangeMe123!"
  sensitive   = true
}

data "azurerm_client_config" "current" {}

data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

locals {
  name_suffix   = substr(md5(data.azurerm_resource_group.rg.id), 0, 13)
  database_name = "demo-db"
}

resource "azurerm_mssql_server" "sql_server" {
  name                         = "sql-ailab-${lower(local.name_suffix)}"
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password
  minimum_tls_version          = "1.2"
  public_network_access_enabled = true

  tags = {
    Project     = var.project_name
    Environment = var.environment
  }
}

resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.sql_server.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_database" "database" {
  name           = local.database_name
  server_id      = azurerm_mssql_server.sql_server.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb    = 2
  sku_name       = "Basic"
  
  storage_account_type = "Local"

  tags = {
    Project     = var.project_name
    Environment = var.environment
  }
}

resource "azurerm_role_assignment" "sql_db_contributor" {
  scope                = azurerm_mssql_server.sql_server.id
  role_definition_name = "SQL DB Contributor"
  principal_id         = data.azurerm_client_config.current.object_id
}

output "sql_server_name" {
  value = azurerm_mssql_server.sql_server.name
}

output "sql_server_fqdn" {
  value = azurerm_mssql_server.sql_server.fully_qualified_domain_name
}

output "database_name" {
  value = azurerm_mssql_database.database.name
}

output "resource_group_name" {
  value = var.resource_group_name
}

output "note" {
  value = "Add your IP to the firewall for local development: az sql server firewall-rule create --resource-group ${var.resource_group_name} --server ${azurerm_mssql_server.sql_server.name} --name AllowMyIP --start-ip-address <your-ip> --end-ip-address <your-ip>"
}
