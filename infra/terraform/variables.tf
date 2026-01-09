# Cost-related variables are now centralized in cost_profiles.tf
# These defaults will be overridden by the cost profile configuration

variable "grafana_enabled" {
  description = <<EOT
Enable Azure Managed Grafana deployment.
When false, Grafana is not deployed (useful for dev environments to save costs).
You can share a single Grafana instance across environments by:
1. Deploying Grafana in prod only (grafana_enabled = true)
2. Configuring the prod Grafana to connect to dev Loki via datasource
EOT
  type        = bool
  default     = true
}

variable "grafana_major_version" {
  description = "Grafana major version."
  type        = number
  default     = 11
}

variable "grafana_public_network_access" {
  description = <<EOT
Enable public network access for Grafana.
When true, Grafana is accessible from the internet (useful for development/debugging).
When false, Grafana is only accessible via private endpoints (recommended for production).
EOT
  type        = bool
  default     = false
}

variable "grafana_admin_users" {
  description = <<EOT
List of Azure AD user/group object IDs to grant Grafana Admin role.
Get user object ID via: az ad user show --id "user@domain.com" --query id -o tsv
EOT
  type        = list(string)
  default     = []
}
# Global variables for Consilient Azure Infrastructure

variable "project_name" {
  description = "Project name used as prefix for all resources"
  type        = string
  default     = "consilient"
}

variable "region" {
  description = "Azure region to deploy resources."
  type        = string
}

variable "environment" {
  description = "Deployment environment (e.g., dev, staging, prod)."
  type        = string

  validation {
    condition     = contains(["dev", "prod"], var.environment)
    error_message = "Environment must be either 'dev' or 'prod'."
  }
}

variable "resource_group_name" {
  description = "Name of the resource group."
  type        = string
}

variable "loki_retention" {
  description = "Loki log retention period (e.g., 7d, 30d)."
  type        = string
  default     = "30d"
}

variable "sql_admin_username" {
  description = "SQL Server administrator username."
  type        = string
}

variable "sql_admin_password" {
  description = <<EOT
SQL Server administrator password.
Never set a default or commit this value to source control.
Inject securely using environment variables (TF_VAR_sql_admin_password), a secret manager, or your CI/CD pipeline.
EOT
  type        = string
  sensitive   = true
}

variable "loki_cpu_request" {
  description = "Loki container CPU request - must match Azure Container Apps valid combinations"
  type        = number
  default     = 0.5
}

variable "loki_cpu_limit" {
  description = "Loki container CPU limit (not used in Azure Container Apps)"
  type        = number
  default     = 1.0
}

variable "loki_memory_request" {
  description = "Loki container memory request - must be 1.0Gi for 0.5 CPU (Azure Container Apps requirement)"
  type        = string
  default     = "1.0Gi"
}

variable "loki_memory_limit" {
  description = "Loki container memory limit (not used in Azure Container Apps)"
  type        = string
  default     = "2.0Gi"
}

variable "subscription_id" {
  description = "Azure Subscription ID"
  type        = string
}

variable "create_container_app_environment" {
  description = "Whether to create a new Container App Environment or use an existing one"
  type        = bool
  default     = true
}

variable "existing_container_app_environment_id" {
  description = "ID of existing Container App Environment (only used if create_container_app_environment is false)"
  type        = string
  default     = ""
}

variable "container_app_environment_name_template" {
  description = <<EOT
Template for Container App Environment name with placeholder substitution.
Supports {environment} placeholder which will be replaced with the actual environment value.
Examples:
  - "consilient-cae-{environment}" → "consilient-cae-dev" for dev environment
  - "my-cae-{environment}" → "my-cae-prod" for prod environment
EOT
  type        = string
  default     = "consilient-cae-{environment}"

  validation {
    condition     = can(regex("^[a-z0-9]([a-z0-9-]{0,58}[a-z0-9])?$", replace(var.container_app_environment_name_template, "{environment}", "dev")))
    error_message = "Container App Environment name template must result in a valid Azure resource name (lowercase alphanumeric and hyphens, 1-60 chars, start/end with alphanumeric)."
  }
}

variable "enable_local_firewall" {
  description = <<EOT
Enable SQL Server firewall rule for local act testing.
WARNING: This opens SQL Server to all IPs (0.0.0.0/0) and enables public network access.
Only use for local development testing via act. Never enable in production.
EOT
  type        = bool
  default     = false
}

variable "jwt_signing_secret" {
  description = <<EOT
JWT signing secret for authentication tokens.
SECURITY: Never set a default or commit this value to source control.
Inject securely using environment variables (TF_VAR_jwt_signing_secret), a secret manager, or your CI/CD pipeline.
Generate using: openssl rand -base64 64
EOT
  type        = string
  sensitive   = true
}

variable "api_custom_domain" {
  description = <<EOT
Custom domain name for the API App Service.
If provided, Azure will automatically issue and manage an SSL certificate for this domain.
The domain must be registered and its DNS must be configured to point to the App Service.
Example: "api.example.com"
EOT
  type        = string
  default     = ""
}

variable "react_custom_domain" {
  description = <<EOT
Custom domain name for the React App Service.
If provided, Azure will automatically issue and manage an SSL certificate for this domain.
The domain must be registered and its DNS must be configured to point to the App Service.
Example: "app.example.com"
EOT
  type        = string
  default     = ""
}

variable "enable_unique_app_names" {
  description = <<EOT
DEPRECATED: Use hostname_naming_tier instead.
Enable unique suffixes for App Service names to avoid global hostname conflicts.
When true, forces hostname_naming_tier=2 (random suffix).
EOT
  type        = bool
  default     = false
}

variable "hostname_naming_tier" {
  description = <<EOT
Hostname naming tier for App Services (fallback strategy for global namespace conflicts).
  0 = Standard names (e.g., consilient-api-dev)
  1 = Region suffix (e.g., consilient-api-dev-eastus)
  2 = Random 4-letter suffix (e.g., consilient-api-dev-ab12)
Automatically determined by hostname-precheck.sh script before Terraform runs.
EOT
  type        = number
  default     = 0

  validation {
    condition     = contains([0, 1, 2], var.hostname_naming_tier)
    error_message = "hostname_naming_tier must be 0 (standard), 1 (region), or 2 (random)."
  }
}

variable "oauth_enabled" {
  description = <<EOT
Enable OAuth/Microsoft login functionality.
When enabled, creates:
  - Azure AD App Registration for OAuth
  - Client secret stored in Key Vault
  - OAuth configuration keys in App Configuration
The Terraform service principal needs Application.ReadWrite.All permission in Azure AD.
EOT
  type        = bool
  default     = false
}

variable "keyvault_user_email" {
  description = <<EOT
Email address of the user who should have "Key Vault Secrets Officer" role.
If provided, Terraform will automatically resolve the email to Azure AD object ID.
This allows passing user emails from CI/CD variables without hardcoding user IDs.
The email can be provided via:
  - GitHub workflow input: keyvault-user-email
  - GitHub repository variable: AZURE_USER_EMAIL
  - Environment variable: TF_VAR_keyvault_user_email
Example: "hernanmarano@consilientivh.com"
Leave empty to skip creating this role assignment.
NOTE: Requires Terraform service principal to have User.Read.All permission in Azure AD.
If permission is denied, use keyvault_user_object_id instead.
EOT
  type        = string
  default     = ""
}

variable "keyvault_user_object_id" {
  description = <<EOT
Azure AD object ID of the user who should have "Key Vault Secrets Officer" role.
Use this as an alternative when keyvault_user_email resolution fails due to insufficient permissions.
The Terraform service principal needs User.Read.All permission in Azure AD to resolve email addresses.
If that permission is not available, provide the object ID directly instead.
Get object ID via: az ad user show --id "user@domain.com" --query id -o tsv
Example: "5daf8f81-7f50-4ad2-bb63-3e78917ab008"
Leave empty to use email-based resolution (if permissions allow).
EOT
  type        = string
  default     = ""
}

variable "loki_basic_auth_username" {
  description = <<EOT
Username for Loki Basic Auth.
Used to protect Loki endpoint from unauthorized access.
EOT
  type        = string
  default     = "loki"
}

variable "loki_basic_auth_password" {
  description = <<EOT
Password for Loki Basic Auth.
If left empty, Terraform will auto-generate a strong password and store it in Key Vault.
You can retrieve it later via: az keyvault secret show --vault-name <vault> --name loki-basic-auth-password --query value -o tsv
EOT
  type        = string
  sensitive   = true
  default     = ""
}
