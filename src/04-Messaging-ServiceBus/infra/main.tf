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
  default     = "04-Messaging-ServiceBus"
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
  queue_name  = "demo-queue"
}

resource "azurerm_servicebus_namespace" "servicebus" {
  name                = "sb-ailab-${lower(local.name_suffix)}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "Basic"
  capacity            = 0
  
  minimum_tls_version           = "1.2"
  public_network_access_enabled = true

  tags = {
    Project     = var.project_name
    Environment = var.environment
  }
}

resource "azurerm_servicebus_queue" "queue" {
  name         = local.queue_name
  namespace_id = azurerm_servicebus_namespace.servicebus.id
  
  max_size_in_megabytes                = 1024
  default_message_ttl                  = "P1D"
  lock_duration                        = "PT30S"
  requires_duplicate_detection         = false
  requires_session                     = false
  dead_lettering_on_message_expiration = true
  max_delivery_count                   = 10
  enable_batched_operations            = true
}

resource "azurerm_role_assignment" "servicebus_data_owner" {
  scope                = azurerm_servicebus_namespace.servicebus.id
  role_definition_name = "Azure Service Bus Data Owner"
  principal_id         = data.azurerm_client_config.current.object_id
}

output "servicebus_namespace_name" {
  value = azurerm_servicebus_namespace.servicebus.name
}

output "servicebus_namespace_fqdn" {
  value = "${azurerm_servicebus_namespace.servicebus.name}.servicebus.windows.net"
}

output "queue_name" {
  value = azurerm_servicebus_queue.queue.name
}

output "resource_group_name" {
  value = var.resource_group_name
}
