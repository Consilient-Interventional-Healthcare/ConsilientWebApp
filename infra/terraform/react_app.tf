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

    # Docker Registry configuration for ACR with Managed Identity
    # Setting DOCKER_REGISTRY_SERVER_URL without USERNAME/PASSWORD credentials
    # tells App Service to use the system-assigned managed identity for authentication
    "DOCKER_REGISTRY_SERVER_URL" = "https://${azurerm_container_registry.main.login_server}"
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
