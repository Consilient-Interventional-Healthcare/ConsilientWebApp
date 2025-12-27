# Resource Naming Conventions

Standardized naming patterns for all Azure resources.

## Global Pattern

**Format:** `{project}-{type}-{name}-{environment}-{unique_suffix}`

**Example:** `consilient-api-app-dev-a1b2c3`

**Components:**
- `project` = "consilient" (fixed)
- `type` = resource category
- `name` = specific resource name
- `environment` = dev, staging, prod
- `unique_suffix` = 6-char hash (ensures global uniqueness)

**Defined in:** [`infra/terraform/locals.tf:32-65`](../../../infra/terraform/locals.tf#L32-L65)

## Unique Suffix Calculation

Generated from subscription ID + resource group name (deterministic):

```hcl
unique_suffix = substr(md5("${var.subscription_id}-${var.resource_group_name}"), 0, 6)
```

**Result:** Same suffix for same subscription + RG combination

**Why:** Ensures globally unique names (Azure requires this for some resources)

## Resource Type Abbreviations

| Resource | Type | Example |
|----------|------|---------|
| App Service | api, web | consilient-api-app-dev-x1y2z3 |
| Container Registry | acr | consilient-acr-dev-x1y2z3 |
| SQL Server | sql | consilient-sql-dev-x1y2z3 |
| Storage Account | st | consilientst dev x1y2z3 |
| Virtual Network | vnet | consilient-vnet-dev-x1y2z3 |
| Subnet | subnet | consilient-subnet-dev-x1y2z3 |
| Container App Env | cae | consilient-cae-dev-x1y2z3 |

## Database Naming

**Different pattern for databases** (no unique suffix):

```
consilient_{database}_{environment}
```

**Examples:**
- `consilient_main_dev` - Main app database (dev)
- `consilient_main_prod` - Main app database (prod)
- `consilient_hangfire_dev` - Background jobs (dev)
- `consilient_hangfire_prod` - Background jobs (prod)
- `consilient_customdb_dev` - Custom database (dev)

**Auto-Discovery:** Directory name maps to database name:
- Directory: `Main/` → `consilient_main_{env}`
- Directory: `Hangfire/` → `consilient_hangfire_{env}`
- Directory: `CustomDB/` → `consilient_customdb_{env}`

See [components/databases.md](../components/databases.md#naming-convention) for details.

## Environment Suffixes

| Environment | Code | Example |
|------------|------|---------|
| Development | dev | consilient-api-app-dev-x1y2z3 |
| Staging | staging | consilient-api-app-staging-x1y2z3 |
| Production | prod | consilient-api-app-prod-x1y2z3 |

## Examples by Resource Type

### App Services

**API:**
```
consilient-api-app-dev-a1b2c3
consilient-api-app-prod-a1b2c3
```

**React:**
```
consilient-react-app-dev-a1b2c3
consilient-react-app-prod-a1b2c3
```

### Container Registry

```
consilientacrdeveca75c    (dev)
consilientacrprode74c2a   (prod)
```

Note: ACR names don't use hyphens (Azure requirement)

### SQL Server

```
consilient-sql-dev-a1b2c3
consilient-sql-prod-a1b2c3
```

### Storage Account

```
consilientstdevx1y2z3     (dev)
consilientstprodx1y2z3    (prod)
```

Note: Storage names don't use hyphens, lowercase only

## Implementation

All naming logic centralized in [`locals.tf`](../../../infra/terraform/locals.tf):

```hcl
locals {
  # Naming components
  unique_suffix = substr(md5("${var.subscription_id}-${var.resource_group_name}"), 0, 6)

  # Resource names
  api_app_name = "consilient-api-app-${var.environment}-${local.unique_suffix}"
  react_app_name = "consilient-react-app-${var.environment}-${local.unique_suffix}"
  acr_name = "consilient-acr-${var.environment}-${local.unique_suffix}"

  # Database names
  main_database_name = "consilient_main_${var.environment}"
  hangfire_database_name = "consilient_hangfire_${var.environment}"
}
```

**Benefits:**
- Single source of truth
- Easy to change naming convention
- Consistent across all resources
- Deterministic (same inputs = same names)

## Database Documentation Files

Configuration and output files for the automated database documentation system.

| Item | Pattern | Example |
|------|---------|---------|
| Configuration File | `src/Databases/{Name}/db_docs.yml` | `src/Databases/Main/db_docs.yml` |
| Configuration Template | `src/Databases/db_docs.yml.template` | Reference for creating new configs |
| Schema Discovery SQL | `infra/db/list_user_schemas.sql` | (Single file, no variation) |
| Discovery Action | `.github/actions/discover-databases/` | (Single action, no variation) |
| Documentation Workflow | `.github/workflows/docs_db.yml` | (Single workflow, no variation) |
| Output Artifact | `database-documentation-{name}-{suffix}` | `database-documentation-consilient_main-latest.zip` |

**Configuration File Naming:**
- Always: `db_docs.yml` (exact spelling, lowercase)
- Location: Database directory root
- One file per database directory
- Optional (uses defaults if missing)

**Configuration Contents:**
```yaml
database:
  name: "MyDatabase"
  generate_docs: true

schemas:
  exclude: []
```

**Artifact Naming Format:**
- Prefix: `database-documentation-`
- Database: `{database_name}` (e.g., consilient_main)
- Suffix variants:
  - `latest` - Manual or main.yml trigger
  - `pr-{number}` - Pull request (e.g., pr-123)
  - `manual-{timestamp}` - Manual workflow dispatch

See [components/database-documentation.md](../components/database-documentation.md) for configuration guide.

## GitHub Actions Conventions

Standardized naming patterns for all GitHub Actions workflows, jobs, and inputs.

### Workflow File Naming

**Format:** `{number}-{purpose}.yml`

**Numbering System:**
- `00-09` - Infrastructure & setup workflows
- `10-19` - Deployment workflows (reserved)
- `20-29` - Validation & testing (reserved)

**Examples:**
- `00-build-runner-image.yml` - Custom GitHub Actions runner image
- `01-main.yml` - Main orchestrator workflow
- `02-terraform.yml` - Infrastructure deployment
- `03-dotnet-apps.yml` - .NET API deployment
- `04-react-apps.yml` - React frontend deployment
- `05-docs-db.yml` - Database documentation generation
- `06-databases.yml` - Database schema deployment

**Display Names** (in workflow metadata):
- Format: `"{Number} - {Purpose}"`
- Example: `"02 - Terraform Infrastructure"`
- Shown in GitHub Actions UI and workflow status

### Job Naming

**Job IDs** (machine readable):
- **Convention:** `kebab-case`
- **Examples:** `deploy-database`, `health-check-api`, `build-and-deploy`
- **Use in:** `jobs:` section, conditionals, dependencies

**Display Names** (human readable):
- **Convention:** Title Case
- **Examples:** "Deploy Database", "Health Check API", "Build and Deploy"
- **Use in:** `name:` field within job definition

**Pattern:**
```yaml
jobs:
  deploy-database:                          # Job ID (kebab-case)
    name: Deploy Database                   # Display name (Title Case)
```

### Input/Output Naming

**Convention:** `kebab-case` (all lowercase with hyphens)

**Examples:**
```yaml
inputs:
  environment:           # Single word
    type: string
  api-app-name:         # Multiple words
    type: string
  skip-health-checks:   # Boolean flag
    type: boolean
```

**Boolean Flag Prefixes:**
- `skip-` - Skip an action (e.g., `skip-databases`)
- `enable-` - Enable optional functionality (e.g., `enable-debug`)
- `allow-` - Allow restricted action (e.g., `allow-local-firewall`)

**Output Naming:**
```yaml
outputs:
  api-app-name:
    value: ${{ jobs.terraform.outputs.api_app_name }}
  previous-image:
    value: ${{ steps.capture_previous.outputs.image }}
```

### Secrets & Variables Naming

**Convention:** `SCREAMING_SNAKE_CASE` (all uppercase with underscores)

**Secrets** (sensitive credentials):
- `AZURE_CLIENT_ID` - OAuth client
- `SQL_ADMIN_USERNAME` - Database admin
- `GITHUB_TOKEN` - GitHub API access
- `ARM_CLIENT_SECRET` - Service principal secret

**Repository Variables** (non-sensitive configuration):
- `AZURE_REGION` - Azure region
- `ACR_REGISTRY_URL` - Container registry endpoint
- `TERRAFORM_VERSION` - Terraform version to use
- `ACTIONS_RUNNER_IMAGE` - Custom runner image URI

**Environment Variables** (workflow-scoped):
```yaml
env:
  SQL_SERVER: ${{ secrets.AZURE_SQL_SERVER }}      # Mix of inputs & secrets
  DATABASE_NAME: ${{ matrix.database.name }}        # From matrix
  TIMEOUT_SECONDS: 300                              # Constants (SCREAMING_SNAKE_CASE)
```

### Step Naming

**Convention:** Descriptive sentences (Title Case where appropriate)

**Good Examples:**
- "Checkout code"
- "Validate Required Secrets"
- "Login to Azure (OIDC + act fallback)"
- "Terraform Format Check"

**Bad Examples:**
- "Checkout" (too vague)
- "Run script" (non-descriptive)

### Composite Action Naming

**File Format:** `.github/actions/{action-name}/action.yml`

**Convention:** `kebab-case` directory names

**Examples:**
- `.github/actions/azure-login/` - Azure OIDC authentication
- `.github/actions/validate-inputs/` - Input validation
- `.github/actions/sqlcmd-execute/` - SQL script execution
- `.github/actions/discover-databases/` - Database discovery

---

## Best Practices

1. **Keep Names Short** - 30-63 character limit on most Azure resources
2. **Use Meaningful Names** - Clearly indicate resource purpose
3. **Avoid Sensitive Data** - Don't include passwords, keys, or personal info
4. **Be Consistent** - Follow pattern for all resources
5. **Document Custom Names** - If deviating from pattern, document why

## Related Documentation

- [`infra/terraform/locals.tf`](../../../infra/terraform/locals.tf) - Naming implementation
- [components/database-documentation.md](../components/database-documentation.md) - Database documentation configuration
- [components/databases.md](../components/databases.md) - Database deployment and structure
- [reference/cost-management.md](cost-management.md) - Resource organization
- [components/terraform.md](../components/terraform.md) - Terraform guide
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System design

---

**Last Updated:** December 2025
**For Navigation:** See [README.md](../README.md)
