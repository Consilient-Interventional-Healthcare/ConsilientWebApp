# Consilient Web Application - AI Assistant Instructions

> **For AI Assistants:** This is the canonical documentation for understanding and working with this codebase. Tool-specific files redirect here.

## Quick Reference

| Resource | Location | Purpose |
|----------|----------|---------|
| REST API Spec | [`docs/openapi.json`](docs/openapi.json) | OpenAPI 3.0 specification |
| GraphQL Schema | [`docs/schema.graphql`](docs/schema.graphql) | GraphQL SDL |
| API Source | [`src/Consilient.Api/`](src/Consilient.Api/) | Controllers, configuration |
| Frontend | [`src/Consilient.WebApp2/`](src/Consilient.WebApp2/) | React TypeScript SPA |
| Scripts | [`scripts/`](scripts/) | PowerShell utilities and secrets |

---

## Command Execution Guidelines (Windows)

### Operating System Context
- **Platform:** Windows with PowerShell
- **Shell:** PowerShell (default), cmd.exe (fallback)
- **Working Directory:** Use absolute paths or paths relative to repository root

### Script Execution

**Preferred: PowerShell scripts with `.\` prefix**
```powershell
.\build.ps1 GenerateAllTypes
.\build.ps1 AddMigration --migration-name MyMigration --db-context ConsilientDbContext
```

**Alternative: Batch files (no prefix needed)**
```cmd
build.cmd GenerateAllTypes
```

**Never use:**
```bash
# Linux-style execution (will fail)
./build.ps1
bash build.sh
```

### Quoting Rules

**Parameters with spaces or special characters:** Use double quotes
```powershell
.\build.ps1 AddMigration --migration-name "Add Patient Notes" --db-context ConsilientDbContext
```

**Simple parameters:** No quotes needed
```powershell
.\build.ps1 AddMigration --migration-name AddPatientNotes --db-context ConsilientDbContext
```

**Avoid single quotes for command arguments** - PowerShell treats them as literal strings without variable expansion.

### npm/Node.js Commands

**Frontend directory:** `src/Consilient.WebApp2/`
```powershell
# Run from repository root - specify working directory
cd src/Consilient.WebApp2 && npm run dev

# Or use npm prefix
npm --prefix src/Consilient.WebApp2 run dev
```

### Command Chaining

```powershell
# Sequential (stop on failure)
cd src/Consilient.WebApp2 && npm install && npm run build

# Sequential (continue on failure)
Remove-Item temp.txt; Write-Host "Done"
```

### Path Conventions

```powershell
# Both work in most contexts
src/Consilient.WebApp2/          # Forward slashes (cross-platform)
src\Consilient.WebApp2\          # Backslashes (Windows native)
```

### Common Pitfalls to Avoid

| Incorrect | Correct | Reason |
|-----------|---------|--------|
| `./build.ps1` | `.\build.ps1` | Windows uses backslash |
| `build.ps1` | `.\build.ps1` | Explicit path required for security |
| `'string param'` | `"string param"` | Use double quotes for parameters |
| `export VAR=value` | `$env:VAR = "value"` | PowerShell syntax |

---

## Secrets Management

### Credentials Location

Secrets are stored in environment files in `scripts/`:

| File | Purpose | Usage |
|------|---------|-------|
| `scripts/.env.dev` | **Remote dev environment credentials** | **Primary - use this for most operations** |
| `scripts/.env.local` | Local development credentials | Rarely used |
| `scripts/.env.template` | Template for creating new env files | Reference only |

### Loading Secrets

```powershell
# Load the environment loader
. scripts/common/Load-Environment.ps1

# Load all credentials for dev environment (most common)
Import-ConsilientEnvironment -Environment dev

# Load only specific categories
Import-ConsilientEnvironment -Environment dev -Categories @('az')
Import-ConsilientEnvironment -Environment dev -Categories @('az', 'db')
```

### Secret Categories

| Category | Variables | Load Command |
|----------|-----------|--------------|
| `az` | ARM_CLIENT_ID, ARM_CLIENT_SECRET, ARM_TENANT_ID, AZURE_SUBSCRIPTION_ID, AZURE_REGION, AZURE_RESOURCE_GROUP_NAME, ACR_REGISTRY_URL | `-Categories @('az')` |
| `db` | SQL_ADMIN_USERNAME, SQL_ADMIN_PASSWORD | `-Categories @('db')` |
| `gh` | GITHUB_TOKEN, CONTAINER_REGISTRY | `-Categories @('gh')` |
| `loki` | LOKI_ADDR, LOKI_USERNAME, LOKI_PASSWORD | `-Categories @('loki')` |
| `act` | CAE_NAME_TEMPLATE | `-Categories @('act')` |

---

## External CLI Tools

### Azure CLI (az)

#### Loading Credentials

Before running any `az` commands, load credentials into the session:

```powershell
. scripts/common/Load-Environment.ps1
Import-ConsilientEnvironment -Environment dev -Categories @('az')
```

#### Authentication with Service Principal

```powershell
# Login with service principal (credentials loaded from env vars)
az login --service-principal `
    --username $env:ARM_CLIENT_ID `
    --password $env:ARM_CLIENT_SECRET `
    --tenant $env:ARM_TENANT_ID

# Set the subscription
az account set --subscription $env:AZURE_SUBSCRIPTION_ID
```

#### Azure Environment Variables

| Variable | Description |
|----------|-------------|
| `ARM_CLIENT_ID` | Service principal application ID |
| `ARM_CLIENT_SECRET` | Service principal secret |
| `ARM_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Target subscription ID |
| `AZURE_REGION` | Default region (canadacentral) |
| `AZURE_RESOURCE_GROUP_NAME` | Default resource group |
| `ACR_REGISTRY_URL` | Azure Container Registry URL |

#### Common az Commands

```powershell
# List resources in the resource group
az resource list --resource-group $env:AZURE_RESOURCE_GROUP_NAME --output table

# Check current subscription
az account show --output table

# List container apps
az containerapp list --resource-group $env:AZURE_RESOURCE_GROUP_NAME --output table

# Get container app logs
az containerapp logs show --name consilient-api --resource-group $env:AZURE_RESOURCE_GROUP_NAME
```

### GitHub CLI (gh)

#### Authentication

```powershell
# Login interactively (one-time setup)
gh auth login

# Check auth status
gh auth status
```

#### Common gh Commands

```powershell
# Pull requests
gh pr list
gh pr view 123
gh pr create --title "Title" --body "Description"

# Issues
gh issue list

# Workflow runs
gh run list
gh run view 12345678
gh workflow run deploy.yml --ref main
```

#### Container Registry (GHCR)

```powershell
# Login to GHCR
echo $env:GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# Pull/push images
docker pull ghcr.io/consilient/consilient-api:latest
```

---

## Project Structure

### Backend (.NET 9)
- **`src/Consilient.Api/`** - Main ASP.NET Core Web API
  - Controllers: REST API endpoints
  - Init: Service registration and configuration
  - Hubs: SignalR real-time communication
- **`src/Consilient.BackgroundHost/`** - Hangfire background job processing
- **`src/Consilient.Data/`** - Entity Framework Core DbContext and migrations
- **`src/Consilient.Data.GraphQL/`** - GraphQL schema configuration
- **`src/Consilient.*.Services/`** - Business logic layer (Employee, Patient, Visit, etc.)
- **`src/Consilient.*.Contracts/`** - DTOs and contract types for API communication

### Frontend
- **`src/Consilient.WebApp2/`** - React 18 TypeScript SPA (Vite, TanStack Query, Tailwind)

### Infrastructure
- **Database:** SQL Server with EF Core 9
- **Background Jobs:** Hangfire with SQL Server storage
- **Authentication:** JWT bearer tokens via custom User Service
- **Configuration:** Azure App Configuration with Key Vault integration
- **Logging:** Serilog with structured logging

---

## Build System

The repository uses a NUKE build system. Use `.\build.ps1` (PowerShell) or `build.cmd` (cmd) at the repository root.

### Usage

```powershell
# Run the default target (Compile)
.\build.ps1

# Run a specific target
.\build.ps1 GenerateAllTypes

# Force regeneration even if outputs are up-to-date
.\build.ps1 GenerateAllTypes --force

# Run with specific configuration
.\build.ps1 Compile --configuration Release
```

### Available Targets

| Category | Target | Description |
|----------|--------|-------------|
| **Build** | `Compile` | Build all projects (default) |
| | `Clean` | Clean build outputs |
| | `Restore` | Restore NuGet packages |
| | `Test` | Run all backend tests |
| **Code Generation** | `GenerateAllTypes` | Generate all types (GraphQL + REST merged) |
| | `GenerateGraphQL` | Generate GraphQL schema + TypeScript types |
| | `GenerateApi` | Generate OpenAPI doc + TypeScript types |
| **Database** | `EnsureDatabase` | Start DB container + apply migrations |
| | `UpdateLocalDatabase` | Apply pending EF migrations |
| | `CheckMigrations` | List pending migrations |
| | `CheckDatabaseHealth` | Verify database container is healthy |
| | `AddMigration` | Add a new EF Core migration |
| | `GenerateMigrationScript` | Generate SQL script from latest migration |
| | `ResetDatabase` | Reset local database (destroys all data) |
| | `GenerateDatabaseDocs` | Generate SchemaSpy database documentation |
| **Docker** | `DockerUp` | Start all Docker services |
| | `DockerDown` | Stop all Docker services |
| | `DockerBuild` | Build Docker images |
| | `DockerRestart` | Restart all Docker services |
| **Frontend** | `RestoreFrontend` | Install frontend npm dependencies |
| | `BuildFrontend` | Build frontend for production |
| | `TestFrontend` | Run frontend tests (Vitest) |
| | `LintFrontend` | Run ESLint on frontend |

### Parameters

| Parameter | Description |
|-----------|-------------|
| `--configuration` | Build configuration (`Debug` or `Release`) |
| `--force` | Force regeneration / skip confirmation prompts |
| `--skip-database` | Skip database operations |
| `--db-context` | Target context (`ConsilientDbContext`, `UsersDbContext`, or `Both`) |
| `--migration-name` | Migration name (required for `AddMigration`) |
| `--sequence-number` | Override sequence number for SQL script (1-99) |
| `--database` | Target database name (default: `consilient_main`) |
| `--use-docker` | Use Docker for SchemaSpy (default: true) |
| `--environment` | Environment name for docs (default: `local`) |

---

## Database Operations

### DbContext Options

The codebase has two database contexts:
- **`ConsilientDbContext`** → `consilient_main` database (clinical data, billing, visits)
- **`UsersDbContext`** → `consilient_users` database (identity, authentication)
- **`Both`** → Applies to both contexts (not valid for `AddMigration`)

### Creating a New Migration

```powershell
# Create migration for clinical database
.\build.ps1 AddMigration --migration-name AddPatientNotes --db-context ConsilientDbContext

# Create migration for users database
.\build.ps1 AddMigration --migration-name AddUserPreferences --db-context UsersDbContext
```

**Output locations:**
- ConsilientDbContext → `src/Consilient.Data.Migrations/Consilient/`
- UsersDbContext → `src/Consilient.Data.Migrations/Users/`

### Generating SQL Scripts for Deployment

```powershell
# Generate SQL script for latest migration (auto-numbered)
.\build.ps1 GenerateMigrationScript --db-context ConsilientDbContext

# Generate with specific sequence number
.\build.ps1 GenerateMigrationScript --db-context ConsilientDbContext --sequence-number 06
```

**Output locations:**
- ConsilientDbContext → `src/Databases/consilient_main/XX_MigrationName.sql`
- UsersDbContext → `src/Databases/users_main/XX_MigrationName.sql`

### Applying Migrations Locally

```powershell
# Apply migrations to specific context
.\build.ps1 UpdateLocalDatabase --db-context ConsilientDbContext

# Apply migrations to both contexts
.\build.ps1 UpdateLocalDatabase --db-context Both
```

### Checking Pending Migrations

```powershell
# Check pending migrations for specific context
.\build.ps1 CheckMigrations --db-context ConsilientDbContext

# Check all contexts
.\build.ps1 CheckMigrations --db-context Both
```

### Resetting Local Database

```powershell
# Reset database (requires --force flag)
.\build.ps1 ResetDatabase --force
```

### Complete Workflow Example

When making database schema changes, follow this sequence:

```powershell
# 1. Modify entity in src/Consilient.Data/Entities/
# 2. Update configuration in src/Consilient.Data/Configurations/ if needed

# 3. Create the EF migration
.\build.ps1 AddMigration --migration-name YourMigrationName --db-context ConsilientDbContext

# 4. Review the generated migration file
# Located at: src/Consilient.Data.Migrations/Consilient/[timestamp]_YourMigrationName.cs

# 5. Generate SQL script for CI/CD deployment
.\build.ps1 GenerateMigrationScript --db-context ConsilientDbContext

# 6. Apply migration locally to test
.\build.ps1 UpdateLocalDatabase --db-context ConsilientDbContext

# 7. Commit all generated files:
#    - src/Consilient.Data.Migrations/Consilient/[timestamp]_YourMigrationName.cs
#    - src/Consilient.Data.Migrations/Consilient/[timestamp]_YourMigrationName.Designer.cs
#    - src/Consilient.Data.Migrations/Consilient/ConsilientDbContextModelSnapshot.cs
#    - src/Databases/consilient_main/XX_YourMigrationName.sql
```

### Troubleshooting

**Migration shows "may result in data loss":**
- This warning appears when dropping columns or tables
- Review the migration carefully before applying

**Database connection fails:**
- Ensure Docker is running: `.\build.ps1 DockerUp`
- Check database health: `.\build.ps1 CheckDatabaseHealth`
- Start database if needed: `.\build.ps1 EnsureDatabase`

**Seed data fails with NULL constraint:**
- Check that `seed.sql` provides all NOT NULL columns
- Review table schema in the migration files

**UpdateLocalDatabase fails with connection string errors:**

If `.\build.ps1 UpdateLocalDatabase` fails with "Format of the initialization string does not conform to specification" or similar connection string errors (often caused by special characters like `!` in passwords), apply the migration directly to the Docker container:

```powershell
# 1. Ensure database container is running and healthy
docker compose -f "src/.docker/docker-compose.yml" up -d db
docker inspect consilient.dbs.container --format="{{.State.Health.Status}}"
# Wait until status is "healthy"

# 2. Copy the SQL script into the container
docker cp 'src/Databases/consilient_main/XX_YourMigration.sql' consilient.dbs.container:/tmp/migration.sql

# 3. Execute the migration using the container's internal SA_PASSWORD
# IMPORTANT: Use MSYS_NO_PATHCONV=1 prefix in Git Bash to prevent path mangling
MSYS_NO_PATHCONV=1 docker exec consilient.dbs.container sh -c "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"\$SA_PASSWORD\" -d consilient_main -C -i /tmp/migration.sql"

# 4. Verify the migration was applied
MSYS_NO_PATHCONV=1 docker exec consilient.dbs.container sh -c "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"\$SA_PASSWORD\" -d consilient_main -C -Q \"SELECT TOP 3 MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC\""
```

**Key points for manual migration:**
| Issue | Solution |
|-------|----------|
| Git Bash converts `/opt/...` paths | Add `MSYS_NO_PATHCONV=1` prefix |
| Password with special characters (`!`) | Use `\$SA_PASSWORD` inside `sh -c` so it expands in the container |
| "Login failed for user 'sa'" | The persistent volume may have a different password; always use `$SA_PASSWORD` from inside the container |

---

## API Documentation

### REST API (OpenAPI)

**Location:** `docs/openapi.json`

This file contains the complete OpenAPI 3.0 specification for the Consilient API, including:
- All REST API endpoints and routes (controllers in `src/Consilient.Api/Controllers/`)
- HTTP verbs (GET, POST, PUT, DELETE, PATCH)
- Request/response schemas and data models
- Query parameters, path parameters, and request bodies
- JWT authentication requirements
- Error responses (including ProblemDetails)

**Generation:** `.\build.ps1 GenerateOpenApiDoc` or `.\build.ps1 GenerateAllTypes`

### GraphQL API

**Location:** `docs/schema.graphql`

This file contains the GraphQL Schema Definition Language (SDL) for the EntityGraphQL API, including:
- All GraphQL types (visit, patient, provider, hospitalization, etc.)
- Query definitions with required parameters
- Enum types (ProviderType, SortDirectionEnum)
- Input types for sorting and filtering

**Endpoint:** `/graphql` (with GraphiQL UI at `/ui/graphiql` in development)

**Generation:** `.\build.ps1 GenerateGraphQLSchema` or `.\build.ps1 GenerateAllTypes`

---

## Code Generation Outputs

Run `.\build.ps1 GenerateAllTypes` to regenerate all types.

| Output | Source | Target |
|--------|--------|--------|
| `docs/openapi.json` | REST API controllers | OpenAPI 3.0 spec |
| `docs/schema.graphql` | EntityGraphQL schema | GraphQL SDL |
| `src/Consilient.WebApp2/src/types/api.generated.ts` | Both specs | TypeScript interfaces |

---

## Coding Standards

### C# (.NET 9)
- **C# Version:** 12.0
- **Nullable Reference Types:** Enabled (all projects)
- **Async/Await:** Required for all I/O operations (database, HTTP, file system)
- **Naming:**
  - Controllers: `[EntityName]Controller` (e.g., `PatientsController`)
  - Services: `I[EntityName]Service` interface + `[EntityName]Service` implementation
  - DTOs: Defined in `*.Contracts` projects

### API Patterns
- **Controllers:** Use `[ApiController]` attribute, return `ActionResult<T>`
- **Authentication:** JWT via `[Authorize]` attribute (global policy in production)
- **Validation:** DataAnnotations on DTOs, validated automatically
- **Error Handling:** Return `ProblemDetails` for errors (standard ASP.NET Core)
- **Routing:** Convention: `[Route("api/[controller]")]`

### Database
- **Migrations:** Use Nuke build targets (`.\build.ps1 AddMigration`, `.\build.ps1 UpdateLocalDatabase`)
- **Relationships:** Configured via Fluent API in `src/Consilient.Data/Configuration/`
- **Queries:** Use async methods (`ToListAsync()`, `FirstOrDefaultAsync()`, etc.)
- **Interceptors:** Custom save changes interceptor tracks hospitalization status changes

### Swagger/OpenAPI Configuration
- **Custom Schema IDs:** Readable names for DTOs in `*.Contracts` namespaces
- **Non-nullable Required:** Properties marked as required via `RequireNonNullablePropertiesSchemaFilter`
- **Generics:** Expanded in schema names (e.g., `Result_User`)

---

## Common Workflows

### Adding a New API Endpoint
1. Create/update controller in `src/Consilient.Api/Controllers/`
2. Add service interface in `*.Services/` project
3. Implement service logic
4. Add/update DTO in `*.Contracts/` project
5. Rebuild project → OpenAPI spec and TypeScript types auto-generate
6. Commit `docs/openapi.json` with your changes

### Database Changes
1. Update entity in `src/Consilient.Data/Entities/`
2. Configure relationship in `src/Consilient.Data/Configuration/` if needed
3. Run `.\build.ps1 AddMigration --db-context ConsilientDbContext --migration-name YourMigrationName`
4. Review migration in `src/Consilient.Data.Migrations/`
5. Run `.\build.ps1 UpdateLocalDatabase` to apply locally
6. (Optional) Run `.\build.ps1 GenerateMigrationScript` to generate SQL for deployment
7. Commit migration files

### Background Jobs
- Define jobs in `src/Consilient.Background.Workers/`
- Register in `ServiceCollectionExtensions.AddWorkers()`
- Schedule in `src/Consilient.BackgroundHost/Program.cs`
- Jobs run in separate process with Hangfire dashboard

---

## Environment Configuration

### Local Development
- **Connection strings:** User Secrets or `appsettings.local.json`
- **Azure App Configuration:** Optional (falls back to Key Vault or local settings)
- **HTTPS:** Self-signed certificate via `dotnet dev-certs https --trust`

### Azure Deployment
- **App Service:** Linux containers (.NET 9 runtime)
- **Configuration:** Azure App Configuration with Key Vault references
- **Labels:** `dev` or `prod` based on environment
- **Data Protection:** Ephemeral keys (container restarts lose keys)

---

## Security

- **Authentication:** Required in production (global `[Authorize]` policy)
- **CORS:** Configured from `AllowedOrigins` setting
- **Rate Limiting:** Configured via `ConfigureRateLimiting()`
- **Cookie Policy:** Secure, HttpOnly, SameSite=Strict
- **Redirect Validation:** Validated against allowed origins

---

## Key Dependencies

- **Swashbuckle.AspNetCore:** 9.0.6 (OpenAPI generation)
- **EntityGraphQL:** 5.7.1 (GraphQL)
- **NSwag.MSBuild:** 14.6.3 (TypeScript generation)
- **Hangfire:** 1.8.22 (background jobs)
- **Serilog.AspNetCore:** 9.0.0 (logging)
- **Microsoft.EntityFrameworkCore:** 9.0.x (data access)

---

*This file is the canonical source for AI assistant instructions. Tool-specific files (`src/.github/copilot-instructions.md`, etc.) redirect here.*
