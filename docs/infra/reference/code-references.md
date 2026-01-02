# Code References Index

Comprehensive index of key files and line numbers for AI agent navigation and developer reference.

## Terraform Configuration

### Core Infrastructure Files

| File | Purpose | Key Lines | Details |
|------|---------|-----------|---------|
| [`locals.tf`](../../../infra/terraform/locals.tf) | Single source of truth for all resource configuration | L13-234 | Names, SKUs, costs, settings |
| [`variables.tf`](../../../infra/terraform/variables.tf) | Input variables for environment configuration | L1-50+ | Environment, region, project name, etc. |
| [`backend.tf`](../../../infra/terraform/backend.tf) | State management configuration | L1-30 | Local state (current), remote template (future) |
| [`main.tf`](../../../infra/terraform/main.tf) | Provider configuration and main resources | L1-30 | Azure provider, resource group, tags |

### Resource Definition Files

| File | Resources | Key Sections |
|------|-----------|--------------|
| [`network.tf`](../../../infra/terraform/network.tf) | Virtual Network, Subnets | VNet address spaces, subnet configuration |
| [`api_app.tf`](../../../infra/terraform/api_app.tf) | API App Service, Service Plan | Health checks, Docker deployment |
| [`react_app.tf`](../../../infra/terraform/react_app.tf) | React App Service, Service Plan | Static site hosting, Docker deployment |
| [`sql.tf`](../../../infra/terraform/sql.tf) | SQL Server, Main DB, Hangfire DB | Database creation, security features |
| [`acr.tf`](../../../infra/terraform/acr.tf) | Container Registry | ACR authentication, OIDC setup |
| [`storage.tf`](../../../infra/terraform/storage.tf) | Storage Account for Loki | Blob containers, private endpoints |
| [`loki.tf`](../../../infra/terraform/loki.tf) | Loki Container App, Log Aggregation | Container image, environment config |
| [`grafana.tf`](../../../infra/terraform/grafana.tf) | Grafana Monitoring | Dashboard configuration |

### Modules

| Module | Purpose | Files |
|--------|---------|-------|
| [`modules/app_service/`](../../../infra/terraform/modules/app_service/) | Reusable App Service configuration | main.tf, variables.tf, outputs.tf |
| [`modules/sql_database/`](../../../infra/terraform/modules/sql_database/) | Reusable SQL Database configuration | main.tf, variables.tf, outputs.tf |
| [`modules/storage_account/`](../../../infra/terraform/modules/storage_account/) | Reusable Storage Account configuration | main.tf, variables.tf, outputs.tf |

## Naming & Cost Configuration

| Configuration | Location | Lines | Details |
|---------------|----------|-------|---------|
| **Unique Suffix** | [`locals.tf:17`](../../../infra/terraform/locals.tf#L17) | L17 | MD5 hash of subscription + RG |
| **Default SKUs** | [`locals.tf:24-49`](../../../infra/terraform/locals.tf#L24-L49) | L24-49 | Dev/Staging/Prod tier definitions |
| **App Service Names** | [`locals.tf:66-89`](../../../infra/terraform/locals.tf#L66-L89) | L66-89 | API and React naming patterns |
| **ACR Name** | [`locals.tf:94-98`](../../../infra/terraform/locals.tf#L94-L98) | L94-98 | No-hyphen format for Azure requirement |
| **SQL Naming** | [`locals.tf:104-138`](../../../infra/terraform/locals.tf#L104-L138) | L104-138 | Server and database names |
| **CAE Naming** | [`locals.tf:183-208`](../../../infra/terraform/locals.tf#L183-L208) | L183-208 | Shared vs dedicated environment |
| **Cost Estimates** | [`locals.tf:220-224`](../../../infra/terraform/locals.tf#L220-L224) | L220-224 | Monthly costs by environment |
| **Tags** | [`locals.tf:229-233`](../../../infra/terraform/locals.tf#L229-L233) | L229-233 | Resource tagging strategy |

## GitHub Actions Workflows

### Workflow Files

| File | Purpose | Trigger | Key Jobs |
|------|---------|---------|----------|
| [`.github/workflows/main.yml`](../../../.github/workflows/main.yml) | Orchestrator workflow | push to main | Calls terraform, databases, applications |
| [`.github/workflows/terraform.yml`](../../../.github/workflows/terraform.yml) | Infrastructure deployment | workflow_dispatch, workflow_call | validate, plan, apply, resource import |
| [`.github/workflows/databases.yml`](../../../.github/workflows/databases.yml) | Database deployment | workflow_call | discovery, deploy, verify |
| [`.github/workflows/dotnet_apps.yml`](../../../.github/workflows/dotnet_apps.yml) | .NET API build & deploy | workflow_call | build, publish, deploy, health check |
| [`.github/workflows/react_apps.yml`](../../../.github/workflows/react_apps.yml) | React frontend build & deploy | workflow_call | build, lighthouse, deploy, health check |
| [`.github/workflows/build-runner-image.yml`](../../../.github/workflows/build-runner-image.yml) | Custom runner image | workflow_dispatch | docker build, push to ACR |
| [`.github/workflows/docs_db.yml`](../../../.github/workflows/docs_db.yml) | Database documentation | workflow_call | schema documentation, SchemaSpy |

### Workflow Key Sections

| Workflow | Section | Lines | Details |
|----------|---------|-------|---------|
| **terraform.yml** | Triggers | L5-20 | When workflow runs (push, dispatch, call) |
| **terraform.yml** | Validation | L30-50 | Secret checking, terraform validate |
| **terraform.yml** | Planning | L60-90 | terraform plan with cost estimation |
| **terraform.yml** | Apply | L100-120 | terraform apply and resource import |
| **databases.yml** | Discovery | L45-60 | Find databases from directory structure |
| **databases.yml** | Deployment | L120-150 | Deploy schemas, seed data, verify |
| **dotnet_apps.yml** | Build | L30-50 | Build .NET solution, run tests |
| **dotnet_apps.yml** | Deploy | L85-110 | Push to ACR, configure App Service |
| **react_apps.yml** | Health Check | L130-160 | Lighthouse CI quality gates |
| **main.yml** | Orchestration | L40-80 | Job dependencies and parallelization |

## Composite Actions

### Action Files

| Action | Purpose | Location | Key Lines |
|--------|---------|----------|-----------|
| **azure-login** | Azure authentication (OIDC or Service Principal) | [`.github/actions/azure-login/`](../../../.github/actions/azure-login/) | Cloud: L28-38, Act: L42-58 |
| **validate-inputs** | Input validation for workflows | [`.github/actions/validate-inputs/`](../../../.github/actions/validate-inputs/) | Checks environment, action, paths |
| **debug-variables** | Secure variable logging with masking | [`.github/actions/debug-variables/`](../../../.github/actions/debug-variables/) | Auto-masks secrets in output |
| **sqlcmd-execute** | SQL script execution with timeout | [`.github/actions/sqlcmd-execute/`](../../../.github/actions/sqlcmd-execute/) | Timeout: L20-25, Error handling: L40-60 |

### Action Implementation Details

| Action | Inputs | Outputs | Notes |
|--------|--------|---------|-------|
| **azure-login** | client_id, tenant_id, subscription_id | access_token | Fallback to AZURE_CREDENTIALS for act |
| **validate-inputs** | required_secrets, required_vars | validation_result | Fails workflow if missing |
| **debug-variables** | variables_to_log | masked_output | Auto-detects secret patterns |
| **sqlcmd-execute** | sql_file, timeout, error_handling | execution_result | Default timeout 600s |

## Authentication Configuration

| Component | Location | Purpose | Lines |
|-----------|----------|---------|-------|
| **OIDC Setup** | [`.github/actions/azure-login/action.yml`](../../../.github/actions/azure-login/action.yml) | Federated credential auth | L28-38 |
| **Service Principal Fallback** | [`.github/actions/azure-login/action.yml`](../../../.github/actions/azure-login/action.yml) | Local/act authentication | L42-58 |
| **AZURE_CREDENTIALS Format** | [components/authentication.md](../components/authentication.md#three-tier-architecture) | Service principal JSON | Examples L96-104 |
| **Environment Variables** | [`.github/workflows/terraform.yml`](../../../.github/workflows/terraform.yml) | Terraform auth setup | L60-80 (env section) |

## Database Configuration

### Database Directories

| Directory | Database Name | Purpose | Script Pattern |
|-----------|---------------|---------|-----------------|
| [`src/Databases/Main/`](../../../src/Databases/Main/) | `consilient_main_{env}` | Main application database | `NNN_*.sql` |
| [`src/Databases/Hangfire/`](../../../src/Databases/Hangfire/) | `consilient_hangfire_{env}` | Background job database | `NNN_*.sql` |
| [`src/Databases/CustomDB/`](../../../src/Databases/CustomDB/) | `consilient_customdb_{env}` | Custom database (example) | `NNN_*.sql` |

### Database Script Organization

| Location | Script Type | Pattern | Purpose |
|----------|-------------|---------|---------|
| `src/Databases/{Name}/Schema/` | Schema creation | `001_*.sql`, `002_*.sql`, etc. | Table, index, constraint creation |
| `src/Databases/{Name}/Seed/` | Test data | `seed_*.sql` | Populate test/dev data |
| `src/Databases/{Name}/` | System | `_*.sql` or `.*` | System files (ignored) |

### Database Discovery Workflow

| Step | Location | Purpose | Lines |
|------|----------|---------|-------|
| **Find Databases** | [`.github/workflows/databases.yml:45-60`](../../../.github/workflows/databases.yml#L45-L60) | Scan directories | L45-60 |
| **Create Matrix** | [`.github/workflows/databases.yml:65-75`](../../../.github/workflows/databases.yml#L65-L75) | Parallel deployment | L65-75 |
| **Deploy Schemas** | [`.github/workflows/databases.yml:120-135`](../../../.github/workflows/databases.yml#L120-L135) | Execute SQL scripts | L120-135 |
| **Verify** | [`.github/workflows/databases.yml:140-150`](../../../.github/workflows/databases.yml#L140-L150) | Health checks | L140-150 |

## Local Testing (Act) Configuration

### Act Files

| File | Purpose | Location | Key Lines |
|------|---------|----------|-----------|
| **run-act.ps1** | Main execution script | [`infra/act/run-act.ps1`](../../../infra/act/run-act.ps1) | Config: L119, Command: L718-750 |
| **.env** | Repository variables | [`infra/act/.env`](../../../infra/act/.env) | GitHub Variables simulation |
| **.env.act** | Secrets & credentials | [`infra/act/.env.act`](../../../infra/act/.env.act) | GitHub Secrets simulation (gitignored) |
| **IMPORT_LOOP_FIX.md** | Performance optimization | [`infra/act/IMPORT_LOOP_FIX.md`](../../../infra/act/IMPORT_LOOP_FIX.md) | --bind flag explanation |

### Runner Image

| Component | Location | Purpose | Key Lines |
|-----------|----------|---------|-----------|
| **Dockerfile** | [`.github/workflows/runner/Dockerfile`](../../../.github/workflows/runner/Dockerfile) | Custom runner image | Base: L1, Tools: L10-50 |
| **Tools Installation** | [`.github/workflows/runner/Dockerfile:20-50`](../../../.github/workflows/runner/Dockerfile#L20-L50) | Pre-installed tools | Azure CLI, Terraform, sqlcmd, etc. |
| **Terraform Cache** | [`.github/workflows/runner/Dockerfile:60-70`](../../../.github/workflows/runner/Dockerfile#L60-L70) | Pre-cached providers | azurerm, azuread |

### run-act.ps1 Parameters

| Parameter | Type | Purpose | Example |
|-----------|------|---------|---------|
| `-Environment` | string | Target environment (dev/prod) | `-Environment dev` |
| `-SkipTerraform` | switch | Skip infrastructure deployment | `-SkipTerraform` |
| `-SkipDatabases` | switch | Skip database deployment | `-SkipDatabases` |
| `-RecreateDatabase` | switch | Recreate all DB objects (dev only) | `-RecreateDatabase` |
| `-AllowLocalFirewall` | switch | Enable SQL firewall for local | `-AllowLocalFirewall` |
| `-EnableDebugMode` | switch | Verbose GitHub Actions logging | `-EnableDebugMode` |
| `-SkipHealthChecks` | switch | Skip post-deploy verification | `-SkipHealthChecks` |
| `-NonInteractive` | switch | No prompts (scripted) | `-NonInteractive` |
| `-NoWait` | switch | Don't wait for keypress | `-NoWait` |
| `-RebuildImage` | switch | Force rebuild Docker image | `-RebuildImage` |

## GitHub Variables & Secrets

### Variables (Environment-Specific Configuration)

| Variable | Purpose | Example | Used By |
|----------|---------|---------|---------|
| `AZURE_REGION` | Azure region for resources | `canadacentral` | terraform.yml, all workflows |
| `AZURE_RESOURCE_GROUP_NAME` | Resource group name | `consilient-resource-group` | terraform.yml, databases.yml |
| `ACR_REGISTRY` | Container Registry URL | `consilientacr.azurecr.io` | dotnet_apps.yml, react_apps.yml |
| `PROJECT_NAME` | Project identifier | `consilient` | terraform.yml, all workflows |
| `ENVIRONMENT` | Deployment environment | `dev`, `prod` | All workflows |

### Secrets (Sensitive Configuration)

| Secret | Purpose | Source | Used By |
|--------|---------|--------|---------|
| `AZURE_CLIENT_ID` | OIDC application ID | Entra ID app | azure-login (cloud) |
| `AZURE_TENANT_ID` | Azure AD tenant ID | Azure portal | azure-login, terraform |
| `AZURE_SUBSCRIPTION_ID` | Target Azure subscription | Azure portal | All workflows |
| `ARM_CLIENT_ID` | Terraform service principal | Service principal | terraform provider |
| `ARM_CLIENT_SECRET` | Terraform SP password | Service principal | terraform provider |
| `ARM_TENANT_ID` | Terraform tenant ID | Service principal | terraform provider |
| `SQL_ADMIN_USERNAME` | SQL Server admin user | Generate (dbadmin) | databases.yml |
| `SQL_ADMIN_PASSWORD` | SQL Server admin password | Generate strong password | databases.yml |
| `AZURE_CREDENTIALS` | Act fallback auth JSON | Service principal | azure-login (act only) |

## Application Code Locations

### API (.NET)

| Component | Location | Purpose |
|-----------|----------|---------|
| **API Project** | `src/API/` | .NET Core API application |
| **Dockerfile** | `src/API/Dockerfile` | Container image definition |
| **Health Endpoint** | `src/API/Controllers/HealthController.cs` | `/health` endpoint for checks |
| **Appsettings** | `src/API/appsettings.json` | Configuration by environment |

### React Frontend

| Component | Location | Purpose |
|-----------|----------|---------|
| **React App** | `src/React/` | React application source |
| **Dockerfile** | `src/React/Dockerfile` | Multi-stage build for optimization |
| **Package.json** | `src/React/package.json` | Dependencies and build scripts |
| **Nginx Config** | `src/React/nginx.conf` | Production web server config |

## Documentation Files

### Main Documentation

| File | Purpose | Location |
|------|---------|----------|
| **README.md** | Main navigation hub | `docs/infra/README.md` |
| **QUICK_START.md** | Common tasks guide | `docs/infra/QUICK_START.md` |
| **ARCHITECTURE.md** | System design and flows | `docs/infra/ARCHITECTURE.md` |
| **TROUBLESHOOTING.md** | Error diagnosis and solutions | `docs/infra/TROUBLESHOOTING.md` |
| **KNOWN_ISSUES.md** | Issues and improvements | `docs/infra/KNOWN_ISSUES.md` |

### Component Guides

| File | Purpose | Location |
|------|---------|----------|
| **terraform.md** | Infrastructure configuration | `docs/infra/components/terraform.md` |
| **github-actions.md** | CI/CD workflows | `docs/infra/components/github-actions.md` |
| **azure-resources.md** | Azure services | `docs/infra/components/azure-resources.md` |
| **authentication.md** | Auth architecture | `docs/infra/components/authentication.md` |
| **databases.md** | Database deployment | `docs/infra/components/databases.md` |
| **local-testing.md** | Act tool guide | `docs/infra/components/local-testing.md` |

### Reference Materials

| File | Purpose | Location |
|------|---------|----------|
| **secrets-checklist.md** | Secret configuration reference | `docs/infra/reference/secrets-checklist.md` |
| **naming-conventions.md** | Resource naming patterns | `docs/infra/reference/naming-conventions.md` |
| **cost-management.md** | Cost tiers and optimization | `docs/infra/reference/cost-management.md` |
| **code-references.md** | This file - code index | `docs/infra/reference/code-references.md` |

## Common File Paths by Use Case

### For Adding a New Azure Resource

**Files to modify:**
1. [`infra/terraform/locals.tf`](../../../infra/terraform/locals.tf) - Add naming and SKU config
2. Create new file `infra/terraform/{resource}.tf` - Resource definition
3. [`infra/terraform/main.tf`](../../../infra/terraform/main.tf) - Add resource reference
4. [`.github/workflows/terraform.yml`](../../../.github/workflows/terraform.yml) - Validation steps if needed

### For Adding a New Database

**Files to create:**
1. `src/Databases/{DatabaseName}/` - Create directory
2. `src/Databases/{DatabaseName}/Schema/` - Schema scripts
3. `src/Databases/{DatabaseName}/Seed/` - Test data
4. Auto-discovery will find it automatically

**Files to modify:**
1. [`infra/terraform/locals.tf`](../../../infra/terraform/locals.tf) - Add database name (if Terraform-managed)

### For Modifying Authentication

**Files to understand first:**
1. [`.github/actions/azure-login/action.yml`](../../../.github/actions/azure-login/action.yml) - Auth logic (L28-58)
2. [`docs/infra/components/authentication.md`](../components/authentication.md) - Architecture details

**Files to modify:**
1. [`.github/actions/azure-login/action.yml`](../../../.github/actions/azure-login/action.yml) - Auth implementation
2. [`.github/workflows/terraform.yml`](../../../.github/workflows/terraform.yml) - Env vars setup
3. [`infra/act/.env.act`](../../../infra/act/.env.act) - Local testing secrets

### For Performance Optimization

**Files to review:**
1. [`.github/workflows/databases.yml`](../../../.github/workflows/databases.yml) - Matrix job parallelization
2. [`.github/workflows/main.yml`](../../../.github/workflows/main.yml) - Job dependencies
3. [`infra/act/run-act.ps1:721`](../../../infra/act/run-act.ps1#L721) - `--bind` flag for state persistence
4. [`infra/act/IMPORT_LOOP_FIX.md`](../../../infra/act/IMPORT_LOOP_FIX.md) - Performance details

## Search Patterns for Common Tasks

### Find where a resource is created
```bash
grep -r "resource \"" infra/terraform/ | grep "resource_name"
# Example: grep -r "app_service" infra/terraform/
```

### Find where a variable is used
```bash
grep -r "var\.variable_name" . --include="*.tf" --include="*.yml" --include="*.md"
# Example: grep -r "var.environment" . --include="*.tf"
```

### Find workflow triggers
```bash
grep -A 5 "^on:" .github/workflows/*.yml
```

### Find composite action implementations
```bash
ls -la .github/actions/*/action.yml
```

### Find database scripts
```bash
find src/Databases/ -name "*.sql" | sort
```

---

**Last Updated:** December 2025

**Related Documentation:**
- [README.md](../README.md) - Main navigation hub
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System design
- [components/](../components/) - Detailed component guides
