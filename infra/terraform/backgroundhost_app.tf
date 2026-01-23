# BackgroundHost App Service for Hangfire Dashboard and Background Job Processing
#
# Architecture Decision:
# - Dev: Share App Service Plan with API (cost optimization, zero extra cost)
# - Prod: Configurable via variable (default: separate plan for independent scaling)

# Conditional App Service Plan for BackgroundHost (only created when backgroundhost_separate_plan = true)
resource "azurerm_service_plan" "backgroundhost_plan" {
  count               = var.backgroundhost_separate_plan ? 1 : 0
  name                = local.backgroundhost.service_plan_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = local.backgroundhost.sku
  tags                = local.tags
}

# BackgroundHost App Service
resource "azurerm_linux_web_app" "backgroundhost" {
  name                = local.backgroundhost.service_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Use separate plan if enabled, otherwise share with API
  service_plan_id = var.backgroundhost_separate_plan ? azurerm_service_plan.backgroundhost_plan[0].id : module.api_app.app_service_plan_id

  https_only                = true
  virtual_network_subnet_id = azurerm_subnet.app_service.id

  identity {
    type = "SystemAssigned"
  }

  site_config {
    vnet_route_all_enabled = true

    # Managed identity authentication for ACR
    container_registry_use_managed_identity       = true
    container_registry_managed_identity_client_id = null

    # Health check configuration
    health_check_path                 = local.backgroundhost.health_check_path
    health_check_eviction_time_in_min = local.backgroundhost.health_check_eviction_time_in_min
  }

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    ASPNETCORE_ENVIRONMENT              = var.environment == "dev" ? "Development" : "Production"
    WEBSITES_PORT                       = "8092"

    # Azure App Configuration endpoint (primary source for runtime config)
    "AppConfiguration__Endpoint" = azurerm_app_configuration.main.endpoint

    # Legacy Key Vault configuration (fallback)
    "KeyVault__Url" = "https://${azurerm_key_vault.main.name}.vault.azure.net/"

    # JWT configuration for dashboard authentication (same as API)
    "ApplicationSettings__Authentication__UserService__Jwt__Secret"   = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=jwt-signing-secret)"
    "ApplicationSettings__Authentication__UserService__Jwt__Issuer"   = "https://${local.api.service_name}.azurewebsites.net"
    "ApplicationSettings__Authentication__UserService__Jwt__Audience" = "https://${local.api.service_name}.azurewebsites.net"

    # Dashboard auth setting
    "ApplicationSettings__Authentication__DashboardAuthEnabled" = var.backgroundhost_dashboard_auth_enabled ? "true" : "false"

    # Logging
    "Logging__GrafanaLoki__Url" = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=grafana-loki-url)"
  }

  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=sql-connection-string-main)"
  }

  connection_string {
    name  = "HangfireConnection"
    type  = "SQLAzure"
    value = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=sql-connection-string-hangfire)"
  }

  tags = local.tags

  depends_on = [azurerm_key_vault.main]
}

# Custom domain hostname binding (required before certificate can be issued)
resource "azurerm_app_service_custom_hostname_binding" "backgroundhost_custom_domain" {
  count               = var.backgroundhost_custom_domain != "" ? 1 : 0
  app_service_name    = azurerm_linux_web_app.backgroundhost.name
  resource_group_name = azurerm_resource_group.main.name
  hostname            = var.backgroundhost_custom_domain

  depends_on = [azurerm_linux_web_app.backgroundhost]
}

# Azure-managed SSL certificate for the custom domain
resource "azurerm_app_service_managed_certificate" "backgroundhost_custom_domain" {
  count                      = var.backgroundhost_custom_domain != "" ? 1 : 0
  custom_hostname_binding_id = azurerm_app_service_custom_hostname_binding.backgroundhost_custom_domain[0].id
}

# Bind the managed certificate to the custom domain
resource "azurerm_app_service_certificate_binding" "backgroundhost_custom_domain" {
  count               = var.backgroundhost_custom_domain != "" ? 1 : 0
  hostname_binding_id = azurerm_app_service_custom_hostname_binding.backgroundhost_custom_domain[0].id
  certificate_id      = azurerm_app_service_managed_certificate.backgroundhost_custom_domain[0].id
  ssl_state           = "SniEnabled"
}
