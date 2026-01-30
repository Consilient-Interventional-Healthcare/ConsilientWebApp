# Consilient Build System

NUKE-based build system with interactive menu and CLI support.

## Quick Start

```cmd
build.cmd                    # Interactive menu
build.cmd GenerateAllTypes   # Non-interactive
build.cmd --help             # Show help
```

## Secrets Configuration

Environment variables are loaded from `.nuke/.env.local`.

### Setup

1. Copy the template:
   ```cmd
   copy build\.env.template .nuke\.env.local
   ```

2. Edit `.nuke/.env.local` and fill in your values

3. The `.nuke/` folder is gitignored - secrets are never committed

### Template Location

See `build/.env.template` for all available variables with descriptions.

### Variables Used by Build

| Variable | Description | Default |
|----------|-------------|---------|
| `SQL_ADMIN_USERNAME` | Database username | `sa` |
| `SQL_ADMIN_PASSWORD` | Database password | `YourStrong!Passw0rd` |
| `DB_DOCKER` | Use Docker for database | `true` |
| `DB_CONTAINER_NAME` | Docker container name | `consilient.dbs.container` |
| `DB_COMPOSE_FILE` | Docker compose file path | `src/.docker/docker-compose.yml` |
| `DB_SERVICE_NAME` | Docker service name | `db` |
| `DB_AUTO_START` | Auto-start container | `false` |

### For Terraform Operations

The `TerraformPlan` target requires these variables from `.nuke/.env.local`:

| Variable | Description |
|----------|-------------|
| `ARM_CLIENT_ID` | Azure service principal ID |
| `ARM_CLIENT_SECRET` | Azure service principal secret |
| `ARM_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Target Azure subscription |
| `AZURE_REGION` | Azure region (e.g., `canadacentral`) |
| `AZURE_RESOURCE_GROUP_NAME` | Target resource group |
| `SQL_ADMIN_USERNAME` | SQL server admin username |
| `SQL_ADMIN_PASSWORD` | SQL server admin password |
| `JWT_SIGNING_SECRET` | JWT signing secret for API |

### For Other Scripts (Act)

Other scripts in `scripts/` also read from `.nuke/.env.local`:
- GitHub token (`GITHUB_TOKEN`)
- Other service credentials

## Interactive Menu

Launch with `build.cmd` (no arguments):

```
┌─────────────────────────────────────────┐
│           Consilient                    │
├─────────────────────────────────────────┤
│  > Generate Code                        │
│    Database Actions                     │
│    Docker                               │
│    Terraform                            │
│    Exit                                 │
└─────────────────────────────────────────┘
```

**Menu Structure:**
- **Generate Code** - All Types, OpenAPI, GraphQL, TypeScript
- **Database Actions** - Add Migration, Apply, Squash, Rebuild, Docs
- **Docker** - Nuclear Reset
- **Terraform** - Plan

## Command Reference

### Code Generation

| Command | Description |
|---------|-------------|
| `build.cmd GenerateAllTypes` | Generate all types (recommended) |
| `build.cmd GenerateAllTypes --force` | Force regeneration |
| `build.cmd GenerateOpenApiDoc` | OpenAPI spec only |
| `build.cmd GenerateGraphQL` | GraphQL schema only |
| `build.cmd GenerateApiTypes` | TypeScript types only |

### Database Operations

| Command | Description |
|---------|-------------|
| `build.cmd AddMigration --migration-name Name --db-context ConsilientDbContext` | Create migration |
| `build.cmd UpdateLocalDatabase --db-context Both` | Apply pending migrations |
| `build.cmd GenerateMigrationScript --db-context ConsilientDbContext` | Generate SQL script |
| `build.cmd SquashMigrations --db-context ConsilientDbContext --force` | Consolidate migrations |
| `build.cmd RebuildDatabase --force` | Rebuild database |
| `build.cmd RebuildDatabase --force --backup` | Rebuild with backup |
| `build.cmd GenerateDatabaseDocs --db-auto-start` | Generate documentation |

**DbContext options:** `ConsilientDbContext`, `UsersDbContext`, `Both`

### Docker Operations

| Command | Description |
|---------|-------------|
| `build.cmd DockerUp` | Start all services |
| `build.cmd DockerDown` | Stop all services |
| `build.cmd DockerBuild` | Build images |
| `build.cmd DockerRestart` | Restart services |
| `build.cmd DockerNuclearReset --force` | Complete cleanup |

### Terraform Operations

| Command | Description |
|---------|-------------|
| `build.cmd TerraformPlan` | Generate Terraform plan (Fresh state, dev) |
| `build.cmd TerraformPlan --terraform-environment prod` | Plan for production |
| `build.cmd TerraformPlan --terraform-state-source Local` | Use local state file |
| `build.cmd TerraformPlan --terraform-state-source Remote` | Use Azure backend state |

**State Source options:**
- `Fresh` (default) - No state, shows what would be created from scratch
- `Local` - Uses local `terraform.tfstate` file
- `Remote` - Connects to Azure blob storage backend

**Output:** Plan is written to temp folder and the path is printed at the end.

### Build & Test

| Command | Description |
|---------|-------------|
| `build.cmd Compile` | Build all projects (default) |
| `build.cmd Compile --configuration Release` | Release build |
| `build.cmd Test` | Run backend tests |
| `build.cmd TestFrontend` | Run frontend tests |
| `build.cmd LintFrontend` | Run ESLint |

## Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `--force` | Skip confirmation prompts | `false` |
| `--db-context` | Target database context | `Both` |
| `--migration-name` | Migration name | Required |
| `--backup` | Create backup before rebuild | `false` |
| `--db-auto-start` | Auto-start DB container | `false` |
| `--configuration` | Build configuration | `Debug` |
| `--terraform-environment` | Target environment (dev, prod) | `dev` |
| `--terraform-state-source` | State source (Fresh, Local, Remote) | `Fresh` |

## Troubleshooting

### Database connection fails
```cmd
build.cmd DockerUp
build.cmd CheckDatabaseHealth
```

### Migration shows "may result in data loss"
Review the migration file carefully - this warning appears when dropping columns/tables.

### Force regeneration when stuck
```cmd
build.cmd GenerateAllTypes --force
```

### Complete Docker reset
```cmd
build.cmd DockerNuclearReset --force
```

### Terraform plan fails with authentication error
Ensure Azure credentials are configured in `.nuke/.env.local`:
```cmd
build.cmd TerraformPlan --terraform-state-source Fresh
```
Use `Fresh` state source to bypass remote state authentication issues during testing.
