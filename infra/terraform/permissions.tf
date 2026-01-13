# ============================================================================
# AZURE RBAC ROLE ASSIGNMENTS
# Centralized management of all role-based access control assignments
# ============================================================================
# This file contains all Azure role assignments used throughout the infrastructure.
# Organizing them in one place makes it easier to audit, manage, and understand
# the permissions granted to services, applications, and users.

# ============================================================================
# USER EMAIL TO OBJECT ID RESOLUTION
# ============================================================================
# When keyvault_user_email is provided, automatically resolve it to the Azure AD object ID
# This allows passing email addresses from CI/CD without hardcoding user IDs
#
# Configuration options (in order of precedence):
#   1. GitHub workflow input: -var="keyvault_user_email=user@domain.com" (manual dispatch)
#   2. GitHub repository variable: AZURE_USER_EMAIL (persistent, all runs)
#   3. Environment variable: TF_VAR_keyvault_user_email=user@domain.com (local)
#   4. Default: empty string (skip role assignment)
#
# The email is resolved to an Azure AD object ID and used to grant Key Vault Secrets Officer role.
# If the email doesn't exist or is empty, the role assignment is skipped.

data "azuread_user" "keyvault_admin" {
  count               = var.keyvault_user_email != "" ? 1 : 0
  user_principal_name = var.keyvault_user_email
}

# ============================================================================
# CONTAINER REGISTRY (ACR) PERMISSIONS
# ============================================================================
# API and React App Services need to pull container images from ACR

# Grant API App Service permission to pull images from ACR
resource "azurerm_role_assignment" "api_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = module.api_app.app_service_principal_id
}

# Grant React App Service permission to pull images from ACR
resource "azurerm_role_assignment" "react_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = module.react_app.app_service_principal_id
}

# ============================================================================
# KEY VAULT PERMISSIONS
# ============================================================================
# Various services need access to secrets stored in Key Vault

# Grant Terraform service principal "Key Vault Secrets Officer" role (manage secrets)
# This allows Terraform to create/update/delete secrets during infrastructure provisioning
resource "azurerm_role_assignment" "terraform_keyvault_secrets_officer" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azurerm_client_config.current.object_id

  depends_on = [azurerm_key_vault.main]
}

# Grant API App Service "Key Vault Secrets User" role (read-only access)
# Allows API to read secrets at runtime via its managed identity
resource "azurerm_role_assignment" "api_keyvault_secrets_user" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = module.api_app.app_service_principal_id

  # Prevent race condition - ensure identity exists before assigning role
  depends_on = [module.api_app]
}

# Grant App Configuration managed identity "Key Vault Secrets User" role
# Allows App Configuration to resolve Key Vault references at read time
resource "azurerm_role_assignment" "appconfig_keyvault_secrets_user" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_app_configuration.main.identity[0].principal_id

  depends_on = [
    azurerm_app_configuration.main,
    azurerm_key_vault.main
  ]
}

# Grant user "Key Vault Secrets Officer" role (manage secrets)
# Resolves email address to Azure AD object ID and grants access
# Only created if keyvault_user_email is provided and user exists in Azure AD
#
# NOTE: This requires the Terraform service principal to have permission to read Azure AD users
# If you see "Authorization_RequestDenied" error, use keyvault_user_object_id instead
resource "azurerm_role_assignment" "user_keyvault_secrets_officer" {
  count = var.keyvault_user_email != "" && var.keyvault_user_object_id == "" ? 1 : 0

  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azuread_user.keyvault_admin[0].object_id

  depends_on = [
    azurerm_key_vault.main,
    data.azuread_user.keyvault_admin
  ]
}

# Alternative: Grant user via direct object ID (when email resolution fails)
# This allows bypassing the Azure AD read permission requirement
# Use when Terraform service principal lacks User.Read.All permission in Azure AD
# Get object ID via: az ad user show --id "user@domain.com" --query id -o tsv
resource "azurerm_role_assignment" "user_keyvault_secrets_officer_by_id" {
  count = var.keyvault_user_object_id != "" ? 1 : 0

  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = var.keyvault_user_object_id

  depends_on = [azurerm_key_vault.main]
}

# ============================================================================
# APP CONFIGURATION PERMISSIONS
# ============================================================================
# Terraform and API need access to Azure App Configuration

# Grant Terraform service principal "App Configuration Data Owner" role
# Allows Terraform to create/read/update/delete configuration keys
resource "azurerm_role_assignment" "terraform_appconfig_owner" {
  scope                = azurerm_app_configuration.main.id
  role_definition_name = "App Configuration Data Owner"
  principal_id         = data.azurerm_client_config.current.object_id

  depends_on = [azurerm_app_configuration.main]
}

# Grant API App Service "App Configuration Data Reader" role
# Allows API to read configuration at runtime via its managed identity
resource "azurerm_role_assignment" "api_appconfig_reader" {
  scope                = azurerm_app_configuration.main.id
  role_definition_name = "App Configuration Data Reader"
  principal_id         = module.api_app.app_service_principal_id

  depends_on = [
    azurerm_app_configuration.main,
    module.api_app
  ]
}

# ============================================================================
# STORAGE PERMISSIONS
# ============================================================================
# Loki needs to write logs to Azure Blob Storage

# Grant Loki managed identity "Storage Blob Data Contributor" role
# Allows Loki to write and read log data from blob storage
resource "azurerm_role_assignment" "loki_blob" {
  scope                = azurerm_storage_account.loki.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.loki.principal_id
}

# ============================================================================
# GRAFANA PERMISSIONS
# ============================================================================
# Grant users/groups admin access to Grafana dashboards

# Grafana Admin role assignments for specified users
# Users from var.grafana_admin_users will have admin privileges on Grafana
# Only created when Grafana is enabled
resource "azurerm_role_assignment" "grafana_admins" {
  for_each             = var.grafana_enabled ? toset(var.grafana_admin_users) : toset([])
  scope                = azurerm_dashboard_grafana.main[0].id
  role_definition_name = "Grafana Admin"
  principal_id         = each.value
}

# Grant Grafana Admin to the Terraform service principal
# This allows the CI/CD pipeline to configure datasources via az grafana commands
resource "azurerm_role_assignment" "terraform_grafana_admin" {
  count                = var.grafana_enabled ? 1 : 0
  scope                = azurerm_dashboard_grafana.main[0].id
  role_definition_name = "Grafana Admin"
  principal_id         = data.azurerm_client_config.current.object_id
}
