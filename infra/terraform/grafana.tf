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

# --------------------------------------------------------------------------
# GRAFANA DATASOURCES
# --------------------------------------------------------------------------
# Azure Managed Grafana datasources must be configured via the Grafana API
# We use a null_resource with local-exec to configure datasources via Azure CLI

resource "null_resource" "grafana_loki_datasource" {
  # Re-run if Grafana or Loki changes
  triggers = {
    grafana_id = azurerm_dashboard_grafana.main.id
    loki_fqdn  = azurerm_container_app.loki.ingress[0].fqdn
  }

  provisioner "local-exec" {
    interpreter = ["bash", "-c"]
    command     = <<-EOT
      # Configure Loki datasource in Azure Managed Grafana
      # Using az grafana data-source create command

      az grafana data-source create \
        --name "${azurerm_dashboard_grafana.main.name}" \
        --resource-group "${azurerm_resource_group.main.name}" \
        --definition '{
          "name": "Loki",
          "type": "loki",
          "access": "proxy",
          "url": "https://${azurerm_container_app.loki.ingress[0].fqdn}",
          "isDefault": true,
          "jsonData": {
            "maxLines": 1000,
            "timeout": "60"
          }
        }' || echo "Datasource may already exist, attempting update..."

      # If create fails (datasource exists), try to update
      az grafana data-source update \
        --name "${azurerm_dashboard_grafana.main.name}" \
        --resource-group "${azurerm_resource_group.main.name}" \
        --data-source "Loki" \
        --definition '{
          "name": "Loki",
          "type": "loki",
          "access": "proxy",
          "url": "https://${azurerm_container_app.loki.ingress[0].fqdn}",
          "isDefault": true,
          "jsonData": {
            "maxLines": 1000,
            "timeout": "60"
          }
        }' 2>/dev/null || true
    EOT
  }

  depends_on = [
    azurerm_dashboard_grafana.main,
    azurerm_container_app.loki
  ]
}
