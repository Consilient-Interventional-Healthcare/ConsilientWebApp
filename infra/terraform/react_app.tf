# Azure App Service Plan and React App Service (Docker, Public)

module "react_app" {
  source                  = "./modules/app_service"
  plan_name               = "asp-react-${var.environment}"
  plan_tier               = var.react_appservice_tier
  plan_size               = var.react_appservice_size
  app_name                = "react-${var.environment}"
  location                = azurerm_resource_group.main.location
  resource_group_name     = azurerm_resource_group.main.name
  linux_fx_version        = "DOCKER|<acr-login-server>/<react-image>:<tag>"
  vnet_route_all_enabled  = false
  app_settings            = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    DOCKER_REGISTRY_SERVER_URL          = azurerm_container_registry.main.login_server
    # DOCKER_REGISTRY_SERVER_USERNAME and DOCKER_REGISTRY_SERVER_PASSWORD should be injected securely via pipeline or Key Vault
  }
  tags                    = local.tags
  sku_name                = "P1v2"
}
