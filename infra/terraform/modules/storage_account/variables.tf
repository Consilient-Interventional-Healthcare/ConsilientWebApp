variable "name" {
  description = "Name of the storage account."
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name."
  type        = string
}

variable "location" {
  description = "Azure region."
  type        = string
}

variable "account_tier" {
  description = "Storage account tier."
  type        = string
  default     = "Standard"
}

variable "account_replication_type" {
  description = "Replication type."
  type        = string
  default     = "LRS"
}

variable "public_network_access_enabled" {
  description = "Enable public network access."
  type        = bool
  default     = false
}

variable "allow_blob_public_access" {
  description = "Allow public blob access."
  type        = bool
  default     = false
}

variable "min_tls_version" {
  description = "Minimum TLS version."
  type        = string
  default     = "TLS1_2"
}

variable "container_name" {
  description = "Name of the blob container."
  type        = string
}

variable "container_access_type" {
  description = "Access type for the blob container."
  type        = string
  default     = "private"
}

variable "tags" {
  description = "Tags to apply to resources."
  type        = map(string)
  default     = {}
}
