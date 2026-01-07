variable "plan_name" {
  description = "Name of the App Service Plan."
  type        = string
}

variable "plan_tier" {
  description = "Tier for the App Service Plan."
  type        = string
}

variable "plan_size" {
  description = "Size for the App Service Plan."
  type        = string
}

variable "app_name" {
  description = "Name of the App Service."
  type        = string
}

variable "location" {
  description = "Azure region."
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name."
  type        = string
}

variable "linux_fx_version" {
  description = "Linux FX version string (Docker image)."
  type        = string
}

variable "vnet_route_all_enabled" {
  description = "Enable VNet route all."
  type        = bool
  default     = false
}

variable "app_settings" {
  description = "App settings for the App Service."
  type        = map(string)
  default     = {}
}

variable "tags" {
  description = "Tags to apply to resources."
  type        = map(string)
  default     = {}
}

variable "sku_name" {
  description = "The SKU name for the App Service Plan."
  type        = string
}

variable "connection_strings" {
  description = "Connection strings for the App Service. Each entry should have 'type' and 'value' keys."
  type = map(object({
    type  = string
    value = string
  }))
  default = {}
}

variable "container_registry_use_managed_identity" {
  description = "Enable using managed identity for container registry authentication."
  type        = bool
  default     = false
}

variable "container_registry_managed_identity_client_id" {
  description = "The client ID of the managed identity to use for container registry authentication. Leave empty for system-assigned identity."
  type        = string
  default     = ""
}

variable "custom_domain_name" {
  description = "Custom domain name for the App Service. If provided, Azure-managed SSL certificate will be automatically issued."
  type        = string
  default     = ""
}

variable "enable_https_only" {
  description = "Enforce HTTPS-only traffic to the App Service."
  type        = bool
  default     = true
}

variable "health_check_path" {
  description = "Path to the health check endpoint. If not set, health checks are disabled."
  type        = string
  default     = ""
}

variable "health_check_eviction_time_in_min" {
  description = "Time in minutes after which an unhealthy instance is removed from the load balancer. Range: 2-10 minutes. Only used if health_check_path is set."
  type        = number
  default     = 10

  validation {
    condition     = var.health_check_eviction_time_in_min >= 2 && var.health_check_eviction_time_in_min <= 10
    error_message = "Health check eviction time must be between 2 and 10 minutes."
  }
}

variable "subnet_id" {
  description = "Subnet ID for VNet integration (required for accessing internal Container Apps)"
  type        = string
  default     = null
}
