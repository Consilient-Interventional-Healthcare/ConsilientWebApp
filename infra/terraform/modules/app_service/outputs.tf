
output "app_service_id" {
  value = azurerm_linux_web_app.this.id
}

output "app_service_plan_id" {
  value = azurerm_service_plan.this.id
}

output "app_service_name" {
  description = "The name of the App Service"
  value       = azurerm_linux_web_app.this.name
}

output "app_service_default_hostname" {
  description = "The default hostname of the App Service"
  value       = azurerm_linux_web_app.this.default_hostname
}

output "app_service_principal_id" {
  description = "The principal ID of the App Service managed identity"
  value       = azurerm_linux_web_app.this.identity[0].principal_id
}
