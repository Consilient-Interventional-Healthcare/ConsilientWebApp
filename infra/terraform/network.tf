# Resource Group
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.region
  tags     = local.tags
}

# Virtual Network and Subnet for internal services
resource "azurerm_virtual_network" "main" {
  name                = local.network.vnet.name
  address_space       = ["10.10.0.0/16"]
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tags                = local.tags
}

resource "azurerm_subnet" "main" {
  name                 = local.network.subnet.name
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.10.1.0/24"]
}
