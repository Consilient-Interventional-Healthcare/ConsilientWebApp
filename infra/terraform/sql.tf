# Azure SQL Server and Databases

# Get current Azure AD client configuration for setting SQL admin
data "azurerm_client_config" "current" {}

resource "azurerm_mssql_server" "main" {
  name                = local.sql.server_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  version             = "12.0"
  administrator_login = var.sql_admin_username
  # IMPORTANT: Inject the password securely using environment variables, a secret manager, or your CI/CD pipeline. Never commit secrets.
  administrator_login_password  = var.sql_admin_password
  public_network_access_enabled = var.enable_local_firewall
  tags                          = local.tags

  # Configure Azure AD admin for service principal authentication
  azuread_administrator {
    login_username              = "SQL Admin Service Principal"
    object_id                   = data.azurerm_client_config.current.object_id
    tenant_id                   = data.azurerm_client_config.current.tenant_id
    azuread_authentication_only = false
  }
}

# Firewall rule for local act testing only
# WARNING: Opens SQL Server to all IPs - only for development testing
resource "azurerm_mssql_firewall_rule" "local_act" {
  count            = var.enable_local_firewall ? 1 : 0
  name             = "AllowLocalActTesting"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "255.255.255.255"
}

# Enable Advanced Threat Protection (controlled by cost profile)
resource "azurerm_mssql_server_security_alert_policy" "main" {
  count               = local.sql.threat_protection_enabled[var.environment] ? 1 : 0
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_mssql_server.main.name
  state               = "Enabled"
}

# Enable Auditing (controlled by cost profile - logs to storage)
resource "azurerm_mssql_server_extended_auditing_policy" "main" {
  count                      = local.sql.auditing_enabled[var.environment] ? 1 : 0
  server_id                  = azurerm_mssql_server.main.id
  storage_endpoint           = azurerm_storage_account.loki.primary_blob_endpoint
  storage_account_access_key = azurerm_storage_account.loki.primary_access_key
  retention_in_days          = local.sql.audit_retention_days[var.environment]
}

module "main_db" {
  source                      = "./modules/sql_database"
  name                        = local.sql.main_db.name
  resource_group_name         = azurerm_resource_group.main.name
  location                    = azurerm_resource_group.main.location
  server_name                 = azurerm_mssql_server.main.name
  server_id                   = azurerm_mssql_server.main.id
  sku_name                    = local.sql.main_db.sku
  zone_redundant              = local.sql.main_db.zone_redundant[var.environment]
  min_capacity                = local.sql.main_db.min_capacity[var.environment]
  auto_pause_delay_in_minutes = local.sql.main_db.auto_pause_delay[var.environment]
  tags = merge(local.tags, {
    criticality = "high"
    compliance  = "hipaa"
  })
}

module "hangfire_db" {
  source                      = "./modules/sql_database"
  name                        = local.sql.hangfire_db.name
  resource_group_name         = azurerm_resource_group.main.name
  location                    = azurerm_resource_group.main.location
  server_name                 = azurerm_mssql_server.main.name
  server_id                   = azurerm_mssql_server.main.id
  sku_name                    = local.sql.hangfire_db.sku
  zone_redundant              = local.sql.hangfire_db.zone_redundant[var.environment]
  min_capacity                = local.sql.hangfire_db.min_capacity[var.environment]
  auto_pause_delay_in_minutes = local.sql.hangfire_db.auto_pause_delay[var.environment]
  tags = merge(local.tags, {
    criticality = "low"
  })
}
