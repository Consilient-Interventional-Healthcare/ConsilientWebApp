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
  key                    = "ConsilientApi:Authentication:Enabled"
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
  key                    = "ConsilientApi:Authentication:UserService:Jwt:Issuer"
  label                  = var.environment
  value                  = local.shared_config.jwt_issuer
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
  key                    = "ConsilientApi:Authentication:UserService:Jwt:Audience"
  label                  = var.environment
  value                  = local.shared_config.jwt_audience
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
  key                    = "ConsilientApi:Authentication:UserService:Jwt:ExpiryMinutes"
  label                  = var.environment
  value                  = local.shared_config.jwt_expiry_minutes
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Password Policy Configuration
resource "azurerm_app_configuration_key" "api_password_policy_require_digit" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Authentication:PasswordPolicy:RequireDigit"
  label                  = var.environment
  value                  = "true"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  lifecycle {
    ignore_changes = [value]
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "api_password_policy_required_length" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Authentication:PasswordPolicy:RequiredLength"
  label                  = var.environment
  value                  = "8"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  lifecycle {
    ignore_changes = [value]
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "api_password_policy_require_non_alphanumeric" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Authentication:PasswordPolicy:RequireNonAlphanumeric"
  label                  = var.environment
  value                  = "false"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  lifecycle {
    ignore_changes = [value]
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "api_password_policy_require_uppercase" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Authentication:PasswordPolicy:RequireUppercase"
  label                  = var.environment
  value                  = "true"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  lifecycle {
    ignore_changes = [value]
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "api_password_policy_require_lowercase" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Authentication:PasswordPolicy:RequireLowercase"
  label                  = var.environment
  value                  = "true"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  lifecycle {
    ignore_changes = [value]
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

resource "azurerm_app_configuration_key" "api_password_policy_required_unique_chars" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Authentication:PasswordPolicy:RequiredUniqueChars"
  label                  = var.environment
  value                  = "1"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  lifecycle {
    ignore_changes = [value]
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# API Logging Configuration
resource "azurerm_app_configuration_key" "api_logging_default_level" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Logging:LogLevel:Default"
  label                  = var.environment
  # Debug in dev for detailed logs; Information in prod
  # Note: Trace level causes crashes due to excessive log volume with Loki sink
  value        = var.environment == "dev" ? "Debug" : "Information"
  type         = "kv"
  content_type = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "logging"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# MicrosoftAspNetCore Log Level
resource "azurerm_app_configuration_key" "api_logging_microsoft_aspnetcore" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Logging:LogLevel:Microsoft.AspNetCore"
  label                  = var.environment
  value                  = local.shared_config.loglevel_microsoft_aspnetcore
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
  value                  = local.shared_config.loki_push_endpoint
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
  value                  = local.shared_config.loki_batch_posting_limit
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
  key                    = "ConsilientApi:Authentication:UserService:Jwt:Secret"
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

# Grafana Loki Basic Auth Username Reference
resource "azurerm_app_configuration_key" "secrets_loki_username" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Logging:GrafanaLoki:Username"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/loki-basic-auth-username"

  tags = {
    application = "ConsilientApi"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.loki_basic_auth_username
  ]
}

# Grafana Loki Basic Auth Password Reference
resource "azurerm_app_configuration_key" "secrets_loki_password" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Logging:GrafanaLoki:Password"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/loki-basic-auth-password"

  tags = {
    application = "ConsilientApi"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.loki_basic_auth_password
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
  key                    = "ConsilientApi:Authentication:UserService:OAuth:Enabled"
  label                  = var.environment
  value                  = var.oauth_enabled ? "true" : "false"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  lifecycle {
    ignore_changes = [value]
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Provider Name
resource "azurerm_app_configuration_key" "oauth_provider_name" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Authentication:UserService:OAuth:ProviderName"
  label                  = var.environment
  value                  = local.shared_config.oauth_provider_name
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Authority (Microsoft login URL - base URL only, TenantId is separate)
resource "azurerm_app_configuration_key" "oauth_authority" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Authentication:UserService:OAuth:Authority"
  label                  = var.environment
  value                  = "${local.shared_config.oauth_authority}/"
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
  key                    = "ConsilientApi:Authentication:UserService:OAuth:ClientId"
  label                  = var.environment
  value                  = local.shared_config.oauth_client_id
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
  key                    = "ConsilientApi:Authentication:UserService:OAuth:TenantId"
  label                  = var.environment
  value                  = local.shared_config.oauth_tenant_id
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
  key                    = "ConsilientApi:Authentication:UserService:OAuth:Scopes"
  label                  = var.environment
  value                  = local.shared_config.oauth_scopes
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Cookie expiry time for authentication tokens (in minutes)
# Controls how long the auth_token cookie is valid
resource "azurerm_app_configuration_key" "auth_cookie_expiry" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Authentication:CookieExpiryMinutes"
  label                  = var.environment
  value                  = "60" # 1 hour
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  lifecycle {
    ignore_changes = [value]
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Auto-provision users on OAuth login
# When enabled, new users are automatically created when they authenticate via Microsoft OAuth
# (provided their email domain is in AllowedEmailDomains)
resource "azurerm_app_configuration_key" "user_auto_provision" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Authentication:UserService:AutoProvisionUser"
  label                  = var.environment
  value                  = var.environment == "dev" ? "true" : "false"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "authentication"
  }

  lifecycle {
    ignore_changes = [value]
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Allowed Email Domains for user authentication
# Restricts which email domains can authenticate via OAuth
resource "azurerm_app_configuration_key" "api_allowed_email_domains" {
  for_each = { for idx, domain in ["consilientivh.com"] : idx => domain }

  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:Authentication:UserService:AllowedEmailDomains:${each.key}"
  label                  = var.environment
  value                  = each.value
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
  key                    = "ConsilientApi:Authentication:UserService:OAuth:ClientSecret"
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

# ============================================================================
# SHARED CONFIGURATION KEYS (Application-Agnostic)
# ============================================================================
# These keys store values that are shared across multiple applications.
# App-specific keys reference these shared keys instead of duplicating values.
# Prefix: Shared: (not tied to any specific application)

# File Storage Provider (Local or AzureBlob)
resource "azurerm_app_configuration_key" "shared_filestorage_provider" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Shared:FileStorage:Provider"
  label                  = var.environment
  value                  = local.shared_config.filestorage_provider
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "Shared"
    category    = "storage"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# File Storage Container Name
resource "azurerm_app_configuration_key" "shared_filestorage_container" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Shared:FileStorage:ContainerName"
  label                  = var.environment
  value                  = local.uploads_storage.container_name
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "Shared"
    category    = "storage"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Shared MicrosoftAspNetCore Log Level
resource "azurerm_app_configuration_key" "shared_logging_microsoft_aspnetcore" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Shared:Logging:LogLevel:Microsoft.AspNetCore"
  label                  = var.environment
  value                  = local.shared_config.loglevel_microsoft_aspnetcore
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "Shared"
    category    = "logging"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Shared JWT Expiry Minutes
resource "azurerm_app_configuration_key" "shared_jwt_expiry" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Shared:Authentication:UserService:Jwt:ExpiryMinutes"
  label                  = var.environment
  value                  = local.shared_config.jwt_expiry_minutes
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "Shared"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# File Storage Connection String (Key Vault Reference - Shared)
resource "azurerm_app_configuration_key" "shared_filestorage_connection" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "Shared:FileStorage:AzureBlobConnectionString"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/uploads-storage-connection-string"

  tags = {
    application = "Shared"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.uploads_storage_connection
  ]
}

# ============================================================================
# CONFIGURATION KEYS - FILE STORAGE (ConsilientApi)
# ============================================================================
# These keys reference the shared FileStorage configuration
# The API will load these and resolve the Shared: references at runtime

# ConsilientApi FileStorage Provider - references Shared
resource "azurerm_app_configuration_key" "api_filestorage_provider" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:FileStorage:Provider"
  label                  = var.environment
  value                  = local.shared_config.filestorage_provider
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "storage"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# ConsilientApi FileStorage ContainerName
resource "azurerm_app_configuration_key" "api_filestorage_container" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:FileStorage:ContainerName"
  label                  = var.environment
  value                  = local.uploads_storage.container_name
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "storage"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# ConsilientApi FileStorage Connection String (Key Vault Reference)
resource "azurerm_app_configuration_key" "api_filestorage_connection" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:FileStorage:AzureBlobConnectionString"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/uploads-storage-connection-string"

  tags = {
    application = "ConsilientApi"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.uploads_storage_connection
  ]
}

# ============================================================================
# CONFIGURATION KEYS - PROVIDER ASSIGNMENTS UPLOADS
# ============================================================================
# Settings for file upload validation (allowed extensions, max size, etc.)
# Binds to ProviderAssignmentsUploadsOptions in Consilient.Api

# Upload Path - directory/prefix for storing uploaded files
resource "azurerm_app_configuration_key" "api_file_upload_path" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ProviderAssignmentsUploads:UploadPath"
  label                  = var.environment
  value                  = local.file_upload.upload_path
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "storage"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Max File Size in Bytes
resource "azurerm_app_configuration_key" "api_file_upload_max_size" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ProviderAssignmentsUploads:MaxFileSizeBytes"
  label                  = var.environment
  value                  = tostring(local.file_upload.max_file_size_bytes)
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "storage"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Allowed Extensions - array of file extensions
# .NET Configuration binds arrays using indexed keys (AllowedExtensions:0, AllowedExtensions:1, etc.)
resource "azurerm_app_configuration_key" "api_file_upload_extensions" {
  for_each = { for idx, ext in local.file_upload.allowed_extensions : idx => ext }

  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:ProviderAssignmentsUploads:AllowedExtensions:${each.key}"
  label                  = var.environment
  value                  = each.value
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "storage"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# ============================================================================
# CONFIGURATION KEYS - FILE STORAGE (BackgroundHost)
# ============================================================================
# BackgroundHost uses same configuration prefix pattern

# BackgroundHost FileStorage Provider
resource "azurerm_app_configuration_key" "bghost_filestorage_provider" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:FileStorage:Provider"
  label                  = var.environment
  value                  = local.shared_config.filestorage_provider
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "storage"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# BackgroundHost FileStorage ContainerName
resource "azurerm_app_configuration_key" "bghost_filestorage_container" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:FileStorage:ContainerName"
  label                  = var.environment
  value                  = local.uploads_storage.container_name
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "storage"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# BackgroundHost FileStorage Connection String (Key Vault Reference)
resource "azurerm_app_configuration_key" "bghost_filestorage_connection" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:FileStorage:AzureBlobConnectionString"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/uploads-storage-connection-string"

  tags = {
    application = "BackgroundHost"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.uploads_storage_connection
  ]
}

# ============================================================================
# CONFIGURATION KEYS - CONNECTION STRINGS (BackgroundHost)
# ============================================================================
# BackgroundHost connection strings referencing same Key Vault secrets as API

# BackgroundHost Main Database Connection String (Key Vault Reference)
resource "azurerm_app_configuration_key" "bghost_connection_default" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:ConnectionStrings:DefaultConnection"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/sql-connection-string-main"

  tags = {
    application = "BackgroundHost"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.sql_connection_main
  ]
}

# BackgroundHost Hangfire Database Connection String (Key Vault Reference)
resource "azurerm_app_configuration_key" "bghost_connection_hangfire" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:ConnectionStrings:HangfireConnection"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/sql-connection-string-hangfire"

  tags = {
    application = "BackgroundHost"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.sql_connection_hangfire
  ]
}

# ============================================================================
# CONFIGURATION KEYS - OAUTH (BackgroundHost)
# ============================================================================
# BackgroundHost OAuth settings - always enabled (uses shared OAuth app with API)

# OAuth Enabled (always true for BackgroundHost)
resource "azurerm_app_configuration_key" "bghost_oauth_enabled" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Authentication:UserService:OAuth:Enabled"
  label                  = var.environment
  value                  = "true"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Provider Name
resource "azurerm_app_configuration_key" "bghost_oauth_provider_name" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Authentication:UserService:OAuth:ProviderName"
  label                  = var.environment
  value                  = local.shared_config.oauth_provider_name
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Authority (Microsoft login URL - base URL only, TenantId is separate)
resource "azurerm_app_configuration_key" "bghost_oauth_authority" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Authentication:UserService:OAuth:Authority"
  label                  = var.environment
  value                  = "${local.shared_config.oauth_authority}/"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Client ID (BackgroundHost has its own App Registration)
resource "azurerm_app_configuration_key" "bghost_oauth_client_id" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Authentication:UserService:OAuth:ClientId"
  label                  = var.environment
  value                  = local.shared_config.oauth_client_id_bghost
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Tenant ID
resource "azurerm_app_configuration_key" "bghost_oauth_tenant_id" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Authentication:UserService:OAuth:TenantId"
  label                  = var.environment
  value                  = local.shared_config.oauth_tenant_id
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Scopes
resource "azurerm_app_configuration_key" "bghost_oauth_scopes" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Authentication:UserService:OAuth:Scopes"
  label                  = var.environment
  value                  = local.shared_config.oauth_scopes
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "authentication"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# OAuth Client Secret (Key Vault reference - same as API)
resource "azurerm_app_configuration_key" "bghost_oauth_client_secret" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Authentication:UserService:OAuth:ClientSecret"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/oauth-client-secret"

  tags = {
    application = "BackgroundHost"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.oauth_client_secret
  ]
}

# ============================================================================
# CONFIGURATION KEYS - LOGGING (BackgroundHost)
# ============================================================================
# BackgroundHost logging configuration for Grafana Loki

# Logging Level
resource "azurerm_app_configuration_key" "bghost_logging_level" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Logging:LogLevel:Default"
  label                  = var.environment
  value                  = var.environment == "dev" ? "Debug" : "Information"
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "logging"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# MicrosoftAspNetCore Log Level
resource "azurerm_app_configuration_key" "bghost_logging_microsoft_aspnetcore" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Logging:LogLevel:Microsoft.AspNetCore"
  label                  = var.environment
  value                  = local.shared_config.loglevel_microsoft_aspnetcore
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "logging"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Loki URL (Key Vault Reference)
resource "azurerm_app_configuration_key" "bghost_loki_url" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Logging:GrafanaLoki:Url"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/grafana-loki-url"

  tags = {
    application = "BackgroundHost"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.grafana_loki_url
  ]
}

# Loki Push Endpoint
resource "azurerm_app_configuration_key" "bghost_loki_endpoint" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Logging:GrafanaLoki:PushEndpoint"
  label                  = var.environment
  value                  = local.shared_config.loki_push_endpoint
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "logging"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Loki Batch Posting Limit
resource "azurerm_app_configuration_key" "bghost_loki_batch_limit" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Logging:GrafanaLoki:BatchPostingLimit"
  label                  = var.environment
  value                  = local.shared_config.loki_batch_posting_limit
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "BackgroundHost"
    category    = "logging"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]
}

# Loki Username (Key Vault Reference)
resource "azurerm_app_configuration_key" "bghost_loki_username" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Logging:GrafanaLoki:Username"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/loki-basic-auth-username"

  tags = {
    application = "BackgroundHost"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.loki_basic_auth_username
  ]
}

# Loki Password (Key Vault Reference)
resource "azurerm_app_configuration_key" "bghost_loki_password" {
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "BackgroundHost:Logging:GrafanaLoki:Password"
  label                  = var.environment
  type                   = "vault"
  vault_key_reference    = "https://${azurerm_key_vault.main.name}.vault.azure.net/secrets/loki-basic-auth-password"

  tags = {
    application = "BackgroundHost"
    category    = "secrets"
  }

  depends_on = [
    azurerm_role_assignment.terraform_appconfig_owner,
    azurerm_key_vault_secret.loki_basic_auth_password
  ]
}

# ============================================================================
# SENTINEL KEY FOR CONFIGURATION REFRESH (Production Only)
# ============================================================================

# Sentinel key for triggering configuration refresh
# Only created for production - dev uses stop/start for config changes
# Update this key's value to force all connected applications to refresh their config
resource "azurerm_app_configuration_key" "refresh_sentinel" {
  count                  = var.environment == "prod" ? 1 : 0
  configuration_store_id = azurerm_app_configuration.main.id
  key                    = "ConsilientApi:RefreshSentinel"
  label                  = var.environment
  value                  = formatdate("YYYY-MM-DD'T'hh:mm:ssZ", timestamp())
  type                   = "kv"
  content_type           = "text/plain"

  tags = {
    application = "ConsilientApi"
    category    = "infrastructure"
    purpose     = "config-refresh-trigger"
  }

  depends_on = [azurerm_role_assignment.terraform_appconfig_owner]

  lifecycle {
    ignore_changes = [value] # Don't update on every apply, only when explicitly changed
  }
}
