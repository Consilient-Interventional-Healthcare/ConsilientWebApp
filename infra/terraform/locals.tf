# ============================================================================
# CENTRALIZED RESOURCE CONFIGURATION
# ============================================================================
# This file contains ALL resource configuration grouped by resource type.
# Each resource block contains:
# - Naming conventions
# - SKUs/tiers per environment
# - Resource-specific settings
#
# Edit this file to control names, costs, and settings for all resources.
# ============================================================================

locals {
  # --------------------------------------------------------------------------
  # UNIQUE IDENTIFIERS
  # --------------------------------------------------------------------------
  unique_suffix = substr(md5("${var.subscription_id}-${var.resource_group_name}"), 0, 6)

  # --------------------------------------------------------------------------
  # DEFAULT SKU OPTIONS PER ENVIRONMENT
  # --------------------------------------------------------------------------
  # These are the default SKU/tier options for each environment
  # Individual resources can override these defaults if needed
  default_skus = {
    # Development - Minimal cost
    dev = {
      app_service_plan   = "B1"      # Basic tier - ~$13/month
      container_registry = "Basic"   # ~$5/month
      sql_basic          = "Basic"   # Basic DTU - ~$5/month
      sql_serverless     = "GP_S_Gen5_2" # Serverless - ~$150/month (when active)
      sql_provisioned    = "GP_Gen5_2"   # General Purpose - ~$650/month
    }

    # Staging - Balanced cost/performance
    staging = {
      app_service_plan   = "P1v2"        # Premium v2 - ~$146/month
      container_registry = "Standard"    # ~$20/month
      sql_serverless     = "GP_S_Gen5_2" # Serverless - ~$150/month
      sql_provisioned    = "GP_Gen5_2"   # General Purpose - ~$650/month
    }

    # Production - High availability and performance
    prod = {
      app_service_plan   = "P2v3"      # Premium v3 - ~$204/month
      container_registry = "Premium"   # ~$40/month (geo-replication support)
      sql_serverless     = "GP_Gen5_2" # General Purpose - ~$650/month
      sql_provisioned    = "GP_Gen5_4" # General Purpose 4 vCores - ~$1,300/month
    }
  }

  # --------------------------------------------------------------------------
  # NETWORK RESOURCES
  # --------------------------------------------------------------------------
  network = {
    vnet = {
      name = "${var.project_name}-vnet-${var.environment}"
    }
    subnet = {
      name = "${var.project_name}-subnet-${var.environment}"
    }
  }

  # --------------------------------------------------------------------------
  # API APP SERVICE
  # --------------------------------------------------------------------------
  api = {
    service_plan_name = "${var.project_name}-asp-api-${var.environment}"
    service_name      = "${var.project_name}-api-${var.environment}"

    # Uses default_skus.app_service_plan for each environment
    # Override here if API needs different SKUs than default
    sku = local.default_skus[var.environment].app_service_plan

    # Health check configuration
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  # --------------------------------------------------------------------------
  # REACT APP SERVICE
  # --------------------------------------------------------------------------
  react = {
    service_plan_name = "${var.project_name}-asp-react-${var.environment}"
    service_name      = "${var.project_name}-react-${var.environment}"

    # Uses default_skus.app_service_plan for each environment
    # Override here if React needs different SKUs than default
    sku = local.default_skus[var.environment].app_service_plan
  }

  # --------------------------------------------------------------------------
  # CONTAINER REGISTRY
  # --------------------------------------------------------------------------
  acr = {
    name = "${var.project_name}acr${var.environment}${local.unique_suffix}"

    # Uses default_skus.container_registry for each environment
    sku = local.default_skus[var.environment].container_registry
  }

  # --------------------------------------------------------------------------
  # SQL SERVER & DATABASES
  # --------------------------------------------------------------------------
  sql = {
    server_name = "${var.project_name}-sqlsrv-${var.environment}-${local.unique_suffix}"

    # Main Application Database
    main_db = {
      name = "${var.project_name}_main_${var.environment}"

      # Uses default_skus.sql_basic for dev, sql_provisioned for prod
      sku = var.environment == "prod" ? local.default_skus[var.environment].sql_provisioned : (var.environment == "dev" ? local.default_skus[var.environment].sql_basic : local.default_skus[var.environment].sql_serverless)

      min_capacity = {
        dev     = null # Not serverless (Basic DTU)
        staging = null
        prod    = null
      }

      auto_pause_delay = {
        dev     = null # Not serverless (Basic DTU)
        staging = null # Not serverless
        prod    = null # Not serverless
      }

      zone_redundant = {
        dev     = false
        staging = false
        prod    = true # Zone redundant for HA
      }
    }

    # Hangfire Database
    hangfire_db = {
      name = "${var.project_name}_hangfire_${var.environment}"

      # Uses default_skus.sql_basic for dev, sql_serverless for staging, sql_provisioned for prod
      sku = var.environment == "prod" ? local.default_skus[var.environment].sql_provisioned : (var.environment == "dev" ? local.default_skus[var.environment].sql_basic : local.default_skus[var.environment].sql_serverless)

      min_capacity = {
        dev     = null # Not serverless (Basic DTU)
        staging = 0.5
        prod    = null
      }

      auto_pause_delay = {
        dev     = null # Not serverless (Basic DTU)
        staging = 120  # Auto-pause after 2 hours
        prod    = null # Not serverless
      }

      zone_redundant = {
        dev     = false
        staging = false
        prod    = false
      }
    }

    # Security Features
    threat_protection_enabled = {
      dev     = false
      staging = true
      prod    = true
    }

    auditing_enabled = {
      dev     = false
      staging = true
      prod    = true
    }

    audit_retention_days = {
      dev     = 0
      staging = 90
      prod    = 365 # 1 year retention for compliance
    }
  }

  # --------------------------------------------------------------------------
  # CONTAINER APPS & LOKI
  # --------------------------------------------------------------------------

  # Resolve Container App Environment name based on subscription tier
  # Free-tier (use_shared_container_environment = true): Uses fixed shared name
  # Paid-tier (use_shared_container_environment = false): Uses template with {environment} placeholder
  container_app_environment_name = var.use_shared_container_environment ? (
    var.shared_container_environment_name
  ) : (
    replace(
      var.container_app_environment_name_template,
      "{environment}",
      var.environment
    )
  )

  loki = {
    # Use resolved CAE name (handles both shared and template-based naming)
    container_app_env_name = local.container_app_environment_name
    container_app_name     = "${var.project_name}-loki-${var.environment}"
    identity_name          = "${var.project_name}-loki-identity-${var.environment}"

    storage = {
      account_name               = "${var.project_name}loki${var.environment}${local.unique_suffix}"
      container_name             = "loki-data"
      private_endpoint           = "${var.project_name}-pe-loki-storage-${var.environment}"
      private_service_connection = "${var.project_name}-psc-loki-storage-${var.environment}"
    }
  }

  # --------------------------------------------------------------------------
  # MONITORING - GRAFANA
  # --------------------------------------------------------------------------
  grafana = {
    name = "${var.project_name}-grafana-${var.environment}"
  }

  # --------------------------------------------------------------------------
  # COST ESTIMATES
  # --------------------------------------------------------------------------
  estimated_monthly_cost = {
    dev     = 45   # USD/month (reduced from 200 - Basic DTU SQL instead of Serverless Gen5)
    staging = 1200 # USD/month
    prod    = 2800 # USD/month
  }

  # --------------------------------------------------------------------------
  # TAGS
  # --------------------------------------------------------------------------
  tags = {
    environment = var.environment
    project     = var.project_name
    managed_by  = "terraform"
  }
}
