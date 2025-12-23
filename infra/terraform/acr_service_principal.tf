# Service Principal for ACR CI/CD Authentication
# This creates a service principal with AcrPush permissions for GitHub Actions
# Uses Azure CLI via local-exec for creation due to permission constraints

variable "create_acr_service_principal" {
  description = "Whether to automatically create the ACR service principal"
  type        = bool
  default     = true
}

variable "env_act_file_path" {
  description = "Path to .env.act file to update with ACR credentials"
  type        = string
  default     = ""
}

# Create Azure AD App and Service Principal using Azure CLI
resource "null_resource" "create_acr_service_principal" {
  count = var.create_acr_service_principal ? 1 : 0

  provisioner "local-exec" {
    command = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"${path.module}/create_acr_sp.ps1\" -AcrName \"${local.acr.name}\""
  }

  lifecycle {
    create_before_destroy = true
  }
}

# Read the created values
data "local_file" "acr_sp_object_id" {
  count    = var.create_acr_service_principal ? 1 : 0
  filename = "acr_sp_object_id.txt"
  depends_on = [null_resource.create_acr_service_principal]
}

data "local_file" "acr_client_id" {
  count    = var.create_acr_service_principal ? 1 : 0
  filename = "acr_client_id.txt"
  depends_on = [null_resource.create_acr_service_principal]
}

data "local_file" "acr_client_secret" {
  count    = var.create_acr_service_principal ? 1 : 0
  filename = "acr_client_secret.txt"
  depends_on = [null_resource.create_acr_service_principal]
}

# Update .env.act file if running under act
resource "null_resource" "update_env_act" {
  count = (var.create_acr_service_principal && var.env_act_file_path != "") ? 1 : 0

  provisioner "local-exec" {
    command = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"${path.module}/update_env_act.ps1\" -EnvActFile \"${var.env_act_file_path}\" -AcrRegistry \"${local.acr.name}.azurecr.io\""
  }

  depends_on = [data.local_file.acr_client_id, data.local_file.acr_client_secret]
}

# Role Assignment: AcrPush for the service principal on the ACR
resource "azurerm_role_assignment" "acr_push" {
  count                = var.create_acr_service_principal ? 1 : 0
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPush"
  principal_id         = trimspace(data.local_file.acr_sp_object_id[0].content)
}

# Role Assignment: AcrPull for the service principal on the ACR (needed to pull base images)
resource "azurerm_role_assignment" "acr_pull" {
  count                = var.create_acr_service_principal ? 1 : 0
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = trimspace(data.local_file.acr_sp_object_id[0].content)
}
