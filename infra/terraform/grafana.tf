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
    # Increment config_version to force re-run of datasource configuration
    config_version = "2"
  }

  provisioner "local-exec" {
    interpreter = ["bash", "-c"]
    environment = {
      LOKI_PASSWORD = local.loki_basic_auth_password
      LOKI_FQDN     = azurerm_container_app.loki.ingress[0].fqdn
    }
    command = <<-EOT
      set -e

      # Properly escape password for JSON using jq
      ESCAPED_PASSWORD=$(printf '%s' "$LOKI_PASSWORD" | jq -Rs .)

      # Wait for Loki to be ready before configuring datasource
      echo "Waiting for Loki to be ready..."
      for i in {1..30}; do
        if curl -sf "https://$LOKI_FQDN/ready" > /dev/null 2>&1; then
          echo "Loki is ready"
          break
        fi
        if [ $i -eq 30 ]; then
          echo "ERROR: Loki did not become ready in time"
          exit 1
        fi
        echo "Attempt $i: Loki not ready, waiting 5s..."
        sleep 5
      done

      # Build the datasource definition with properly escaped password
      DATASOURCE_DEF=$(cat <<EOF
      {
        "name": "Loki",
        "type": "loki",
        "access": "proxy",
        "url": "https://$LOKI_FQDN",
        "isDefault": true,
        "basicAuth": true,
        "basicAuthUser": "${var.loki_basic_auth_username}",
        "secureJsonData": {
          "basicAuthPassword": $ESCAPED_PASSWORD
        },
        "jsonData": {
          "maxLines": 1000,
          "timeout": "60"
        }
      }
      EOF
      )

      # Try to create datasource first
      echo "Creating Loki datasource in Grafana..."
      if az grafana data-source create \
        --name "${azurerm_dashboard_grafana.main[0].name}" \
        --resource-group "${azurerm_resource_group.main.name}" \
        --definition "$DATASOURCE_DEF" 2>&1; then
        echo "Datasource created successfully"
      else
        # If create fails (datasource exists), try to update
        echo "Datasource may already exist, attempting update..."
        if az grafana data-source update \
          --name "${azurerm_dashboard_grafana.main[0].name}" \
          --resource-group "${azurerm_resource_group.main.name}" \
          --data-source "Loki" \
          --definition "$DATASOURCE_DEF" 2>&1; then
          echo "Datasource updated successfully"
        else
          echo "ERROR: Failed to create or update datasource"
          exit 1
        fi
      fi

      # Verify datasource was configured with basic auth
      echo "Verifying datasource configuration..."
      BASIC_AUTH_ENABLED=$(az grafana data-source show \
        --name "${azurerm_dashboard_grafana.main[0].name}" \
        --resource-group "${azurerm_resource_group.main.name}" \
        --data-source "Loki" \
        --query "basicAuth" -o tsv 2>/dev/null || echo "false")

      if [ "$BASIC_AUTH_ENABLED" != "true" ]; then
        echo "WARNING: basicAuth may not be enabled on datasource. Please verify in Grafana UI."
      else
        echo "Datasource verified: basicAuth is enabled"
      fi
    EOT
  }

  depends_on = [
    azurerm_dashboard_grafana.main,
    azurerm_container_app.loki
  ]
}
