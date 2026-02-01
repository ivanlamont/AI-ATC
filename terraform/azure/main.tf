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

# Container Registry
resource "azurerm_container_registry" "aiatc" {
  name                = var.container_registry_name
  resource_group_name = azurerm_resource_group.aiatc.name
  location            = azurerm_resource_group.aiatc.location
  sku                 = "Standard"
  admin_enabled       = true
}

# AKS Cluster
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

  tags = var.common_tags
}

# PostgreSQL Database
resource "azurerm_postgresql_server" "aiatc" {
  name                = var.postgres_server_name
  location            = azurerm_resource_group.aiatc.location
  resource_group_name = azurerm_resource_group.aiatc.name

  administrator_login          = var.db_admin_username
  administrator_login_password = var.db_admin_password
  version                      = "12"
  ssl_enforcement_enabled      = true

  sku_name = "GP_Gen5_2"

  tags = var.common_tags
}

resource "azurerm_postgresql_database" "aiatc" {
  name                = "aiatc"
  resource_group_name = azurerm_resource_group.aiatc.name
  server_name         = azurerm_postgresql_server.aiatc.name
  charset             = "UTF8"
  collation           = "en_US.utf8"
}

# Redis Cache
resource "azurerm_redis_cache" "aiatc" {
  name                = var.redis_cache_name
  location            = azurerm_resource_group.aiatc.location
  resource_group_name = azurerm_resource_group.aiatc.name
  capacity            = 2
  family              = "C"
  sku_name            = "Standard"
  enable_non_ssl_port = false

  tags = var.common_tags
}

# Storage Account
resource "azurerm_storage_account" "aiatc" {
  name                     = var.storage_account_name
  resource_group_name      = azurerm_resource_group.aiatc.name
  location                 = azurerm_resource_group.aiatc.location
  account_tier             = "Standard"
  account_replication_type = "GRS"

  tags = var.common_tags
}
