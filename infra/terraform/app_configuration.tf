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

  # Soft delete for accidental deletion protection (Standard SKU only)
  # Free SKU does not support soft delete
  # 7 days for prod (compliance)
  soft_delete_retention_days = var.environment == "prod" ? 7 : null

  # Purge protection prevents permanent deletion (Standard SKU only)
  # Free SKU does not support purge protection
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
# Role assignments for App Configuration have been moved to permissions.tf
# See: infra/terraform/permissions.tf

# ============================================================================
# CONFIGURATION KEYS - API SETTINGS
# ============================================================================

# API Authentication and JWT Configuration
# Keys use ConsilientApi: prefix which is stripped by the API at runtime via TrimKeyPrefix()
resource "azurerm_app_configuration_key" "api_auth_enabled" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ApplicationSettings:Authentication:Enabled"
  label                  = var.environment
  value                  = "true"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# JWT Issuer (matches API app service URL for audience validation)
resource "azurerm_app_configuration_key" "api_jwt_issuer" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ApplicationSettings:Authentication:UserService:Jwt:Issuer"
  label                  = var.environment
  value                  = "https://${local.api.service_name}.azurewebsites.net"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# JWT Audience (matches API app service URL)
resource "azurerm_app_configuration_key" "api_jwt_audience" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ApplicationSettings:Authentication:UserService:Jwt:Audience"
  label                  = var.environment
  value                  = "https://${local.api.service_name}.azurewebsites.net"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# JWT Token Expiry (in minutes)
resource "azurerm_app_configuration_key" "api_jwt_expiry" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ApplicationSettings:Authentication:UserService:Jwt:ExpiryMinutes"
  label                  = var.environment
  value                  = "60"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# API Logging Configuration
resource "azurerm_app_configuration_key" "api_logging_default_level" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Logging:LogLevel:Default"
  label                  = var.environment
  # Debug in dev, Information in prod
  value        = var.environment == "dev" ? "Debug" : "Information"
  type         = "kv"
  content_type = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "logging"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "api_logging_aspnetcore_level" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Logging:LogLevel:Microsoft.AspNetCore"
  label                  = var.environment
  value                  = "Warning"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "logging"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Grafana Loki Configuration (push endpoint path)
resource "azurerm_app_configuration_key" "api_loki_push_endpoint" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Logging:GrafanaLoki:PushEndpoint"
  label                  = var.environment
  value                  = "/loki/api/v1/push"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "logging"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "api_loki_batch_posting_limit" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Logging:GrafanaLoki:BatchPostingLimit"
  label                  = var.environment
  value                  = "100"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "logging"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# ASP.NET Core Environment
resource "azurerm_app_configuration_key" "aspnetcore_environment" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ASPNETCORE_ENVIRONMENT"
  label                  = var.environment
  value                  = var.environment == "dev" ? "Development" : "Production"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "logging"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# ============================================================================
# CONFIGURATION KEYS - REACT/FRONTEND SETTINGS
# ============================================================================

# API Base URL for React app
resource "azurerm_app_configuration_key" "react_api_base_url" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:APP_API_BASE_URL"
  label                  = var.environment
  value                  = "https://${local.api.service_name}.azurewebsites.net"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "React"
    category    = "feature-flags"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# NOTE: React:Environment removed - this is a build-time variable set via Docker build args,
# not a runtime configuration. See react-apps.yml workflow for build-arg configuration.

# Debug Mode Feature Flag
resource "azurerm_app_configuration_key" "react_debug_mode" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:APP_ENABLE_DEBUG_MODE"
  label                  = var.environment
  value                  = tostring(local.react.enable_debug_mode[var.environment])
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "React"
    category    = "feature-flags"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Mock Services Feature Flag
resource "azurerm_app_configuration_key" "react_use_mock_services" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:APP_USE_MOCK_SERVICES"
  label                  = var.environment
  value                  = tostring(local.react.use_mock_services[var.environment])
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "React"
    category    = "feature-flags"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Remote Logging Feature Flag
resource "azurerm_app_configuration_key" "react_remote_logging" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:APP_ENABLE_REMOTE_LOGGING"
  label                  = var.environment
  value                  = tostring(local.react.enable_remote_logging[var.environment])
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "React"
    category    = "feature-flags"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Individual Mock Service Flags (all disabled)
resource "azurerm_app_configuration_key" "react_mock_auth_service" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:APP_MOCK_AUTH_SERVICE"
  label                  = var.environment
  value                  = "false"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "React"
    category    = "feature-flags"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "react_mock_employees_service" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:APP_MOCK_EMPLOYEES_SERVICE"
  label                  = var.environment
  value                  = "false"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "React"
    category    = "feature-flags"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "react_mock_daily_log_service" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:APP_MOCK_DAILY_LOG_SERVICE"
  label                  = var.environment
  value                  = "false"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "React"
    category    = "feature-flags"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "react_mock_app_settings_service" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:APP_MOCK_APP_SETTINGS_SERVICE"
  label                  = var.environment
  value                  = "false"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "React"
    category    = "feature-flags"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# External Login Mock (enabled in dev for testing OAuth flows)
resource "azurerm_app_configuration_key" "react_external_login_mock" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "React:APP_ENABLE_EXTERNAL_LOGIN_MOCK"
  label                  = var.environment
  value                  = tostring(local.react.enable_external_login_mock[var.environment])
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "React"
    category    = "feature-flags"
  }

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
  key                    = "ConsilientApi:ApplicationSettings:Authentication:UserService:Jwt:Secret"
  label                  = var.environment
  type                   = "vault" # Special type for Key Vault references
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/jwt-signing-secret"

  tags = {
    application = "ConsilientApi"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.jwt_signing_secret
  ]
}

# SQL Connection String - Main Database Reference
resource "azurerm_app_configuration_key" "secrets_sql_connection_main" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ConnectionStrings:DefaultConnection"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/sql-connection-string-main"

  tags = {
    application = "ConsilientApi"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.sql_connection_main
  ]
}

# SQL Connection String - Hangfire Database Reference
resource "azurerm_app_configuration_key" "secrets_sql_connection_hangfire" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ConnectionStrings:HangfireConnection"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/sql-connection-string-hangfire"

  tags = {
    application = "ConsilientApi"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.sql_connection_hangfire
  ]
}

# Grafana Loki URL Reference
resource "azurerm_app_configuration_key" "secrets_loki_url" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Logging:GrafanaLoki:Url"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/grafana-loki-url"

  tags = {
    application = "ConsilientApi"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.grafana_loki_url
  ]
}

# ============================================================================
# CONFIGURATION KEYS - OAUTH SETTINGS
# ============================================================================
# OAuth configuration is conditionally created based on var.oauth_enabled
# When enabled, creates Azure AD App Registration and stores credentials in Key Vault

# OAuth Enabled Flag
resource "azurerm_app_configuration_key" "oauth_enabled" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ApplicationSettings:Authentication:UserService:OAuth:Enabled"
  label                  = var.environment
  value                  = var.oauth_enabled ? "true" : "false"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Provider Name
resource "azurerm_app_configuration_key" "oauth_provider_name" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ApplicationSettings:Authentication:UserService:OAuth:ProviderName"
  label                  = var.environment
  value                  = "Microsoft"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Authority (Microsoft login URL)
resource "azurerm_app_configuration_key" "oauth_authority" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ApplicationSettings:Authentication:UserService:OAuth:Authority"
  label                  = var.environment
  value                  = "https://login.microsoftonline.com/${data.azurerm_client_config.current.tenant_id}"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Client ID (from Azure AD App Registration, or placeholder if not configured)
resource "azurerm_app_configuration_key" "oauth_client_id" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ApplicationSettings:Authentication:UserService:OAuth:ClientId"
  label                  = var.environment
  value                  = var.oauth_enabled ? azuread_application.oauth[0].client_id : "not-configured"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Tenant ID
resource "azurerm_app_configuration_key" "oauth_tenant_id" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ApplicationSettings:Authentication:UserService:OAuth:TenantId"
  label                  = var.environment
  value                  = data.azurerm_client_config.current.tenant_id
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Scopes (comma-separated string for .NET to parse)
resource "azurerm_app_configuration_key" "oauth_scopes" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ApplicationSettings:Authentication:UserService:OAuth:Scopes"
  label                  = var.environment
  value                  = "openid profile email"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Client Secret (Key Vault reference)
resource "azurerm_app_configuration_key" "oauth_client_secret" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ApplicationSettings:Authentication:UserService:OAuth:ClientSecret"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/oauth-client-secret"

  tags = {
    application = "ConsilientApi"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.oauth_client_secret
  ]
}

# ============================================================================
# CONFIGURATION KEYS - CORS SETTINGS
# ============================================================================
# AllowedOrigins for CORS - uses dynamically generated React app hostname
# This ensures CORS configuration matches the actual deployed React app URL

resource "azurerm_app_configuration_key" "allowed_origins" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:AllowedOrigins:0"
  label                  = var.environment
  value                  = "https://${local.react.service_name}.azurewebsites.net"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "cors"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}
