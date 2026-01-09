# Azure Managed Grafana

# Note: Microsoft.Dashboard provider must be registered in your subscription
# You can register it manually via: az provider register --namespace Microsoft.Dashboard

# Grafana is optional - controlled by var.grafana_enabled
# For cost savings, deploy Grafana only in prod and share across environments
# by adding dev Loki as a datasource in the prod Grafana instance

resource "azurerm_dashboard_grafana" "main" {
  count                 = var.grafana_enabled ? 1 : 0
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

# Role assignments for Grafana admins have been moved to permissions.tf
# See: infra/terraform/permissions.tf

# --------------------------------------------------------------------------
# GRAFANA DATASOURCES
# --------------------------------------------------------------------------
# Azure Managed Grafana datasources must be configured via the Grafana API
# We use a null_resource with local-exec to configure datasources via Azure CLI

resource "null_resource" "grafana_loki_datasource" {
  count = var.grafana_enabled ? 1 : 0

  # Re-run if Grafana or Loki changes
  triggers = {
    grafana_id    = azurerm_dashboard_grafana.main[0].id
    loki_fqdn     = azurerm_container_app.loki.ingress[0].fqdn
    loki_username = var.loki_basic_auth_username
    # Trigger on password change (using hash to avoid logging sensitive value)
    loki_password_hash = sha256(local.loki_basic_auth_password)
  }

  provisioner "local-exec" {
    interpreter = ["bash", "-c"]
    environment = {
      LOKI_PASSWORD = local.loki_basic_auth_password
    }
    command = <<-EOT
      # Configure Loki datasource in Azure Managed Grafana with Basic Auth
      # Using az grafana data-source create command

      az grafana data-source create \
        --name "${azurerm_dashboard_grafana.main[0].name}" \
        --resource-group "${azurerm_resource_group.main.name}" \
        --definition '{
          "name": "Loki",
          "type": "loki",
          "access": "proxy",
          "url": "https://${azurerm_container_app.loki.ingress[0].fqdn}",
          "isDefault": true,
          "basicAuth": true,
          "basicAuthUser": "${var.loki_basic_auth_username}",
          "secureJsonData": {
            "basicAuthPassword": "'"$LOKI_PASSWORD"'"
          },
          "jsonData": {
            "maxLines": 1000,
            "timeout": "60"
          }
        }' || echo "Datasource may already exist, attempting update..."

      # If create fails (datasource exists), try to update
      az grafana data-source update \
        --name "${azurerm_dashboard_grafana.main[0].name}" \
        --resource-group "${azurerm_resource_group.main.name}" \
        --data-source "Loki" \
        --definition '{
          "name": "Loki",
          "type": "loki",
          "access": "proxy",
          "url": "https://${azurerm_container_app.loki.ingress[0].fqdn}",
          "isDefault": true,
          "basicAuth": true,
          "basicAuthUser": "${var.loki_basic_auth_username}",
          "secureJsonData": {
            "basicAuthPassword": "'"$LOKI_PASSWORD"'"
          },
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
