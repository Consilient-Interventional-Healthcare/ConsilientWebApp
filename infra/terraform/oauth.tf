# Azure AD App Registration for OAuth
# Creates application, service principal, and client secret for Microsoft OAuth login

locals {
  oauth = {
    enabled          = var.oauth_enabled
    app_display_name = "${var.project_name}-oauth-${var.environment}"
    # Callback URL for OAuth flow - matches AuthController endpoint
    redirect_uri = "https://${local.api.service_name}.azurewebsites.net/auth/microsoft/callback"
  }
}

# Azure AD Application Registration
resource "azuread_application" "oauth" {
  count        = local.oauth.enabled ? 1 : 0
  display_name = local.oauth.app_display_name

  sign_in_audience = "AzureADMyOrg" # Single tenant

  web {
    redirect_uris = [local.oauth.redirect_uri]

    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = false
    }
  }

  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # User.Read
      type = "Scope"
    }
    resource_access {
      id   = "64a6cdd6-aab1-4aaf-94b8-3cc8405e90d0" # email
      type = "Scope"
    }
    resource_access {
      id   = "14dad69e-099b-42c9-810b-d002981feec1" # profile
      type = "Scope"
    }
    resource_access {
      id   = "37f7f235-527c-4136-accd-4a02d197296e" # openid
      type = "Scope"
    }
  }

  tags = [var.environment, "oauth", "managed-by-terraform"]
}

# Service Principal for the application
resource "azuread_service_principal" "oauth" {
  count     = local.oauth.enabled ? 1 : 0
  client_id = azuread_application.oauth[0].client_id
}

# Client Secret (password) for the application
resource "azuread_application_password" "oauth" {
  count          = local.oauth.enabled ? 1 : 0
  application_id = azuread_application.oauth[0].id
  display_name   = "terraform-managed-secret"
  end_date       = timeadd(timestamp(), "8760h") # 1 year

  lifecycle {
    ignore_changes = [end_date]
  }
}

# Store client secret in Key Vault
resource "azurerm_key_vault_secret" "oauth_client_secret" {
  count        = local.oauth.enabled ? 1 : 0
  name         = "oauth-client-secret"
  value        = azuread_application_password.oauth[0].value
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_role_assignment.terraform_keyvault_secrets_officer]
}

# Output OAuth application details (for reference)
output "oauth_application_id" {
  value       = local.oauth.enabled ? azuread_application.oauth[0].client_id : null
  description = "OAuth Application (Client) ID"
}

output "oauth_redirect_uri" {
  value       = local.oauth.enabled ? local.oauth.redirect_uri : null
  description = "OAuth Redirect URI"
}
