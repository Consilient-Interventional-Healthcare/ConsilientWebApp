# Azure App Service Plan and React App Service (Docker, Public)

# React App Runtime Environment Variables
# ========================================
# The React app uses runtime environment injection (NOT build-time).
# Flow:
#   1. Terraform sets app_settings with APP_* variables
#   2. docker-entrypoint.sh reads APP_* env vars at container startup
#   3. Script generates /usr/share/nginx/html/env.js with window.__ENV
#   4. React app loads env.js and reads configuration
#
# API URL Construction:
#   - Uses local.api.service_name (handles multi-tier hostname naming)
#   - Format: https://{api_app_name}.azurewebsites.net
#   - Automatically matches the deployed API in same environment
#
# Environment-Specific Values:
#   - dev:  development, debug=true, mocks=false, logging=false
#   - prod: production, debug=false, mocks=false, logging=true

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

  app_settings = {
    # Azure App Service configuration
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"

    # React Runtime Environment Variables (APP_* prefix)
    # These are read by docker-entrypoint.sh and exported to window.__ENV

    # API Configuration - Construct URL from API app service name
    APP_API_BASE_URL = "https://${local.api.service_name}.azurewebsites.net"

    # Environment Configuration
    APP_ENV = local.react.app_env[var.environment]

    # Feature Flags
    APP_ENABLE_DEBUG_MODE     = tostring(local.react.enable_debug_mode[var.environment])
    APP_USE_MOCK_SERVICES     = tostring(local.react.use_mock_services[var.environment])
    APP_ENABLE_REMOTE_LOGGING = tostring(local.react.enable_remote_logging[var.environment])

    # Individual Mock Service Controls (all disabled by default)
    APP_MOCK_AUTH_SERVICE         = "false"
    APP_MOCK_EMPLOYEES_SERVICE    = "false"
    APP_MOCK_DAILY_LOG_SERVICE    = "false"
    APP_MOCK_APP_SETTINGS_SERVICE = "false"

    # External Login Mock (enabled in dev for testing OAuth flows)
    APP_ENABLE_EXTERNAL_LOGIN_MOCK = tostring(local.react.enable_debug_mode[var.environment])
  }
  tags     = local.tags
  sku_name = local.react.sku
}

# Grant React App Service permission to pull images from ACR
resource "azurerm_role_assignment" "react_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = module.react_app.app_service_principal_id
}
