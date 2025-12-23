# Cost-related variables are now centralized in cost_profiles.tf
# These defaults will be overridden by the cost profile configuration

variable "grafana_major_version" {
  description = "Grafana major version."
  type        = number
  default     = 11
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
