# Azure App Service Plan and React App Service (Docker, Public)

# React App Runtime Environment Variables
# ========================================
# The React app uses runtime environment injection (NOT build-time).
# Flow:
#   1. Terraform reads React:* configuration from App Configuration
#   2. Terraform transforms keys to APP_* format and sets app_settings
#   3. docker-entrypoint.sh reads APP_* env vars at container startup
#   4. Script generates /usr/share/nginx/html/env.js with window.__ENV
#   5. React app loads env.js and reads configuration
#
# Configuration Source:
#   - App Configuration: Single source of truth for all React configuration
#   - Terraform: Reads React:* keys from AAC, transforms to APP_* format
#
# API URL Construction:
#   - Uses local.api.service_name (handles multi-tier hostname naming)
#   - Format: https://{api_app_name}.azurewebsites.net
#   - Automatically matches the deployed API in same environment
#
# Environment-Specific Values:
#   - dev:  development, debug=true, mocks=false, logging=false
#   - prod: production, debug=false, mocks=false, logging=true

# Read React configuration from App Configuration
# Filters for keys starting with "React:" and matching environment label
data "azurerm_app_configuration_keys" "react_config" {
  configuration_store_id = azurerm_app_configuration.main.id
  label                  = var.environment
  key                    = "React:*"

  depends_on = [
    azurerm_app_configuration.main,
    azurerm_app_configuration_key.react_api_base_url,
    azurerm_app_configuration_key.react_environment,
    azurerm_app_configuration_key.react_debug_mode,
    azurerm_app_configuration_key.react_use_mock_services,
    azurerm_app_configuration_key.react_remote_logging,
    azurerm_app_configuration_key.react_mock_auth_service,
    azurerm_app_configuration_key.react_mock_employees_service,
    azurerm_app_configuration_key.react_mock_daily_log_service,
    azurerm_app_configuration_key.react_mock_app_settings_service,
    azurerm_app_configuration_key.react_external_login_mock
  ]
}

locals {
  # Transform App Configuration keys to APP_* format for React app
  # "React:ApiBaseUrl" → "APP_API_BASE_URL"
  # "React:EnableDebugMode" → "APP_ENABLE_DEBUG_MODE"
  react_app_settings_from_aac = {
    for item in data.azurerm_app_configuration_keys.react_config.items :
    replace(upper(replace(item.key, "React:", "APP_")), ":", "_") => item.value
  }
}

module "react_app" {
  source                 = "./modules/app_service"
  plan_name              = local.react.service_plan_name
  plan_tier              = local.react.sku
  plan_size              = local.react.sku
  app_name               = local.react.service_name
  location               = azurerm_resource_group.main.location
  resource_group_name    = azurerm_resource_group.main.name
  linux_fx_version       = "DOCKER|<acr-login-server>/<react-image>:<tag>"
  vnet_route_all_enabled = false

  # Enable managed identity for ACR authentication
  # Uses the system-assigned managed identity (client_id left empty)
  container_registry_use_managed_identity       = true
  container_registry_managed_identity_client_id = ""

  # HTTPS configuration
  enable_https_only  = true
  custom_domain_name = var.react_custom_domain

  app_settings = merge(
    {
      # Azure App Service configuration
      WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    },
    # Load all React configuration from App Configuration
    # Data source transforms React:* keys to APP_* format
    local.react_app_settings_from_aac
  )

  tags     = local.tags
  sku_name = local.react.sku

  depends_on = [
    azurerm_app_configuration.main,
    data.azurerm_app_configuration_keys.react_config
  ]
}

# Grant React App Service permission to pull images from ACR
resource "azurerm_role_assignment" "react_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = module.react_app.app_service_principal_id
}
