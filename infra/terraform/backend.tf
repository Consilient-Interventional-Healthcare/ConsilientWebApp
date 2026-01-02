# Remote backend configuration for GitHub Actions
# For local testing (act), backend config is omitted via -reconfigure flag
terraform {
  backend "azurerm" {
    # Configuration provided via CLI flags in workflow
    # This allows conditional backend based on environment (GitHub Actions vs act)
    use_oidc = true  # Use OIDC authentication instead of service principal
  }
}
