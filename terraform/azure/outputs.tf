output "resource_group_id" {
  description = "Resource group ID"
  value       = azurerm_resource_group.aiatc.id
}

output "kubernetes_cluster_id" {
  description = "Kubernetes cluster ID"
  value       = azurerm_kubernetes_cluster.aiatc.id
}

output "kubernetes_cluster_name" {
  description = "Kubernetes cluster name"
  value       = azurerm_kubernetes_cluster.aiatc.name
}

output "container_registry_login_server" {
  description = "Container registry login server"
  value       = azurerm_container_registry.aiatc.login_server
}

output "postgresql_server_fqdn" {
  description = "PostgreSQL server FQDN"
  value       = azurerm_postgresql_server.aiatc.fqdn
}

output "redis_cache_hostname" {
  description = "Redis cache hostname"
  value       = azurerm_redis_cache.aiatc.hostname
}

output "storage_account_id" {
  description = "Storage account ID"
  value       = azurerm_storage_account.aiatc.id
}

output "kube_config" {
  description = "Kubernetes config"
  value       = azurerm_kubernetes_cluster.aiatc.kube_config_raw
  sensitive   = true
}
