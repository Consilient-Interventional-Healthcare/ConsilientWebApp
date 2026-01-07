# Azure Container Apps Environment and Loki Deployment

# Option: Create a new Container App Environment (may hit quota limits)
# Set create_container_app_environment to false and provide existing_container_app_environment_id if quota is exceeded
resource "azurerm_container_app_environment" "shared" {
  count               = var.create_container_app_environment ? 1 : 0
  name                = local.loki.container_app_env_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tags                = local.tags

  lifecycle {
    ignore_changes = [
      workload_profile
    ]
  }
}

locals {
  # Priority: explicit ID > created resource
  container_app_env_id = (
    var.existing_container_app_environment_id != ""
    ? var.existing_container_app_environment_id
    : azurerm_container_app_environment.shared[0].id
  )
}

resource "azurerm_user_assigned_identity" "loki" {
  name                = local.loki.identity_name
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
  name                         = local.loki.container_app_name
  container_app_environment_id = local.container_app_env_id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"
  tags                         = local.tags

  lifecycle {
    ignore_changes = [
      workload_profile_name,
      template
    ]
  }

  template {
    min_replicas = 1
    max_replicas = 10

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
        port             = 3100
        transport        = "HTTP"
        interval_seconds = 10
        timeout          = 1
        path             = "/ready"
      }
      readiness_probe {
        port             = 3100
        transport        = "HTTP"
        interval_seconds = 10
        timeout          = 1
        path             = "/ready"
      }
      # Add more Loki config as needed
    }
  }

  ingress {
    external_enabled = true
    target_port      = 3100
    transport        = "auto"
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
}
