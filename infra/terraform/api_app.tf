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
  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"

    # Key Vault configuration for secret loading via Managed Identity
    "KeyVault__Url" = "https://${azurerm_key_vault.main.name}.vault.azure.net/"

    # Key Vault references for secrets
    # Syntax: @Microsoft.KeyVault(VaultName=<vault-name>;SecretName=<secret-name>)
    # These are resolved at runtime by App Service using the managed identity
    "ApplicationSettings__Authentication__UserService__Jwt__Secret" = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=jwt-signing-secret)"
    "Logging__GrafanaLoki__Url"                                     = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=grafana-loki-url)"

    # OAuth Client Secret (only if OAuth is configured)
    "ApplicationSettings__Authentication__UserService__OAuth__ClientSecret" = var.oauth_client_secret != "" ? "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=oauth-client-secret)" : ""

    # Non-secret configuration
    "ApplicationSettings__Authentication__UserService__Jwt__Issuer"     = "https://${local.api.service_name}.azurewebsites.net"
    "ApplicationSettings__Authentication__UserService__Jwt__Audience"   = "https://${local.api.service_name}.azurewebsites.net"
    "ApplicationSettings__Authentication__UserService__Jwt__ExpiryMinutes" = "60"
    "ApplicationSettings__Authentication__Enabled"                      = "true"
    "Logging__GrafanaLoki__PushEndpoint"                                = "/loki/api/v1/push"
    "Logging__GrafanaLoki__BatchPostingLimit"                           = "100"
    "Logging__LogLevel__Default"                                        = "Information"
    "Logging__LogLevel__Microsoft.AspNetCore"                           = "Warning"
  }

  # Connection strings must be separate from app_settings
  # Azure App Service treats these specially
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
