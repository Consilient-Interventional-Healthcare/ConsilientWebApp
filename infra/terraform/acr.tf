# Azure Container Registry
resource "azurerm_container_registry" "main" {
  name                = local.acr.name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = local.acr.sku
  admin_enabled       = false
  tags                = local.tags
}
