# Azure Storage Account and Blob Container for Loki

resource "azurerm_storage_account" "loki" {
  name                          = local.loki.storage.account_name
  resource_group_name           = azurerm_resource_group.main.name
  location                      = azurerm_resource_group.main.location
  account_tier                  = "Standard"
  account_replication_type      = "LRS"
  public_network_access_enabled = false
  min_tls_version               = "TLS1_2"
  tags                          = local.tags
}

resource "azurerm_storage_container" "loki" {
  name                  = local.loki.storage.container_name
  storage_account_id    = azurerm_storage_account.loki.id
  container_access_type = "private"
}

# Private endpoint for Loki storage account
resource "azurerm_private_endpoint" "loki_storage" {
  name                = local.loki.storage.private_endpoint
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  subnet_id           = azurerm_subnet.private_endpoints.id
  tags                = local.tags

  private_service_connection {
    name                           = local.loki.storage.private_service_connection
    private_connection_resource_id = azurerm_storage_account.loki.id
    is_manual_connection           = false
    subresource_names              = ["blob"]
  }
}

# ============================================================================
# FILE UPLOADS STORAGE
# ============================================================================
# Azure Storage Account for file uploads (Doctor Assignments, etc.)
# Used by both ConsilientApi and BackgroundHost

resource "azurerm_storage_account" "uploads" {
  name                          = local.uploads_storage.account_name
  resource_group_name           = azurerm_resource_group.main.name
  location                      = azurerm_resource_group.main.location
  account_tier                  = "Standard"
  account_replication_type      = "LRS" # Cheapest: Locally Redundant Storage
  public_network_access_enabled = true  # Required for App Service access
  min_tls_version               = "TLS1_2"
  tags                          = local.tags
}

resource "azurerm_storage_container" "uploads" {
  name                  = local.uploads_storage.container_name
  storage_account_id    = azurerm_storage_account.uploads.id
  container_access_type = "private"
}

# Import block for existing storage account (if exists in Azure but not in TF state)
# Uncomment and run `terraform plan` to check if import is needed
# import {
#   to = azurerm_storage_account.uploads
#   id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.resource_group_name}/providers/Microsoft.Storage/storageAccounts/${local.uploads_storage.account_name}"
# }

# import {
#   to = azurerm_storage_container.uploads
#   id = "https://${local.uploads_storage.account_name}.blob.core.windows.net/uploads"
# }
