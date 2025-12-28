resource "azurerm_service_plan" "this" {
  name                = var.plan_name
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = var.sku_name
  tags                = var.tags
}

resource "azurerm_linux_web_app" "this" {
  name                = var.app_name
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.this.id
  https_only          = var.enable_https_only

  identity {
    type = "SystemAssigned"
  }

  site_config {
    vnet_route_all_enabled = var.vnet_route_all_enabled

    # Managed identity authentication for ACR
    container_registry_use_managed_identity = var.container_registry_use_managed_identity
    # Only set client_id if provided (must be a valid UUID)
    # When omitted/null, the provider uses the system-assigned identity automatically
    container_registry_managed_identity_client_id = var.container_registry_managed_identity_client_id != "" ? var.container_registry_managed_identity_client_id : null
  }

  app_settings = var.app_settings

  dynamic "connection_string" {
    for_each = var.connection_strings
    content {
      name  = connection_string.key
      type  = connection_string.value.type
      value = connection_string.value.value
    }
  }

  tags = var.tags
}

# Custom domain hostname binding (required before certificate can be issued)
resource "azurerm_app_service_custom_hostname_binding" "custom_domain" {
  count               = var.custom_domain_name != "" ? 1 : 0
  app_service_name    = azurerm_linux_web_app.this.name
  resource_group_name = var.resource_group_name
  hostname            = var.custom_domain_name

  depends_on = [azurerm_linux_web_app.this]
}

# Azure-managed SSL certificate for the custom domain
resource "azurerm_app_service_managed_certificate" "custom_domain" {
  count               = var.custom_domain_name != "" ? 1 : 0
  custom_hostname_binding_id = azurerm_app_service_custom_hostname_binding.custom_domain[0].id
}

# Bind the managed certificate to the custom domain
resource "azurerm_app_service_certificate_binding" "custom_domain" {
  count               = var.custom_domain_name != "" ? 1 : 0
  hostname_binding_id = azurerm_app_service_custom_hostname_binding.custom_domain[0].id
  certificate_id      = azurerm_app_service_managed_certificate.custom_domain[0].id
  ssl_state           = "SniEnabled"
}
