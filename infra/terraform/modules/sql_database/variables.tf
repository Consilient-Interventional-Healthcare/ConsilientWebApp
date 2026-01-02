
variable "name" {
  description = "Name of the SQL database."
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

variable "server_name" {
  description = "SQL server name."
  type        = string
}

variable "sku_name" {
  description = "SKU name for the database."
  type        = string
}

variable "zone_redundant" {
  description = "Enable zone redundancy."
  type        = bool
  default     = false
}

variable "tags" {
  description = "Tags to apply to the database."
  type        = map(string)
  default     = {}
}

variable "server_id" {
  description = "The ID of the SQL server to associate with this database."
  type        = string
}

variable "min_capacity" {
  description = "Minimal capacity for serverless databases (GP_S SKUs). Required for serverless tier."
  type        = number
  default     = null
}

variable "auto_pause_delay_in_minutes" {
  description = "Time in minutes after which database is automatically paused. -1 to disable auto-pause."
  type        = number
  default     = null
}
