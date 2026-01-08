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
  # Network integration - toggle via var.grafana_public_network_access
  public_network_access_enabled = var.grafana_public_network_access
  # Add more configuration as needed
  tags = local.tags
}

# Grafana Admin role assignments
# Add users/groups who need admin access to Grafana
resource "azurerm_role_assignment" "grafana_admins" {
  for_each             = toset(var.grafana_admin_users)
  scope                = azurerm_dashboard_grafana.main.id
  role_definition_name = "Grafana Admin"
  principal_id         = each.value
}
