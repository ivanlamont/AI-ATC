variable "subscription_id" {
  description = "Azure subscription ID"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
  default     = "aiatc-rg"
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "East US"
}

variable "cluster_name" {
  description = "AKS cluster name"
  type        = string
  default     = "aiatc-aks"
}

variable "dns_prefix" {
  description = "DNS prefix for AKS"
  type        = string
  default     = "aiatc"
}

variable "node_count" {
  description = "Node count for AKS"
  type        = number
  default     = 3
}

variable "vm_size" {
  description = "VM size for AKS nodes"
  type        = string
  default     = "Standard_D2s_v3"
}

variable "container_registry_name" {
  description = "Container registry name"
  type        = string
  default     = "aiatcregistry"
}

variable "postgres_server_name" {
  description = "PostgreSQL server name"
  type        = string
  default     = "aiatc-db"
}

variable "db_admin_username" {
  description = "Database admin username"
  type        = string
  default     = "aiatcadmin"
}

variable "db_admin_password" {
  description = "Database admin password"
  type        = string
  sensitive   = true
}

variable "redis_cache_name" {
  description = "Redis cache name"
  type        = string
  default     = "aiatc-redis"
}

variable "storage_account_name" {
  description = "Storage account name"
  type        = string
  default     = "aiatcstorage"
}

variable "common_tags" {
  description = "Common tags for all resources"
  type        = map(string)
  default = {
    Project     = "AIATC"
    Environment = "Production"
    Terraform   = "true"
  }
}
