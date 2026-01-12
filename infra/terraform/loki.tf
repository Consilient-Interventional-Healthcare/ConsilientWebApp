# Azure Container Apps Environment and Loki Deployment

# Generate a strong password for Loki Basic Auth if not provided
resource "random_password" "loki_basic_auth" {
  length           = 32
  special          = true
  override_special = "!@#$%^&*"
}

locals {
  # Use provided password or fall back to generated one
  loki_basic_auth_password = var.loki_basic_auth_password != "" ? var.loki_basic_auth_password : random_password.loki_basic_auth.result

  # Generate bcrypt hash for htpasswd (nginx supports bcrypt with $2y$ prefix)
  # Note: bcrypt() generates a different hash each run due to random salt.
  # The container_app lifecycle ignores secret changes to prevent recreation.
  # To update the password: terraform taint azurerm_container_app.loki
  loki_htpasswd_hash = bcrypt(local.loki_basic_auth_password)
}

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

  # Nginx configuration for Loki reverse proxy with basic auth
  loki_nginx_config = <<-EOF
    worker_processes 1;
    events { worker_connections 1024; }
    http {
      server {
        listen 8080;

        # Health check endpoint - no auth required
        location /ready {
          proxy_pass http://localhost:3100/ready;
          proxy_set_header Host $host;
        }

        # All other endpoints require basic auth
        location / {
          auth_basic "Loki";
          auth_basic_user_file /etc/nginx/.htpasswd;

          proxy_pass http://localhost:3100;
          proxy_set_header Host $host;
          proxy_set_header X-Real-IP $remote_addr;
          proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
          proxy_set_header X-Forwarded-Proto $scheme;

          # WebSocket support for tail endpoint
          proxy_http_version 1.1;
          proxy_set_header Upgrade $http_upgrade;
          proxy_set_header Connection "upgrade";

          # Increase timeouts for long-running queries
          proxy_connect_timeout 300;
          proxy_send_timeout 300;
          proxy_read_timeout 300;
        }
      }
    }
  EOF
}

resource "azurerm_user_assigned_identity" "loki" {
  name                = local.loki.identity_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  tags                = local.tags
}

# Role assignment for Loki storage has been moved to permissions.tf
# See: infra/terraform/permissions.tf

resource "azurerm_container_app" "loki" {
  name                         = local.loki.container_app_name
  container_app_environment_id = local.container_app_env_id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"
  tags                         = local.tags

  lifecycle {
    ignore_changes = [
      workload_profile_name,
      # Ignore secret changes to prevent recreation on every apply
      # (bcrypt generates different hash each time due to random salt)
      # To update htpasswd: terraform taint azurerm_container_app.loki
      secret
    ]
  }

  secret {
    name  = "nginx-config"
    value = local.loki_nginx_config
  }

  secret {
    name  = "htpasswd"
    value = "${var.loki_basic_auth_username}:${local.loki_htpasswd_hash}"
  }

  template {
    min_replicas = 1
    max_replicas = 10

    # Init container to set up nginx config files
    init_container {
      name   = "init-nginx"
      image  = "busybox:1.36"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name        = "NGINX_CONFIG"
        secret_name = "nginx-config"
      }
      env {
        name        = "HTPASSWD"
        secret_name = "htpasswd"
      }

      command = ["/bin/sh", "-c"]
      args    = ["printf '%s' \"$NGINX_CONFIG\" > /etc/nginx/nginx.conf && printf '%s' \"$HTPASSWD\" > /etc/nginx/.htpasswd"]

      volume_mounts {
        name = "nginx-config-volume"
        path = "/etc/nginx"
      }
    }

    # Nginx sidecar for basic auth
    container {
      name   = "nginx"
      image  = "nginx:1.25-alpine"
      cpu    = 0.25
      memory = "0.5Gi"

      volume_mounts {
        name = "nginx-config-volume"
        path = "/etc/nginx"
      }

      liveness_probe {
        port             = 8080
        transport        = "HTTP"
        interval_seconds = 10
        timeout          = 1
        path             = "/ready"
      }
      readiness_probe {
        port             = 8080
        transport        = "HTTP"
        interval_seconds = 10
        timeout          = 1
        path             = "/ready"
      }
    }

    # Loki container
    container {
      name   = "loki"
      image  = "grafana/loki:2.9.3"
      cpu    = local.loki.resources[var.environment].cpu
      memory = local.loki.resources[var.environment].memory
      env {
        name  = "LOKI_RETENTION_PERIOD"
        value = local.loki.retention[var.environment]
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
    }

    volume {
      name         = "nginx-config-volume"
      storage_type = "EmptyDir"
    }
  }

  ingress {
    external_enabled = true
    target_port      = 8080 # Route through nginx instead of directly to Loki
    transport        = "auto"
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
}
