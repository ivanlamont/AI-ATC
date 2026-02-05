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

# Resource Group
resource "azurerm_resource_group" "aiatc" {
  name     = var.resource_group_name
  location = var.location
  tags     = var.common_tags
}

# Container Registry - Cost optimized
resource "azurerm_container_registry" "aiatc" {
  name                = var.container_registry_name
  resource_group_name = azurerm_resource_group.aiatc.name
  location            = azurerm_resource_group.aiatc.location
  sku                 = "Basic"  # Changed from Standard to Basic for cost savings
  admin_enabled       = true
}

# AKS Cluster - Cost optimized for 3 users
resource "azurerm_kubernetes_cluster" "aiatc" {
  name                = var.cluster_name
  location            = azurerm_resource_group.aiatc.location
  resource_group_name = azurerm_resource_group.aiatc.name
  dns_prefix          = var.dns_prefix

  default_node_pool {
    name       = "default"
    node_count = var.node_count
    vm_size    = var.vm_size
  }

  identity {
    type = "SystemAssigned"
  }

  # Cost optimization: Use free tier where possible
  sku_tier = "Free"

  tags = var.common_tags
}

# PostgreSQL Database - Cost optimized
resource "azurerm_postgresql_server" "aiatc" {
  name                = var.postgres_server_name
  location            = azurerm_resource_group.aiatc.location
  resource_group_name = azurerm_resource_group.aiatc.name

  administrator_login          = var.db_admin_username
  administrator_login_password = var.db_admin_password
  version                      = "11"
  ssl_enforcement_enabled      = true

  # Cost optimization: Use Basic tier instead of General Purpose
  sku_name = "B_Gen5_1"  # 1 vCore, Basic tier

  # Cost optimization: Use minimal storage
  storage_mb                   = 5120  # 5GB minimum
  backup_retention_days        = 7     # Minimal retention

  tags = var.common_tags
}

resource "azurerm_postgresql_database" "aiatc" {
  name                = "aiatc"
  resource_group_name = azurerm_resource_group.aiatc.name
  server_name         = azurerm_postgresql_server.aiatc.name
  charset             = "UTF8"
  collation           = "en_US.utf8"
}

# Redis Cache - Cost optimized
resource "azurerm_redis_cache" "aiatc" {
  name                = var.redis_cache_name
  location            = azurerm_resource_group.aiatc.location
  resource_group_name = azurerm_resource_group.aiatc.name
  capacity            = 0    # Basic tier (C0)
  family              = "C"
  sku_name            = "Basic"  # Changed from Standard to Basic
  enable_non_ssl_port = false

  tags = var.common_tags
}

# Storage Account - Cost optimized
resource "azurerm_storage_account" "aiatc" {
  name                     = var.storage_account_name
  resource_group_name      = azurerm_resource_group.aiatc.name
  location                 = azurerm_resource_group.aiatc.location
  account_tier             = "Standard"
  account_replication_type = "LRS"  # Changed from GRS to LRS for cost savings

  tags = var.common_tags
}
