# Azure App Service Plan and API App Service (Docker)

module "api_app" {
  source                  = "./modules/app_service"
  plan_name               = "asp-api-${var.environment}"
  plan_tier               = var.api_appservice_tier
  plan_size               = var.api_appservice_size
  app_name                = "api-${var.environment}"
  location                = azurerm_resource_group.main.location
  resource_group_name     = azurerm_resource_group.main.name
  linux_fx_version        = "DOCKER|<acr-login-server>/<api-image>:<tag>"
  vnet_route_all_enabled  = true
  app_settings            = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    DOCKER_REGISTRY_SERVER_URL          = azurerm_container_registry.main.login_server
    # DOCKER_REGISTRY_SERVER_USERNAME and DOCKER_REGISTRY_SERVER_PASSWORD should be injected securely via pipeline or Key Vault
  }
  tags                    = local.tags
  sku_name                = "P1v2"
}
