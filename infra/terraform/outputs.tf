# Output values for Consilient Azure Infrastructure

output "resource_group_name" {
  description = "The name of the resource group"
  value       = azurerm_resource_group.main.name
}

# --------------------------------------------------------------------------
# COST TRANSPARENCY OUTPUTS
# --------------------------------------------------------------------------

output "selected_cost_profile" {
  description = "The cost profile being used for this environment"
  value       = var.environment
}

output "estimated_monthly_cost_usd" {
  description = "Estimated monthly cost in USD for this environment"
  value       = local.estimated_monthly_cost[var.environment]
}

output "cost_configuration" {
  description = "Detailed cost configuration for this environment"
  value = {
    api_app_sku           = local.api.sku
    react_app_sku         = local.react.sku
    main_db_sku           = local.sql.main_db.sku
    hangfire_db_sku       = local.sql.hangfire_db.sku
    acr_sku               = local.acr.sku
    sql_auditing          = local.sql.auditing_enabled[var.environment]
    sql_threat_protection = local.sql.threat_protection_enabled[var.environment]
  }
}

# --------------------------------------------------------------------------
# SQL SERVER OUTPUTS
# --------------------------------------------------------------------------

output "sql_server_fqdn" {
  description = "The fully qualified domain name of the Azure SQL Server"
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "sql_server_name" {
  description = "The name of the Azure SQL Server"
  value       = azurerm_mssql_server.main.name
}

output "sql_main_database_name" {
  description = "The name of the main database"
  value       = module.main_db.name
}

output "sql_hangfire_database_name" {
  description = "The name of the Hangfire database"
  value       = module.hangfire_db.name
}

# --------------------------------------------------------------------------
# CONTAINER REGISTRY OUTPUTS
# --------------------------------------------------------------------------

output "acr_registry_url" {
  description = "The URL of the Azure Container Registry"
  value       = azurerm_container_registry.main.login_server
}

output "acr_name" {
  description = "The name of the Azure Container Registry"
  value       = azurerm_container_registry.main.name
}

# Example: Mark sensitive outputs (add more as needed)
# output "sql_admin_password" {
#   value     = var.sql_admin_password
#   sensitive = true
# }

# --------------------------------------------------------------------------
# APP SERVICE OUTPUTS
# --------------------------------------------------------------------------

output "api_app_service_name" {
  description = "The name of the API App Service"
  value       = module.api_app.app_service_name
}

output "react_app_service_name" {
  description = "The name of the React App Service"
  value       = module.react_app.app_service_name
}

output "api_app_service_hostname" {
  description = "The default hostname of the API App Service"
  value       = module.api_app.app_service_default_hostname
}

output "react_app_service_hostname" {
  description = "The default hostname of the React App Service"
  value       = module.react_app.app_service_default_hostname
}

output "api_app_service_principal_id" {
  description = "The principal ID of the API App Service managed identity"
  value       = module.api_app.app_service_principal_id
}

output "react_app_service_principal_id" {
  description = "The principal ID of the React App Service managed identity"
  value       = module.react_app.app_service_principal_id
}

# --------------------------------------------------------------------------
# KEY VAULT OUTPUTS
# --------------------------------------------------------------------------

output "key_vault_name" {
  description = "The name of the Azure Key Vault"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "The URI of the Azure Key Vault"
  value       = azurerm_key_vault.main.vault_uri
}

output "key_vault_id" {
  description = "The resource ID of the Azure Key Vault"
  value       = azurerm_key_vault.main.id
}

# --------------------------------------------------------------------------
# AZURE APP CONFIGURATION OUTPUTS
# --------------------------------------------------------------------------
# Note: app_configuration_endpoint is defined in app_configuration.tf
# Additional outputs for reference:

output "app_configuration_name" {
  description = "The name of the Azure App Configuration"
  value       = azurerm_app_configuration.main.name
}

output "app_configuration_id" {
  description = "The resource ID of the Azure App Configuration"
  value       = azurerm_app_configuration.main.id
}

# --------------------------------------------------------------------------
# LOKI OUTPUTS
# --------------------------------------------------------------------------

output "loki_basic_auth_username" {
  description = "Loki Basic Auth username"
  value       = var.loki_basic_auth_username
  sensitive   = true
}

output "loki_basic_auth_password" {
  description = "Loki Basic Auth password"
  value       = local.loki_basic_auth_password
  sensitive   = true
}

output "loki_url" {
  description = "Loki endpoint URL"
  value       = "https://${azurerm_container_app.loki.ingress[0].fqdn}"
}
