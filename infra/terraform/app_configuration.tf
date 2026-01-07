# Azure App Configuration Service
# Single source of truth for all application runtime configuration
# Integrates with Azure Key Vault for secret references

# Configuration locals
locals {
  app_config = {
    # Name: consilient-appconfig-dev-a1b2c3
    name = "${var.project_name}-appconfig-${var.environment}-${local.unique_suffix}"

    # Use Standard SKU for prod (includes SLA, more requests), Free for dev
    # Free SKU: 10MB storage, 1000 req/day, no SLA
    # Standard SKU: 1GB storage, 30000 req/day, 99.9% SLA
    sku = var.environment == "prod" ? "standard" : "free"
  }
}

# Create Azure App Configuration resource
resource "azurerm_app_configuration" "main" {
  name                = local.app_config.name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = local.app_config.sku

  # Enable managed identity for accessing Key Vault references
  identity {
    type = "SystemAssigned"
  }

  # Soft delete for accidental deletion protection
  # 7 days for prod (compliance), 1 day for dev (faster recovery)
  soft_delete_retention_days = var.environment == "prod" ? 7 : 1

  # Purge protection prevents permanent deletion
  # Enable for prod (compliance), disable for dev (faster iterations)
  purge_protection_enabled = var.environment == "prod" ? true : false

  tags = local.tags
}

# Output the endpoint for use in app settings
output "app_configuration_endpoint" {
  value       = azurerm_app_configuration.main.endpoint
  description = "Azure App Configuration endpoint URL"
}

# ============================================================================
# RBAC ROLE ASSIGNMENTS
# ============================================================================

# Grant Terraform service principal "App Configuration Data Owner" role
# Allows Terraform to create/read/update/delete configuration keys
resource "azurerm_role_assignment" "terraform_appconfig_owner" {
  scope                = azurerm_app_configuration.main.id
  role_definition_name = "App Configuration Data Owner"
  principal_id         = data.azurerm_client_config.current.object_id

  depends_on = [azurerm_app_configuration.main]
}

# Grant API App Service "App Configuration Data Reader" role
# Allows API to read configuration at runtime via managed identity
resource "azurerm_role_assignment" "api_appconfig_reader" {
  scope                = azurerm_app_configuration.main.id
  role_definition_name = "App Configuration Data Reader"
  principal_id         = module.api_app.app_service_principal_id

  depends_on = [
    azurerm_app_configuration.main,
    module.api_app
  ]
}

# Grant App Configuration managed identity "Key Vault Secrets User" role
# Allows App Configuration to resolve Key Vault references at read time
resource "azurerm_role_assignment" "appconfig_keyvault_secrets_user" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_app_configuration.main.identity[0].principal_id

  depends_on = [
    azurerm_app_configuration.main,
    azurerm_key_vault.main
  ]
}

# ============================================================================
# CONFIGURATION KEYS - API SETTINGS
# ============================================================================

# API Authentication and JWT Configuration
resource "azurerm_app_configuration_key" "api_auth_enabled" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Api:Authentication:Enabled"
  label                  = var.environment
  value                  = "true"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# JWT Issuer (matches API app service URL for audience validation)
resource "azurerm_app_configuration_key" "api_jwt_issuer" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Api:Authentication:Jwt:Issuer"
  label                  = var.environment
  value                  = "https://${local.api.service_name}.azurewebsites.net"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# JWT Audience (matches API app service URL)
resource "azurerm_app_configuration_key" "api_jwt_audience" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Api:Authentication:Jwt:Audience"
  label                  = var.environment
  value                  = "https://${local.api.service_name}.azurewebsites.net"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# JWT Token Expiry (in minutes)
resource "azurerm_app_configuration_key" "api_jwt_expiry" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Api:Authentication:Jwt:ExpiryMinutes"
  label                  = var.environment
  value                  = "60"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# API Logging Configuration
resource "azurerm_app_configuration_key" "api_logging_default_level" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Api:Logging:LogLevel:Default"
  label                  = var.environment
  # Debug in dev, Information in prod
  value = var.environment == "dev" ? "Debug" : "Information"
  type  = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "api_logging_aspnetcore_level" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Api:Logging:LogLevel:Microsoft.AspNetCore"
  label                  = var.environment
  value                  = "Warning"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Grafana Loki Configuration (push endpoint path)
resource "azurerm_app_configuration_key" "api_loki_push_endpoint" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Api:Logging:GrafanaLoki:PushEndpoint"
  label                  = var.environment
  value                  = "/loki/api/v1/push"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "api_loki_batch_posting_limit" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Api:Logging:GrafanaLoki:BatchPostingLimit"
  label                  = var.environment
  value                  = "100"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# ASP.NET Core Environment
resource "azurerm_app_configuration_key" "aspnetcore_environment" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ASPNETCORE_ENVIRONMENT"
  label                  = var.environment
  value                  = var.environment == "dev" ? "Development" : "Production"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# ============================================================================
# CONFIGURATION KEYS - REACT/FRONTEND SETTINGS
# ============================================================================

# API Base URL for React app
resource "azurerm_app_configuration_key" "react_api_base_url" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:ApiBaseUrl"
  label                  = var.environment
  value                  = "https://${local.api.service_name}.azurewebsites.net"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# NOTE: React:Environment removed - this is a build-time variable set via Docker build args,
# not a runtime configuration. See react-apps.yml workflow for build-arg configuration.

# Debug Mode Feature Flag
resource "azurerm_app_configuration_key" "react_debug_mode" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:EnableDebugMode"
  label                  = var.environment
  value                  = tostring(local.react.enable_debug_mode[var.environment])
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Mock Services Feature Flag
resource "azurerm_app_configuration_key" "react_use_mock_services" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:UseMockServices"
  label                  = var.environment
  value                  = tostring(local.react.use_mock_services[var.environment])
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Remote Logging Feature Flag
resource "azurerm_app_configuration_key" "react_remote_logging" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:EnableRemoteLogging"
  label                  = var.environment
  value                  = tostring(local.react.enable_remote_logging[var.environment])
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Individual Mock Service Flags (all disabled)
resource "azurerm_app_configuration_key" "react_mock_auth_service" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:MockAuthService"
  label                  = var.environment
  value                  = "false"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "react_mock_employees_service" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:MockEmployeesService"
  label                  = var.environment
  value                  = "false"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "react_mock_daily_log_service" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:MockDailyLogService"
  label                  = var.environment
  value                  = "false"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "react_mock_app_settings_service" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:MockAppSettingsService"
  label                  = var.environment
  value                  = "false"
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# External Login Mock (enabled in dev for testing OAuth flows)
resource "azurerm_app_configuration_key" "react_external_login_mock" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:EnableExternalLoginMock"
  label                  = var.environment
  value                  = tostring(local.react.enable_debug_mode[var.environment])
  type                   = "kv"

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# ============================================================================
# CONFIGURATION KEYS - KEY VAULT REFERENCES
# ============================================================================
# These keys store references to secrets in Key Vault
# App Configuration resolves them at read time using its managed identity
# Format: @Microsoft.KeyVault(VaultName=<vault-name>;SecretName=<secret-name>)

# JWT Signing Secret Reference
resource "azurerm_app_configuration_key" "secrets_jwt_signing_secret" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Api:Authentication:Jwt:Secret"
  label                  = var.environment
  type                   = "vault" # Special type for Key Vault references
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/jwt-signing-secret"

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.jwt_signing_secret
  ]
}

# SQL Connection String - Main Database Reference
resource "azurerm_app_configuration_key" "secrets_sql_connection_main" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConnectionStrings:DefaultConnection"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/sql-connection-string-main"

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.sql_connection_main
  ]
}

# SQL Connection String - Hangfire Database Reference
resource "azurerm_app_configuration_key" "secrets_sql_connection_hangfire" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConnectionStrings:HangfireConnection"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/sql-connection-string-hangfire"

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.sql_connection_hangfire
  ]
}

# Grafana Loki URL Reference
resource "azurerm_app_configuration_key" "secrets_loki_url" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Api:Logging:GrafanaLoki:Url"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/grafana-loki-url"

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.grafana_loki_url
  ]
}

# OAuth Client Secret Reference (only if configured)
resource "azurerm_app_configuration_key" "secrets_oauth_client_secret" {
  count                  = var.oauth_client_secret != "" ? 1 : 0
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Api:Authentication:OAuth:ClientSecret"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/oauth-client-secret"

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.oauth_client_secret
  ]
}
