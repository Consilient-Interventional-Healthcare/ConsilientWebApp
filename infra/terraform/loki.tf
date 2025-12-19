# Azure Container Apps Environment and Loki Deployment

resource "azurerm_container_app_environment" "shared" {
  name                = "cae-shared-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "loki" {
  name                = "loki-identity-${var.environment}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  tags                = local.tags
}

resource "azurerm_role_assignment" "loki_blob" {
  scope                = azurerm_storage_account.loki.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.loki.principal_id
}

resource "azurerm_container_app" "loki" {
  name                         = "loki-${var.environment}"
  container_app_environment_id = azurerm_container_app_environment.shared.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"
  tags                         = local.tags

  template {
    container {
      name   = "loki"
      image  = "grafana/loki:2.9.3"
      cpu    = var.loki_cpu_request
      memory = var.loki_memory_request
      env {
        name  = "LOKI_RETENTION_PERIOD"
        value = var.loki_retention
      }
      env {
        name  = "LOKI_STORAGE_TYPE"
        value = "azure"
      }
      env {
        name  = "AZURE_STORAGE_ACCOUNT"
        value = azurerm_storage_account.loki.name
      }
      env {
        name  = "AZURE_STORAGE_CONTAINER"
        value = azurerm_storage_container.loki.name
      }
      liveness_probe {
        port            = 3100
        transport       = "HTTP"
        interval_seconds = 10
        timeout         = 1
        path            = "/ready"
      }
      readiness_probe {
        port            = 3100
        transport       = "HTTP"
        interval_seconds = 10
        timeout         = 1
        path            = "/ready"
      }
      # Add more Loki config as needed
    }
  }

  ingress {
    external_enabled = false
    target_port      = 3100
    transport        = "auto"
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
}
