# Azure SQL Server and Databases

resource "azurerm_mssql_server" "main" {
  name                         = "sqlsrv-${var.environment}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  # IMPORTANT: Inject the password securely using environment variables, a secret manager, or your CI/CD pipeline. Never commit secrets.
  administrator_login_password = var.sql_admin_password
  public_network_access_enabled = false
  tags                        = local.tags
}

# Enable Advanced Threat Protection
resource "azurerm_mssql_server_security_alert_policy" "main" {
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_mssql_server.main.name
  state               = "Enabled"
}

# Enable Auditing (logs to storage)
resource "azurerm_mssql_server_extended_auditing_policy" "main" {
  server_id                    = azurerm_mssql_server.main.id
  storage_endpoint             = azurerm_storage_account.loki.primary_blob_endpoint
  storage_account_access_key   = azurerm_storage_account.loki.primary_access_key
  retention_in_days            = 90
}

module "main_db" {
  source              = "./modules/sql_database"
  name                = "consilient_main_${var.environment}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  server_name         = azurerm_mssql_server.main.name
  server_id           = azurerm_mssql_server.main.id   # <-- Add this line
  sku_name            = var.main_db_sku_name
  zone_redundant      = true
  tags                = merge(local.tags, {
    criticality = "high"
    compliance  = "hipaa"
  })
}

module "hangfire_db" {
  source              = "./modules/sql_database"
  name                = "consilient_hangfire_${var.environment}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  server_name         = azurerm_mssql_server.main.name
  server_id           = azurerm_mssql_server.main.id   # <-- Add this line
  sku_name            = var.hangfire_db_sku_name
  zone_redundant      = false
  tags                = merge(local.tags, {
    criticality = "low"
  })
}
