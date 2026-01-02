# GitHub Actions Workflows & Automation

Complete guide to CI/CD pipelines, GitHub Actions workflows, and composite actions.

## Quick Summary

**7 Active Workflows:**
- `terraform.yml` - Infrastructure provisioning
- `databases.yml` - Database deployment
- `dotnet_apps.yml` - .NET API deployment
- `react_apps.yml` - React frontend deployment
- `main.yml` - Orchestrator (runs all above)
- `build-runner-image.yml` - Custom runner image
- `docs_db.yml` - Database documentation

**4 Composite Actions:**
- `azure-login` - OIDC + service principal auth
- `validate-inputs` - Input validation
- `debug-variables` - Secure variable logging
- `sqlcmd-execute` - SQL script execution

See original [`COMPOSITE_ACTIONS_GUIDE.md`](../../COMPOSITE_ACTIONS_GUIDE.md) and [`README.md`](../../README.md) for detailed information.

## Architecture

```
GitHub Event
    ↓
main.yml (Orchestrator)
    ├─ terraform.yml (Infrastructure)
    ├─ databases.yml (Database deployment)
    ├─ dotnet_apps.yml (API deployment)
    └─ react_apps.yml (React deployment)
```

**Key Characteristics:**
- Sequential execution (wait for previous job)
- Automatic rollback on health check failure
- Matrix jobs for parallel database deployment
- Custom runner with pre-installed tools

See [ARCHITECTURE.md#cicd-pipeline-flow](../ARCHITECTURE.md#cicd-pipeline-flow) for detailed flow diagram.

## Terraform Workflow

**File:** [`.github/workflows/terraform.yml`](../../../.github/workflows/terraform.yml)

**Triggers:**
- Push to `main` branch
- Manual workflow dispatch
- Called from `main.yml`

**Key Jobs:**
1. Validate inputs (environment, action)
2. Azure login (OIDC)
3. Terraform init/validate/plan/apply
4. Resource import (for existing Azure resources)
5. Output results

**Inputs:**
- `environment` (dev/prod)
- `action` (plan/apply/destroy)
- `allow-local-firewall` (for act)
- `enable-debug` (verbose logging)

**Outputs:**
- `api_app_name` - API App Service name
- `react_app_name` - React App Service name
- `acr_registry_url` - Container Registry URL

See [`INFRASTRUCTURE.md`](../../INFRASTRUCTURE.md) for detailed Terraform workflow documentation.

## Database Workflow

**File:** [`.github/workflows/databases.yml`](../../../.github/workflows/databases.yml)

**Process:**
1. Auto-discovery finds databases in [`src/Databases/`](../../../src/Databases/)
2. Creates naming: `{folder}_{environment}`
3. Matrix job deploys each database (parallel)
4. Azure AD authentication (sqlcmd -G)
5. Executes SQL scripts in order
6. Verifies deployment

**Auto-Discovery Pattern:**
- Discovers directories in `src/Databases/`
- Creates database for each directory
- Naming: `Main` → `consilient_main_dev`

See [components/databases.md](databases.md) for complete database guide.

## Application Deployment

**API (.NET):** [`.github/workflows/dotnet_apps.yml`](../../../.github/workflows/dotnet_apps.yml)
- Docker build from [`src/Consilient.Api/Dockerfile`](../../../src/Consilient.Api/Dockerfile)
- Push to ACR
- Deploy to App Service
- Health check: `/health` endpoint with database check
- Automatic rollback on failure

**React:** [`.github/workflows/react_apps.yml`](../../../.github/workflows/react_apps.yml)
- Docker build from [`src/Consilient.WebApp2/Dockerfile`](../../../src/Consilient.WebApp2/Dockerfile)
- Push to ACR
- Deploy to App Service
- Health check: Lighthouse CI quality gates
- Automatic rollback on failure

## Database Documentation Workflow

**File:** [`.github/workflows/docs_db.yml`](../../../.github/workflows/docs_db.yml)

**Purpose:** Auto-generate interactive HTML documentation for database schemas using SchemaSpy

**Trigger:**
- Called from `main.yml` on pull requests (after database deployment)
- Manual trigger: GitHub Actions UI → "05 - Generate DB Docs" → Run workflow
- Skippable via `skip_db_docs` input to main.yml

**Process:**
1. **Extract Database Names** - Parse discovered databases
2. **Validate Prerequisites** - Check SQL Server, sqlcmd, java, SchemaSpy available
3. **Generate Docs (Matrix Job)** - For each database:
   - Parse `db_docs.yml` configuration
   - Query database for schemas (`list_user_schemas.sql`)
   - Filter excluded schemas (from `schemas.exclude` list)
   - Run SchemaSpy in parallel for each schema
   - Generate HTML documentation
   - Create index.html with schema navigation

**Configuration:**
Per-database control via `src/Databases/{Name}/db_docs.yml`:
- `database.generate_docs` - Enable/disable documentation
- `schemas.exclude` - Schemas to skip in documentation

**Output:**
- Artifact: `database-documentation-{database}-{suffix}.zip`
- Contents: Interactive HTML docs with diagrams, table details, relationships
- Retention: 7 days (PR), 30 days (manual/main.yml)

See [components/database-documentation.md](database-documentation.md) for comprehensive guide.

## Composite Actions

### azure-login

**Location:** [`.github/actions/azure-login/`](../../../.github/actions/azure-login/)

**Purpose:** Dual authentication (OIDC for cloud, service principal for act)

**Behavior:**
- Cloud: Uses `azure/login` with OIDC
- Local (act): Parses JSON credentials, uses service principal

**Code Reduction:** 23 lines → 8 lines per usage (65% reduction)

See [`authentication.md`](authentication.md) for detailed auth guide.

### validate-inputs

**Location:** [`.github/actions/validate-inputs/`](../../../.github/actions/validate-inputs/)

**Purpose:** Validate environment, action, paths before execution

**Validations:**
- Environment: dev/prod/staging
- Action: plan/apply/destroy
- Paths exist
- Required fields set

### debug-variables

**Location:** [`.github/actions/debug-variables/`](../../../.github/actions/debug-variables/)

**Purpose:** Display variables with automatic secret masking

**Features:**
- Auto-masks: password, secret, token, key, credential
- Truncates long values
- JSON formatting

### sqlcmd-execute

**Location:** [`.github/actions/sqlcmd-execute/`](../../../.github/actions/sqlcmd-execute/)

**Purpose:** Execute SQL scripts with error handling

**Features:**
- Azure AD authentication
- Timeout enforcement (default 600s)
- Error log capture
- Fail-on-error flag

## GitHub Variables Configuration

**Required Variables (9 total):**

| Variable | Purpose |
|----------|---------|
| AZURE_REGION | Azure region (default: canadacentral) |
| AZURE_RESOURCE_GROUP_NAME | Resource group name |
| AZURE_SQL_SERVER_FQDN | SQL server address |
| ACR_REGISTRY_URL | Container registry URL |
| API_APP_NAME | API App Service name |
| REACT_APP_NAME | React App Service name |
| ACTIONS_RUNNER_IMAGE | Custom runner image URL |
| SQL_SERVER_VERSION | SQL Server Docker version |
| SCHEMASPY_VERSION | SchemaSpy version |

**Setup:** GitHub Settings → Variables and Secrets → Variables (repository level)

See [`GITHUB_VARIABLES_SETUP.md`](../../GITHUB_VARIABLES_SETUP.md) for detailed setup.

## Custom Runner Image

**Build Workflow:** [`.github/workflows/build-runner-image.yml`](../../../.github/workflows/build-runner-image.yml)

**Dockerfile:** [`.github/workflows/runner/Dockerfile`](../../../.github/workflows/runner/Dockerfile)

**Pre-Installed Tools:**
- Azure CLI (latest)
- sqlcmd v1.6.0
- Terraform v1.9.0 with pre-cached providers
- Java 17 JRE
- SchemaSpy v6.2.4
- Node.js 20, Docker CLI, git, jq

**Benefits:**
- All required tools pre-installed
- Faster workflow execution
- Terraform providers cached (no download)
- Consistent environment across runs

**Build Time:** ~5-10 min (first time), cached after

## Best Practices

1. **Always Plan Before Apply**
   ```powershell
   terraform plan -out=tfplan
   # Review output before applying
   terraform apply tfplan
   ```

2. **Use Composite Actions**
   - Reduces code duplication
   - Standardizes best practices
   - Easier testing and maintenance

3. **Test Locally with Act**
   - Validate workflows before push
   - Save GitHub Actions minutes
   - See [components/local-testing.md](local-testing.md)

4. **Monitor Health Checks**
   - API checks `/health` endpoint
   - React checks Lighthouse CI quality gates
   - Failures trigger automatic rollback

5. **Secret Management**
   - Use GitHub Secrets (encrypted)
   - Never hardcode in workflows
   - Rotate quarterly
   - See [components/authentication.md](authentication.md)

## Troubleshooting

See [TROUBLESHOOTING.md#github-actions-failures](../TROUBLESHOOTING.md#github-actions-failures) for:
- Secret validation errors
- OIDC authentication fails
- Workflow trigger issues

## Related Documentation

- [ARCHITECTURE.md](../ARCHITECTURE.md) - Pipeline flow diagrams
- [components/authentication.md](authentication.md) - Auth details
- [components/local-testing.md](local-testing.md) - Act tool guide
- [COMPOSITE_ACTIONS_GUIDE.md](../../COMPOSITE_ACTIONS_GUIDE.md) - Detailed guide
- [README.md](../../README.md) - Composite actions reference

---

**Last Updated:** December 2025
**For Navigation:** See [README.md](../README.md)
