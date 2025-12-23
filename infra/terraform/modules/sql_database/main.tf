resource "azurerm_mssql_database" "this" {
  name           = var.name
  server_id      = var.server_id
  sku_name       = var.sku_name
  zone_redundant = var.zone_redundant
  tags           = var.tags

  # Serverless tier configuration (for GP_S SKUs)
  min_capacity                = var.min_capacity
  auto_pause_delay_in_minutes = var.auto_pause_delay_in_minutes
}
