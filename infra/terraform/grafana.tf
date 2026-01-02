# Azure Managed Grafana

# Note: Microsoft.Dashboard provider must be registered in your subscription
# You can register it manually via: az provider register --namespace Microsoft.Dashboard

resource "azurerm_dashboard_grafana" "main" {
  name                  = local.grafana.name
  resource_group_name   = azurerm_resource_group.main.name
  location              = azurerm_resource_group.main.location
  grafana_major_version = var.grafana_major_version
  identity {
    type = "SystemAssigned"
  }
  api_key_enabled = true
  # Network integration to subnet for private access
  public_network_access_enabled = false
  # Add more configuration as needed
  tags = local.tags
}
