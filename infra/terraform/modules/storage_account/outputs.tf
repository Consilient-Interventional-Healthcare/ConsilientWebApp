output "storage_account_id" {
  value = azurerm_storage_account.this.id
}

output "container_id" {
  value = azurerm_storage_container.this.id
}
