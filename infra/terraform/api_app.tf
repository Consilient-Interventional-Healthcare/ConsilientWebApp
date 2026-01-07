# Azure App Service Plan and API App Service (Docker)

module "api_app" {
  source                 = "./modules/app_service"
  plan_name              = local.api.service_plan_name
  plan_tier              = local.api.sku
  plan_size              = local.api.sku
  app_name               = local.api.service_name
  location               = azurerm_resource_group.main.location
  resource_group_name    = azurerm_resource_group.main.name
  linux_fx_version       = "DOCKER|<acr-login-server>/<api-image>:<tag>"
  vnet_route_all_enabled = true
  subnet_id              = azurerm_subnet.app_service.id

  # Enable managed identity for ACR authentication
  # Uses the system-assigned managed identity (client_id left empty)
  container_registry_use_managed_identity       = true
  container_registry_managed_identity_client_id = ""

  # HTTPS configuration
  enable_https_only  = true
  custom_domain_name = var.api_custom_domain

  # Health check configuration
  health_check_path                 = local.api.health_check_path
  health_check_eviction_time_in_min = local.api.health_check_eviction_time_in_min

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    ASPNETCORE_ENVIRONMENT              = "Production"

    # Azure App Configuration endpoint (NEW - primary source for runtime config)
    # If set, API uses this for configuration loading via managed identity
    "AppConfiguration__Endpoint" = azurerm_app_configuration.main.endpoint

    # Legacy Key Vault configuration (TEMPORARY - for backward compatibility during migration)
    # This is kept temporarily to allow fallback if AAC has issues
    # Will be removed after validation period in next phase
    "KeyVault__Url" = "https://${azurerm_key_vault.main.name}.vault.azure.net/"

    # Legacy Key Vault references for secrets (TEMPORARY - kept for backward compatibility)
    # These are resolved at runtime by App Service using the managed identity
    # Will be removed after migration is complete
    "ApplicationSettings__Authentication__UserService__Jwt__Secret" = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=jwt-signing-secret)"
    "Logging__GrafanaLoki__Url"                                     = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=grafana-loki-url)"

    # Non-secret configuration (DEPRECATED - now in App Configuration, kept for transition)
    # These will be removed from here in next phase as API loads from AAC
    "ApplicationSettings__Authentication__UserService__Jwt__Issuer"        = "https://${local.api.service_name}.azurewebsites.net"
    "ApplicationSettings__Authentication__UserService__Jwt__Audience"      = "https://${local.api.service_name}.azurewebsites.net"
    "ApplicationSettings__Authentication__UserService__Jwt__ExpiryMinutes" = "60"
    "ApplicationSettings__Authentication__Enabled"                         = "true"
    "Logging__GrafanaLoki__PushEndpoint"                                   = "/loki/api/v1/push"
    "Logging__GrafanaLoki__BatchPostingLimit"                              = "100"
    "Logging__LogLevel__Default"                                           = "Information"
    "Logging__LogLevel__Microsoft.AspNetCore"                              = "Warning"
  }

  # Connection strings must be separate from app_settings
  # Azure App Service treats these specially
  # DEPRECATED - now managed by App Configuration via Key Vault references
  # Kept for backward compatibility during migration phase
  connection_strings = {
    DefaultConnection = {
      type  = "SQLAzure"
      value = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=sql-connection-string-main)"
    }
    HangfireConnection = {
      type  = "SQLAzure"
      value = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=sql-connection-string-hangfire)"
    }
  }

  tags     = local.tags
  sku_name = local.api.sku

  depends_on = [azurerm_key_vault.main]
}

# Grant API App Service permission to pull images from ACR
resource "azurerm_role_assignment" "api_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = module.api_app.app_service_principal_id
}
