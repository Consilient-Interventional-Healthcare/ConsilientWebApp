# Azure Key Vault for storing application secrets
# Uses Azure RBAC for access control (not legacy access policies)

# Generate Key Vault name (must be globally unique, 3-24 characters)
locals {
  # Key Vault naming: project-kv-env-suffix (e.g., consilient-kv-dev-a1b2c3)
  keyvault = {
    name = "${var.project_name}-kv-${var.environment}-${local.unique_suffix}"
  }
}

resource "azurerm_key_vault" "main" {
  name                = local.keyvault.name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard" # Standard tier is sufficient for secrets

  # RBAC authorization model (not legacy access policies)
  rbac_authorization_enabled      = true
  enabled_for_deployment          = false
  enabled_for_disk_encryption     = false
  enabled_for_template_deployment = false

  # Network security
  public_network_access_enabled = true # Required for App Service Key Vault references
  network_acls {
    bypass         = "AzureServices" # Allow Azure services to bypass firewall
    default_action = "Allow"         # For dev; use "Deny" + IP rules in prod
  }

  # Security settings
  purge_protection_enabled   = var.environment == "prod" ? true : false # Prevent permanent deletion in prod
  soft_delete_retention_days = 90

  tags = local.tags
}

# Grant API App Service "Key Vault Secrets User" role (read-only access to secrets)
resource "azurerm_role_assignment" "api_keyvault_secrets_user" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User" # Built-in role for reading secrets
  principal_id         = module.api_app.app_service_principal_id

  # Prevent race condition - ensure identity exists before assigning role
  depends_on = [module.api_app]
}

# Grant Terraform service principal "Key Vault Secrets Officer" role (manage secrets)
# This allows Terraform to create/update/delete secrets
resource "azurerm_role_assignment" "terraform_keyvault_secrets_officer" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets Officer" # Built-in role for managing secrets
  principal_id         = data.azurerm_client_config.current.object_id

  depends_on = [azurerm_key_vault.main]
}

# Create secrets in Key Vault
# These will be populated during terraform apply using variables

# SQL Connection String - Main Database
resource "azurerm_key_vault_secret" "sql_connection_main" {
  name         = "sql-connection-string-main"
  value        = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${module.main_db.name};User Id=${var.sql_admin_username};Password=${var.sql_admin_password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [
    azurerm_role_assignment.terraform_keyvault_secrets_officer,
    azurerm_mssql_server.main,
    module.main_db
  ]

  tags = merge(local.tags, {
    description = "Main database connection string"
  })
}

# SQL Connection String - Hangfire Database
resource "azurerm_key_vault_secret" "sql_connection_hangfire" {
  name         = "sql-connection-string-hangfire"
  value        = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${module.hangfire_db.name};User Id=${var.sql_admin_username};Password=${var.sql_admin_password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [
    azurerm_role_assignment.terraform_keyvault_secrets_officer,
    azurerm_mssql_server.main,
    module.hangfire_db
  ]

  tags = merge(local.tags, {
    description = "Hangfire database connection string"
  })
}

# JWT Signing Secret
resource "azurerm_key_vault_secret" "jwt_signing_secret" {
  name         = "jwt-signing-secret"
  value        = var.jwt_signing_secret
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_role_assignment.terraform_keyvault_secrets_officer]

  tags = merge(local.tags, {
    description = "JWT signing secret for authentication"
  })
}

# Grafana Loki URL
resource "azurerm_key_vault_secret" "grafana_loki_url" {
  name         = "grafana-loki-url"
  value        = "http://${azurerm_container_app.loki.ingress[0].fqdn}"
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [
    azurerm_role_assignment.terraform_keyvault_secrets_officer,
    azurerm_container_app.loki
  ]

  tags = merge(local.tags, {
    description = "Grafana Loki push endpoint URL"
  })
}

# OAuth Client Secret (optional - only if OAuth is configured)
resource "azurerm_key_vault_secret" "oauth_client_secret" {
  count        = var.oauth_client_secret != "" ? 1 : 0
  name         = "oauth-client-secret"
  value        = var.oauth_client_secret
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_role_assignment.terraform_keyvault_secrets_officer]

  tags = merge(local.tags, {
    description = "OAuth provider client secret"
  })
}
