# Consilient Web Application - AI Assistant Instructions

> **For AI Assistants:** This is the canonical documentation for understanding and working with this codebase. Tool-specific files redirect here.

## Quick Reference

| Resource | Location | Purpose |
|----------|----------|---------|
| REST API Spec | [`docs/openapi.json`](docs/openapi.json) | OpenAPI 3.0 specification |
| GraphQL Schema | [`docs/schema.graphql`](docs/schema.graphql) | GraphQL SDL |
| API Source | [`src/Consilient.Api/`](src/Consilient.Api/) | Controllers, configuration |
| Frontend | [`src/Consilient.WebApp2/`](src/Consilient.WebApp2/) | React TypeScript SPA |

---

## API Documentation

### REST API (OpenAPI)

**Location:** `docs/openapi.json` (at repository root)

This file contains the complete OpenAPI 3.0 specification for the Consilient API, including:
- All REST API endpoints and routes (controllers in `src/Consilient.Api/Controllers/`)
- HTTP verbs (GET, POST, PUT, DELETE, PATCH)
- Request/response schemas and data models
- Query parameters, path parameters, and request bodies
- JWT authentication requirements
- Error responses (including ProblemDetails)

**Generation:** Auto-generated via Nuke build system. Run `build.cmd GenerateOpenApiDoc` or `build.cmd GenerateAllTypes`.

**Usage:** When discussing API endpoints, parameters, or schemas, always refer to this file for accurate, current definitions.

### GraphQL API

**Location:** `docs/schema.graphql` (at repository root)

This file contains the GraphQL Schema Definition Language (SDL) for the EntityGraphQL API, including:
- All GraphQL types (visit, patient, provider, hospitalization, etc.)
- Query definitions with required parameters
- Enum types (ProviderType, SortDirectionEnum)
- Input types for sorting and filtering

**Endpoint:** `/graphql` (with GraphiQL UI at `/ui/graphiql` in development)

**Generation:** Auto-generated via Nuke build system. Run `build.cmd GenerateGraphQLSchema` or `build.cmd GenerateAllTypes`.

**Usage:** When discussing GraphQL queries, types, or schema structure, refer to this file for accurate, current definitions.

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

The repository uses a NUKE build system via `build.cmd` at the repository root. This is the primary way to build, test, and generate code.

### Usage

```bash
# Run the default target (Compile)
build.cmd

# Run a specific target
build.cmd GenerateAllTypes

# Force regeneration even if outputs are up-to-date
build.cmd GenerateAllTypes --force

# Run with specific configuration
build.cmd Compile --configuration Release
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

## Code Generation Outputs

Code generation is handled by the NUKE build system. Run `build.cmd GenerateAllTypes` to regenerate all types.

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
- **Migrations:** Use Nuke build targets (`build.cmd AddMigration`, `build.cmd UpdateLocalDatabase`)
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
3. Run `build.cmd AddMigration --db-context ConsilientDbContext --migration-name YourMigrationName`
4. Review migration in `src/Consilient.Data.Migrations/`
5. Run `build.cmd UpdateLocalDatabase` to apply locally
6. (Optional) Run `build.cmd GenerateMigrationScript` to generate SQL for deployment
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
