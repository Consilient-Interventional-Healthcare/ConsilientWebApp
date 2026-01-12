# Consilient Web App - Scripts Registry

Central index of all scripts for debugging, troubleshooting, and automation.

## Quick Reference

| Category | Script | Purpose |
|----------|--------|---------|
| Common | Load-Environment.ps1 | Load secrets by environment and category |
| Act | Invoke-ActWorkflow.ps1 | Run GitHub Actions locally |
| Act | Build-RunnerImage.ps1 | Build custom Docker runner |
| Act | Initialize-ActCache.ps1 | Initialize act action cache |
| Docker | KillContainers.ps1 | Stop/remove Docker containers |
| Terraform | Run-TerraformPlan.ps1 | Run terraform plan locally |
| Loki | Get-LokiLogs.ps1 | Query logs from Loki using LogCLI |
| Database | AddMigration.ps1 | Add new EF migration |
| Database | CreateScript.ps1 | Generate idempotent SQL script |
| Database | UpdateDatabase.ps1 | Add migrations + generate scripts for all contexts |

## Secrets Management

Secrets are stored in `.env.local` (local development) and `.env.dev` (remote dev) files in this folder.

```bash
# Copy template and fill in values
cp .env.template .env.local
```

### Loading Secrets

```powershell
# Load all secrets for local environment
. scripts/common/Load-Environment.ps1
Import-ConsilientEnvironment -Environment local

# Load only Azure secrets
Import-ConsilientEnvironment -Environment local -Categories @('az')

# Load secrets for dev environment
Import-ConsilientEnvironment -Environment dev

# Get secrets as hashtable (without setting env vars)
$secrets = Import-ConsilientEnvironment -Environment local -PassThru
```

### Secret Categories

| Category | Variables |
|----------|-----------|
| `az` | ARM_CLIENT_ID, ARM_CLIENT_SECRET, ARM_TENANT_ID, AZURE_SUBSCRIPTION_ID, etc. |
| `db` | SQL_ADMIN_USERNAME, SQL_ADMIN_PASSWORD |
| `gh` | GITHUB_TOKEN, CONTAINER_REGISTRY |
| `loki` | LOKI_ADDR, LOKI_USERNAME, LOKI_PASSWORD |
| `act` | CAE_NAME_TEMPLATE, CAE_* variables |

---

## Scripts by Category

### Common (`scripts/common/`)

Shared modules used by other scripts.

| Script | Purpose | Usage |
|--------|---------|-------|
| **Load-Environment.ps1** | Load environment variables from secrets files | `Import-ConsilientEnvironment -Environment local` |

### Act (`scripts/act/`)

Local GitHub Actions testing with the `act` tool.

| Script | Purpose | Usage |
|--------|---------|-------|
| **Invoke-ActWorkflow.ps1** | Run GitHub Actions workflow locally | `.\Invoke-ActWorkflow.ps1 -Environment dev` |
| **Build-RunnerImage.ps1** | Build/verify custom Docker runner image | `.\Build-RunnerImage.ps1 -Force` |
| **Initialize-ActCache.ps1** | Extract pre-baked actions to cache | Called by Invoke-ActWorkflow |
| **ActConfig.ps1** | Shared configuration constants | Dot-source to use |
| **Write-Message.ps1** | Colored console output helper | Dot-source to use |

### Docker (`scripts/docker/`)

Docker container management utilities.

| Script | Purpose | Usage |
|--------|---------|-------|
| **KillContainers.ps1** | Stop/remove Consilient containers and images | `.\KillContainers.ps1` |

### Terraform (`scripts/terraform/`)

Local Terraform operations.

| Script | Purpose | Usage |
|--------|---------|-------|
| **Run-TerraformPlan.ps1** | Execute terraform plan with secrets loaded | `.\Run-TerraformPlan.ps1 -Environment dev` |

### Loki (`scripts/loki/`)

Loki log querying and monitoring scripts.

| Script | Purpose | Usage |
|--------|---------|-------|
| **Get-LokiLogs.ps1** | Query logs from Loki using LogCLI | `.\Get-LokiLogs.ps1 -App consilient-api -Level ERROR` |

### Database (`scripts/db/`)

EF Core migration utilities.

| Script | Purpose | Usage |
|--------|---------|-------|
| **AddMigration.ps1** | Add new EF migration | `.\AddMigration.ps1 -MigrationName "AddUserTable"` |
| **CreateScript.ps1** | Generate idempotent SQL script | `.\CreateScript.ps1` |
| **UpdateDatabase.ps1** | Add migrations + generate scripts for all contexts | `.\UpdateDatabase.ps1 -MigrationName "AddUserTable"` |

### Placeholders

These folders are reserved for future scripts:

- `scripts/az/` - Azure CLI debugging scripts

---

## Scripts in Other Locations

These scripts remain in their original locations due to tight coupling with their context.

### CI/CD Workflow Scripts (`.github/workflows/`)

| Location | Scripts | Purpose |
|----------|---------|---------|
| `.github/workflows/terraform/scripts/` | terraform-init.sh, import.sh, terraform-outputs.sh | Terraform CI/CD |
| `.github/workflows/database-docs/` | process-all-databases.sh, generate-index.sh | DB documentation |
| `.github/actions/discover-databases/` | discover-databases.sh | GitHub Action component |

### Project-Specific Scripts (`src/`)

| Location | Scripts | Purpose |
|----------|---------|---------|
| `src/Scripts/` | generate-types.ps1, organize-types.ps1 | TypeScript generation |
| `src/Consilient.Api/` | Generate-ApiTypes.ps1 | API type generation |
| `src/Consilient.WebApp2/scripts/` | docker-entrypoint.sh | Container entrypoint |

---

## Migration from Legacy Locations

If you have existing secrets in legacy locations, copy them to the new location:

```powershell
# From .env.local in repo root
cp .env.local scripts/.env.local

# From infra/act/.env.act
cp infra/act/.env.act scripts/.env.local
```

The scripts include fallback support for legacy locations during migration.
