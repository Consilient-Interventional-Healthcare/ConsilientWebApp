resource "azurerm_mssql_database" "this" {
  name           = var.name
  server_id      = var.server_id
  sku_name       = var.sku_name
  zone_redundant = var.zone_redundant
  tags           = var.tags

  # Conditional attributes: only set for serverless SKUs (GP_S)
  # Basic and GP_Gen5 SKUs don't support these parameters
  # Setting to null for non-serverless SKUs prevents Azure API errors
  min_capacity                = strcontains(var.sku_name, "GP_S") ? var.min_capacity : null
  auto_pause_delay_in_minutes = strcontains(var.sku_name, "GP_S") ? var.auto_pause_delay_in_minutes : null
}
