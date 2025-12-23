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

# --------------------------------------------------------------------------
# ACR CI/CD SERVICE PRINCIPAL OUTPUTS
# --------------------------------------------------------------------------
# The service principal for ACR CI/CD is automatically created during Terraform apply
# Credentials are saved to files in the terraform directory:
#   - acr_client_id.txt: Service principal client ID
#   - acr_client_secret.txt: Service principal client secret
#   - acr_sp_object_id.txt: Service principal object ID
#
# These files are read by the .env.act update process for local testing.
# To view the credentials after apply:
#   - cat acr_client_id.txt
#   - cat acr_client_secret.txt
#   - cat acr_sp_object_id.txt

# Example: Mark sensitive outputs (add more as needed)
# output "sql_admin_password" {
#   value     = var.sql_admin_password
#   sensitive = true
# }
