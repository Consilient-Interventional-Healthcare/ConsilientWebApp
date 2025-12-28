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
