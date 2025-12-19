# Output values for Consilient Azure Infrastructure

output "resource_group_name" {
  value     = azurerm_resource_group.main.name
}

# Example: Mark sensitive outputs (add more as needed)
# output "sql_admin_password" {
#   value     = var.sql_admin_password
#   sensitive = true
# }

# Add more outputs as resources are defined
