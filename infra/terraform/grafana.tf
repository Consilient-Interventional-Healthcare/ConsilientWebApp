# Azure Managed Grafana

resource "azurerm_dashboard_grafana" "main" {
  name                = "grafana-${var.environment}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  grafana_major_version = var.grafana_major_version
  identity {
    type = "SystemAssigned"
  }
  api_key_enabled = true
  # Network integration to subnet for private access
  public_network_access_enabled = false
  # Add more configuration as needed
  tags                = local.tags
}
