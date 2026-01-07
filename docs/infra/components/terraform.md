# Terraform Infrastructure Guide

<!-- AI_CONTEXT: Infrastructure as Code using Terraform. All Azure resources defined in code. Two environments: dev ($45/mo) and prod ($2,800/mo). No staging. -->

## For Non-Technical Stakeholders

Terraform is a tool that defines cloud infrastructure in code files instead of clicking through Azure Portal. All resources (databases, servers, networks) are described in `.tf` files. When you run Terraform, it creates or updates Azure resources to match the code. This means infrastructure is versioned in Git, reviewable in pull requests, and reproducible with one command.

---

Complete guide to managing Azure infrastructure with Terraform.

## Overview

All infrastructure for Consilient is defined as code using Terraform and managed in `infra/terraform/`.

**Key Facts:**
- **Version:** Terraform >= 1.5.0
- **Provider:** azurerm >= 3.0.0 (locked at 4.57.0)
- **State:** Local (can migrate to Azure Storage)
- **Repository:** `infra/terraform/`
- **Environments:** Two (dev and prod only, validated in variables.tf:27-28)

## Configuration Architecture

### Single Source of Truth

All configuration is centralized in [`locals.tf`](../../../infra/terraform/locals.tf) including:
- Resource naming conventions
- Cost profiles (dev and prod only)
- SKU configurations
- Environment-specific settings

**Why:** Changes propagate automatically to all resources, reducing drift.

<!-- AI_CONTEXT: Two environments validated in variables.tf. No staging configuration should be used. -->

### File Organization

```
infra/terraform/
├── main.tf                 # Provider config, resource group
├── locals.tf              # Centralized configuration (SOURCE OF TRUTH)
├── variables.tf           # Input variable definitions
├── outputs.tf             # Output values
├── backend.tf             # State backend config
│
├── network.tf             # VNet, Subnet, NSGs
├── acr.tf                 # Azure Container Registry
├── api_app.tf             # API App Service
├── react_app.tf           # React App Service
├── sql.tf                 # SQL Server + Databases
├── storage.tf             # Storage Account (Loki logs)
├── loki.tf                # Container App Environment + Loki
├── grafana.tf             # Managed Grafana
│
└── modules/
    ├── app_service/       # Reusable module for App Services
    ├── sql_database/      # Reusable module for databases
    └── storage_account/   # Reusable module for storage
```

## Naming Conventions

**Pattern:** `{project}-{type}-{name}-{environment}-{unique_suffix}`

**Example:** `consilient-api-app-dev-a1b2c3`

**Defined in:** [`locals.tf:32-65`](../../../infra/terraform/locals.tf#L32-L65)

**Components:**
- `project` = "consilient" (fixed)
- `type` = resource type (api, db, acr, etc.)
- `name` = resource name
- `environment` = dev, staging, prod
- `unique_suffix` = 6-char hash (global uniqueness)

**Unique Suffix Calculation:** [`locals.tf:15-17`](../../../infra/terraform/locals.tf#L15-L17)

Database naming uses different pattern:
```
consilient_{database}_{environment}
Example: consilient_main_dev, consilient_hangfire_prod
```

See [reference/naming-conventions.md](../reference/naming-conventions.md) for all patterns.

## Cost Management

### Three-Tier Environment Strategy

**Configuration:** [`locals.tf:85-120`](../../../infra/terraform/locals.tf#L85-L120)

| Environment | Monthly Cost | SKU Strategy |
|-------------|------------|--------------|
| **Development** | ~$200 | Basic/Low-cost |
| **Staging** | ~$1,200 | Standard |
| **Production** | ~$2,800 | Premium |

**Cost Drivers:**
1. App Service Plans (largest)
2. SQL Database tier
3. Storage & Monitoring

See [reference/cost-management.md](../reference/cost-management.md) for optimization tips.

## Key Resources

**Networking:** [`network.tf`](../../../infra/terraform/network.tf)
- VNet, Subnet, NSGs

**Compute:**
- API App Service: [`api_app.tf`](../../../infra/terraform/api_app.tf)
- React App Service: [`react_app.tf`](../../../infra/terraform/react_app.tf)

**Container Registry:** [`acr.tf`](../../../infra/terraform/acr.tf)

**Database:** [`sql.tf`](../../../infra/terraform/sql.tf)
- SQL Server with Azure AD auth
- Main & Hangfire databases
- Serverless configuration

**Monitoring:**
- Storage: [`storage.tf`](../../../infra/terraform/storage.tf)
- Loki: [`loki.tf`](../../../infra/terraform/loki.tf)
- Grafana: [`grafana.tf`](../../../infra/terraform/grafana.tf)

## Modules

**Location:** [`modules/`](../../../infra/terraform/modules/)

- **app_service** - Reusable App Service configuration
- **sql_database** - Database module with serverless support
- **storage_account** - Storage with private endpoints

## Common Commands

```powershell
cd infra/terraform

# Initialize
terraform init

# Validate configuration
terraform validate

# Plan changes (always do this first!)
terraform plan -out=tfplan

# Apply changes
terraform apply tfplan

# Show outputs
terraform output

# List resources
terraform state list

# Destroy (caution in production!)
terraform destroy
```

## State Management

### Current Setup

**GitHub Actions:** Azure Storage backend with OIDC authentication
- State file: `{environment}.terraform.tfstate` (e.g., `dev.terraform.tfstate`)
- Storage account: `consilienttfstate{environment}{hash}`
- Container: `tfstate`
- Authentication: Azure AD federated identity (OIDC)

**Local Testing (act):** Local backend
- State file: `infra/terraform/terraform.tfstate`
- Backend: Disabled (`-reconfigure -backend=false`)
- Persists via `--bind` mount between test runs

### Backend Configuration

**File:** [`backend.tf`](../../../infra/terraform/backend.tf)

The backend is configured for Azure Storage with OIDC:
```hcl
terraform {
  backend "azurerm" {
    use_oidc = true
  }
}
```

Configuration details are provided via CLI flags in the workflow to support conditional backend setup:
- GitHub Actions: Uses Azure Storage backend
- Local (act): Uses local backend with `backend=false` flag

### State Detection

State detection uses `terraform state list` to check if state exists, which works reliably with both local and remote backends:
```bash
STATE_RESOURCES=$(terraform state list 2>/dev/null || echo "")
if [ -z "$STATE_RESOURCES" ]; then
  # Truly fresh deployment
fi
```

This approach queries the actual backend instead of checking local files, preventing false positives when state exists remotely but local files don't.

### GitHub Variables Required

Two variables must be configured in GitHub repository settings:

```
TF_STATE_STORAGE_ACCOUNT = consilienttfstate{env}{hash}
TF_STATE_CONTAINER = tfstate
```

See [reference/secrets-variables.md](../reference/secrets-variables.md) for complete setup instructions.

## Security Best Practices

1. **State File Protection** - Never commit to Git, use remote backend for teams
2. **Access Control** - Service principal with Contributor role
3. **Sensitive Data** - Use `sensitive` attribute, rotate secrets quarterly
4. **Resource Security** - Enable Azure AD auth, encryption at rest, private endpoints

## Troubleshooting

See [TROUBLESHOOTING.md#terraform-errors](../TROUBLESHOOTING.md#terraform-errors) for:
- State lock issues
- Import loop problems
- Provider authentication errors
- Remote backend configuration errors
- OIDC authentication failures

## Related Documentation

- [components/azure-resources.md](azure-resources.md) - Azure service details
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System design
- [reference/naming-conventions.md](../reference/naming-conventions.md) - Naming patterns
- [reference/cost-management.md](../reference/cost-management.md) - Cost optimization

---

**Last Updated:** December 2025
**For Navigation:** See [README.md](../README.md)
