# Azure App Service Plan and API App Service (Docker)

module "api_app" {
  source                 = "./modules/app_service"
  plan_name              = local.api.service_plan_name
  plan_tier              = local.api.sku
  plan_size              = local.api.sku
  app_name               = local.api.service_name
  location               = azurerm_resource_group.main.location
  resource_group_name    = azurerm_resource_group.main.name
  linux_fx_version       = "DOCKER|<acr-login-server>/<api-image>:<tag>"
  vnet_route_all_enabled = true
  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    # DOCKER_REGISTRY_SERVER_URL is managed automatically by Azure
    # DOCKER_REGISTRY_SERVER_USERNAME and DOCKER_REGISTRY_SERVER_PASSWORD should be injected securely via pipeline or Key Vault
  }
  tags     = local.tags
  sku_name = local.api.sku
}

# Grant API App Service permission to pull images from ACR
resource "azurerm_role_assignment" "api_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = module.api_app.app_service_principal_id
}
