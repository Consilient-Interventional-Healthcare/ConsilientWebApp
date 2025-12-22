# Azure App Service Plan and React App Service (Docker, Public)

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
  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    # DOCKER_REGISTRY_SERVER_URL is managed automatically by Azure
    # DOCKER_REGISTRY_SERVER_USERNAME and DOCKER_REGISTRY_SERVER_PASSWORD should be injected securely via pipeline or Key Vault
  }
  tags     = local.tags
  sku_name = local.react.sku
}
