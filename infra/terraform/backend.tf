# Example provider configuration for remote state (optional)
# Uncomment and configure if you want to use remote state
# terraform {
#   backend "azurerm" {
#     resource_group_name  = "<state-rg>"
#     storage_account_name = "<state-storage>"
#     container_name       = "tfstate"
#     key                  = "${var.environment}.terraform.tfstate"
#   }
# }
