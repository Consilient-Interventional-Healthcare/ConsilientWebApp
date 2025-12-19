variable "acr_sku" {
  description = "SKU for Azure Container Registry."
  type        = string
  default     = "Standard"
}

variable "api_appservice_tier" {
  description = "App Service Plan tier for API."
  type        = string
  default     = "P1v2"
}

variable "api_appservice_size" {
  description = "App Service Plan size for API."
  type        = string
  default     = "P1v2"
}

variable "react_appservice_tier" {
  description = "App Service Plan tier for React app."
  type        = string
  default     = "P1v2"
}

variable "react_appservice_size" {
  description = "App Service Plan size for React app."
  type        = string
  default     = "P1v2"
}

variable "grafana_major_version" {
  description = "Grafana major version."
  type        = number
  default     = 11
}
variable "main_db_sku_name" {
  description = "SKU name for the main (business) database."
  type        = string
  default     = "GP_S_Gen5_2"
}

variable "hangfire_db_sku_name" {
  description = "SKU name for the Hangfire database."
  type        = string
  default     = "GP_S_Gen5_2"
}
# Global variables for Consilient Azure Infrastructure

variable "region" {
  description = "Azure region to deploy resources."
  type        = string
}

variable "environment" {
  description = "Deployment environment (e.g., dev, staging, prod)."
  type        = string
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
  description = "Loki container CPU request (e.g., 0.5)"
  type        = number
  default     = 0.5
}

variable "loki_cpu_limit" {
  description = "Loki container CPU limit (e.g., 1.0)"
  type        = number
  default     = 1.0
}

variable "loki_memory_request" {
  description = "Loki container memory request (e.g., 512Mi)"
  type        = string
  default     = "512Mi"
}

variable "loki_memory_limit" {
  description = "Loki container memory limit (e.g., 1Gi)"
  type        = string
  default     = "1Gi"
}

variable "subscription_id" {
  description = "Azure Subscription ID"
  type        = string
}
