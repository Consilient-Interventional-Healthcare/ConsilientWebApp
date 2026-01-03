# Remote backend configuration for GitHub Actions
# For local testing (act), backend config is omitted via -reconfigure flag
terraform {
  backend "azurerm" {
    # Configuration provided via CLI flags in workflow
    # This allows conditional backend based on environment (GitHub Actions vs act)

    # Backend configuration (provided via workflow):
    # - resource_group_name: consilient-terraform (dedicated state resource group)
    # - storage_account_name: consilienttfstate
    # - container_name: tfstate
    # - key: ${environment}.terraform.tfstate (dev.terraform.tfstate, prod.terraform.tfstate)
    # - use_oidc: true (GitHub Actions OIDC authentication)

    use_oidc = true # Use OIDC authentication instead of service principal
  }
}
