# Consilient Web Application - AI Assistant Instructions

## API Documentation

**OpenAPI Specification Location:** `docs/openapi.json` **(at repository root, NOT in src/)**

This file contains the complete OpenAPI 3.0 specification for the Consilient API, including:
- All REST API endpoints and routes (controllers in `Consilient.Api/Controllers/`)
- HTTP verbs (GET, POST, PUT, DELETE, PATCH)
- Request/response schemas and data models
- Query parameters, path parameters, and request bodies
- JWT authentication requirements
- Error responses (including ProblemDetails)

**Generation:** The OpenAPI spec is automatically generated during rebuild via Swashbuckle CLI from the compiled `Consilient.Api` project. The generation script is at `Scripts/openapi-generation/Generate-OpenApiDoc.ps1`.

**Usage:** When discussing API endpoints, parameters, or schemas, always refer to this file for accurate, current definitions.

## Project Structure

### Backend (.NET 9)
- **`Consilient.Api/`** - Main ASP.NET Core Web API
  - Controllers: REST API endpoints
  - Init: Service registration and configuration
  - Hubs: SignalR real-time communication
- **`Consilient.BackgroundHost/`** - Hangfire background job processing
- **`Consilient.Data/`** - Entity Framework Core DbContext and migrations
- **`*.Services/`** - Business logic layer (Employee, Patient, Visit, etc.)
- **`*.Contracts/`** - DTOs and contract types for API communication

### Frontend
- **`Consilient.WebApp2/`** - React 18 TypeScript SPA

### Infrastructure
- **Database:** SQL Server with EF Core 9
- **Background Jobs:** Hangfire with SQL Server storage
- **Authentication:** JWT bearer tokens via custom User Service
- **Configuration:** Azure App Configuration with Key Vault integration
- **Logging:** Serilog with structured logging

## Code Generation Scripts

### OpenAPI Documentation
**Script:** `Scripts/openapi-generation/Generate-OpenApiDoc.ps1`
**Trigger:** Automatically runs after rebuild in local development
**Output:** `docs/openapi.json`

### TypeScript Types
**Script:** `Scripts/typescript-type-generation/Generate-ApiTypes.ps1`
**Trigger:** Automatically runs after rebuild in local development
**Output:** TypeScript interfaces for frontend consumption
**Config:** `Consilient.Api/nswag.json`

Both scripts use assembly reflection (no app startup) and run via MSBuild targets in `Consilient.Api.csproj`.

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
- **Migrations:** Use EF Core migrations (`Add-Migration`, `Update-Database`)
- **Relationships:** Configured via Fluent API in `Consilient.Data/Configuration/`
- **Queries:** Use async methods (`ToListAsync()`, `FirstOrDefaultAsync()`, etc.)
- **Interceptors:** Custom save changes interceptor tracks hospitalization status changes

### Swagger/OpenAPI Configuration
- **Custom Schema IDs:** Readable names for DTOs in `*.Contracts` namespaces
- **Non-nullable Required:** Properties marked as required via `RequireNonNullablePropertiesSchemaFilter`
- **Generics:** Expanded in schema names (e.g., `Result_User`)

## Common Workflows

### Adding a New API Endpoint
1. Create/update controller in `Consilient.Api/Controllers/`
2. Add service interface in `*.Services/` project
3. Implement service logic
4. Add/update DTO in `*.Contracts/` project
5. Rebuild project ? OpenAPI spec and TypeScript types auto-generate
6. Commit `docs/openapi.json` with your changes

### Database Changes
1. Update entity in `Consilient.Data/Entities/`
2. Configure relationship in `Consilient.Data/Configuration/` if needed
3. Run `Add-Migration [MigrationName]` in Package Manager Console
4. Review migration, then `Update-Database`
5. Commit migration files

### Background Jobs
- Define jobs in `Consilient.Background.Workers/`
- Register in `ServiceCollectionExtensions.AddWorkers()`
- Schedule in `Consilient.BackgroundHost/Program.cs`
- Jobs run in separate process with Hangfire dashboard

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

## Security

- **Authentication:** Required in production (global `[Authorize]` policy)
- **CORS:** Configured from `AllowedOrigins` setting
- **Rate Limiting:** Configured via `ConfigureRateLimiting()`
- **Cookie Policy:** Secure, HttpOnly, SameSite=Strict
- **Redirect Validation:** Validated against allowed origins

## GraphQL

GraphQL endpoint available at `/graphql` with GraphiQL UI at `/ui/graphiql` (development only).

## Key Dependencies

- **Swashbuckle.AspNetCore:** 9.0.6 (OpenAPI generation)
- **NSwag.MSBuild:** 14.6.3 (TypeScript generation)
- **Hangfire:** 1.8.22 (background jobs)
- **Serilog.AspNetCore:** 9.0.0 (logging)
- **Microsoft.EntityFrameworkCore:** 9.0.x (data access)
