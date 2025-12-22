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

# Example: Mark sensitive outputs (add more as needed)
# output "sql_admin_password" {
#   value     = var.sql_admin_password
#   sensitive = true
# }
