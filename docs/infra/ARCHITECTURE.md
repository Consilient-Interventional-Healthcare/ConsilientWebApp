# Infrastructure Architecture

<!-- AI_CONTEXT: High-level system architecture for Consilient web application. Two environments: dev ($45/mo) and prod ($2,800/mo). No staging environment. -->

## For Non-Technical Stakeholders

This document describes how the Consilient application infrastructure works. The infrastructure automatically builds, tests, and deploys code changes to Azure cloud. Everything is automated through GitHub Actions workflows, meaning developers push code and the system handles the rest. No manual deployment steps are required.

---

High-level system design, component relationships, and data flow diagrams.

## High-Level Architecture

```
┌─────────────────┐
│  GitHub Events  │ (Push, PR, Manual Dispatch)
└────────┬────────┘
         │
         ▼
┌──────────────────────────┐
│  GitHub Actions Workflows│
│  (main.yml orchestrates) │
└────────┬─────────────────┘
         │
         ├──► terraform.yml      ─────┐
         ├──► databases.yml      ─────┤
         ├──► dotnet-apps.yml    ─────┤
         └──► react-apps.yml     ─────┤
                                      │
                                      ▼
                        ┌─────────────────────────┐
                        │  Azure Infrastructure   │
                        │  (Terraform Managed)    │
                        └──────────┬──────────────┘
                                   │
                    ┌──────────────┼──────────────┐
                    │              │              │
                    ▼              ▼              ▼
            ┌──────────────┐ ┌──────────┐ ┌──────────────┐
            │  Compute     │ │ Database │ │   Storage    │
            │ - API App    │ │ - SQL DB │ │ - ACR Images │
            │ - React App  │ │ - Backup │ │ - Loki Logs  │
            └──────────────┘ └──────────┘ └──────────────┘
```

**Components:**
- **GitHub Actions:** CI/CD orchestration and workflow automation
- **Terraform:** Infrastructure as Code (IaC) for Azure resource management
- **Azure:** Cloud infrastructure for compute, database, and storage

---

## CI/CD Pipeline Flow

```
GitHub Event (push, workflow_dispatch, schedule)
    │
    ▼
main.yml (Orchestrator Workflow)
    │
    ├─ validate-environment
    │   └─ Check environment is dev/prod
    │
    ├─ terraform job
    │   ├─ terraform.yml (called workflow)
    │   ├─ Steps: init → validate → plan → apply
    │   ├─ Outputs: api_app_name, react_app_name, acr_registry_url
    │   └─ Resource import for existing resources
    │
    ├─ deploy-databases job (depends on: terraform)
    │   ├─ databases.yml (called workflow)
    │   ├─ Auto-discovery: finds databases in src/Databases/
    │   ├─ Matrix job: one job per database (parallel execution)
    │   ├─ Azure AD authentication (sqlcmd)
    │   └─ Verify: table counts, schema validation
    │
    ├─ deploy-dotnet-apps job (depends on: deploy-databases)
    │   ├─ dotnet-apps.yml
    │   ├─ Steps: Docker build → ACR push → Azure deploy
    │   ├─ Health check: API /health endpoint
    │   └─ Automatic rollback on failure
    │
    └─ deploy-react-apps job (depends on: deploy-databases)
        ├─ react-apps.yml
        ├─ Steps: Docker build → ACR push → Azure deploy
        ├─ Health check: Lighthouse CI quality gates
        └─ Automatic rollback on failure
```

**Key Characteristics:**
- **Sequential:** Each job waits for previous to complete
- **Conditional:** Skip jobs based on inputs (skip_terraform, skip_databases)
- **Automatic Rollback:** Health check failures trigger automatic rollback
- **Matrix Jobs:** Databases deployed in parallel for speed
- **Custom Container:** All jobs run in custom runner with pre-installed tools

See [components/github-actions.md](components/github-actions.md) for detailed workflow documentation.

---

## Authentication Flow

### Cloud Execution (GitHub Actions)

```
GitHub Actions
    │
    ├─ Environment Variable: ACT not set
    │
    ├─ azure/login v2.3.0
    │  └─ OIDC Authentication
    │     ├─ client-id: AZURE_CLIENT_ID
    │     ├─ tenant-id: AZURE_TENANT_ID
    │     └─ subscription-id: AZURE_SUBSCRIPTION_ID
    │
    └─ Terraform Provider
       └─ Service Principal
          ├─ ARM_CLIENT_ID
          ├─ ARM_CLIENT_SECRET
          └─ ARM_TENANT_ID
```

**Why Two Different Authentication Methods?**
- **OIDC (azure/login):** For Azure CLI commands (e.g., `az account set`)
- **Service Principal (ARM_*):** For Terraform provider (better for IaC)

### Local Testing with Act

```
.\run-act.ps1
    │
    ├─ Environment Variable: ACT = true
    │
    └─ Composite Action: azure-login
       └─ Service Principal Fallback
          ├─ Parse AZURE_CREDENTIALS JSON
          ├─ Extract: clientId, clientSecret, tenantId
          └─ az login --service-principal
```

**Why Service Principal for Local?**
- OIDC requires GitHub's OIDC provider (not available locally)
- Service Principal works with Docker containers via JSON credentials

See [components/authentication.md](components/authentication.md) for complete authentication guide.

---

## Database Deployment Flow

```
src/Databases/ Directory Structure
    │
    ├─ Main/
    │   ├─ Schema/        ← SQL scripts (001_, 002_, etc.)
    │   └─ Seeds/         ← Test data (seed_*.sql)
    │
    ├─ Hangfire/
    │   └─ Schema/
    │
    └─ CustomDB/
        └─ Schema/

              ▼

databases.yml Workflow

1. discover-databases job
   └─ Find all directories in src/Databases/
   └─ Output: JSON array of database names

              ▼

2. deploy-database job (matrix per database)
   ├─ Azure AD Login
   ├─ List SQL files in order
   │  ├─ Include: Schema scripts (001, 002, etc.)
   │  └─ Exclude: Seed files (unless recreate)
   │
   ├─ For each SQL file:
   │  ├─ Execute with sqlcmd (Azure AD auth)
   │  └─ Timeout: 600 seconds per script
   │
   └─ Verify:
      ├─ List tables
      ├─ Count records
      └─ Check constraints
```

**Database Naming Pattern:**
- Directory: `Main` → Database: `consilient_main_dev`, `consilient_main_prod`
- Directory: `Hangfire` → Database: `consilient_hangfire_dev`
- Directory: `CustomDB` → Database: `consilient_customdb_dev`

**Script Execution:**
- Schema scripts: Always execute (numbered order: 001, 002, etc.)
- Seed scripts: Only if `recreate_database_objects=true` (dev only)
- System scripts: Skip (start with `_` or `.`)

See [components/databases.md](components/databases.md) for detailed database guide.

---

## Database Documentation Generation Flow

```
src/Databases/ Directory Structure
    │
    ├─ consilient_main/
    │   └─ *.sql           ← SQL scripts
    │
    └─ users_main/
        └─ *.sql

              ▼

discover-databases Action

1. Scan src/Databases/ for directories
2. Output: database list for documentation workflow

              ▼

database-docs.yml Workflow (Called from main.yml on PR)

1. extract-databases job
   └─ Extract database names

              ▼

2. validate-prerequisites job
   └─ Check AZURE_SQL_SERVER secret
   └─ Verify: sqlcmd, jq, java available

              ▼

3. generate-db-docs job (matrix per database)
   ├─ Discover Schemas (list_user_schemas.sql)
   │  └─ Query database for user-created schemas
   │  └─ Auto-exclude: sys, INFORMATION_SCHEMA, guest
   │
   ├─ For each schema (parallel execution):
   │  ├─ SchemaSpy generates HTML documentation
   │  │  └─ Timeout: 600 seconds per schema
   │  └─ Output: docs/{schema_name}/ directory
   │
   └─ Create HTML Index
      ├─ docs/index.html (landing page)
      └─ List all schema documentation

              ▼

Output Artifacts

- Artifact name: database-documentation-{name}-{suffix}
- Contents: docs/ folder with all HTML documentation
- Retention: 7 days (PR), 30 days (manual/main.yml)
```

**Parallel Execution:**
All schemas processed simultaneously for performance:
- Single schema: 2-3 minutes
- 3 schemas in parallel: 2-3 minutes (same time!)
- Without parallelization: 6-9 minutes

See [components/database-documentation.md](components/database-documentation.md) for complete documentation guide.

---

## Resource Dependencies

### Terraform Module Dependencies

```
locals.tf (Configuration)
    │
    ├─ main.tf
    │  └─ azurerm_resource_group
    │     └─ Common location/tags for all resources
    │
    ├─ network.tf
    │  └─ azurerm_virtual_network
    │  └─ azurerm_subnet
    │     └─ Used by: App Services, SQL Server
    │
    ├─ acr.tf
    │  └─ azurerm_container_registry
    │     └─ Used by: dotnet-apps.yml, react-apps.yml
    │
    ├─ sql.tf
    │  └─ azurerm_sql_server
    │  └─ azurerm_sql_database (Main, Hangfire)
    │     └─ Used by: databases.yml
    │
    ├─ api_app.tf (Module: app_service)
    │  └─ azurerm_app_service_plan
    │  └─ azurerm_app_service (Linux)
    │     └─ Depends on: network, acr
    │
    └─ react_app.tf (Module: app_service)
       └─ azurerm_app_service_plan
       └─ azurerm_app_service (Linux)
          └─ Depends on: network, acr
```

### Azure Resource Dependencies

```
Resource Group
    │
    ├─ Virtual Network (10.10.0.0/16)
    │  └─ Subnet (10.10.1.0/24)
    │     └─ App Service Plans (API, React)
    │        └─ App Services (API, React) ◄── Docker containers
    │
    ├─ Container Registry (ACR)
    │  └─ API image: consilientapi:vN-{sha}
    │  └─ React image: consilient-react:vN-{sha}
    │
    ├─ SQL Server
    │  ├─ Database: consilient_main_{env}
    │  ├─ Database: consilient_hangfire_{env}
    │  └─ Firewall rules (for local testing)
    │
    ├─ Storage Account (Loki logs)
    │  ├─ Private Endpoint (secure access)
    │  └─ Blob Containers
    │
    └─ Container App Environment
       └─ Container App: Loki
          └─ Monitoring/Log Aggregation
```

### Deployment Sequence

```
1. Terraform creates infrastructure
   ├─ Resource Group
   ├─ Network (VNet, Subnet)
   ├─ SQL Server & Databases (empty)
   ├─ ACR (empty)
   └─ App Services (no containers yet)

2. Database scripts deploy
   └─ Populate consilient_main_{env}
   └─ Populate consilient_hangfire_{env}

3. .NET API deploys
   ├─ Docker build & push to ACR
   └─ Update App Service to use new image

4. React app deploys
   ├─ Docker build & push to ACR
   └─ Update App Service to use new image

5. Health checks verify
   ├─ API: Check /health endpoint
   ├─ React: Lighthouse CI quality gates
   └─ Rollback if checks fail
```

---

## State Management

### Terraform State

**Location:** `infra/terraform/terraform.tfstate` (local state for dev)

**For Production:** Should use Azure Storage backend (see [`backend.tf`](../../../infra/terraform/backend.tf))

**Important:** State file contains sensitive data
- Never commit to Git
- Already in `.gitignore`
- Should use remote backend in production

### GitHub Actions State

**Maintained by:** Workflow runs and job outputs
- Job outputs: Passed from terraform → downstream jobs
- Example: `${{ needs.terraform.outputs.api_app_name }}`
- Ephemeral: Lost when workflow completes

---

## Security Architecture

### Authentication Tiers

```
Tier 1: OIDC (Cloud - GitHub Actions to Azure)
├─ No long-lived secrets
├─ Federated credentials
└─ Most secure

Tier 2: Service Principal (Terraform)
├─ Client ID + Secret
├─ Required for Terraform provider
└─ Moderate security (secret management required)

Tier 3: Fallback (Local Testing - act)
├─ Service Principal JSON
├─ Only needed for local development
└─ Requires careful credential management
```

### Network Security

- **Private Endpoints:** Storage Account access via private endpoint
- **VNet Integration:** App Services in VNet for internal communication
- **SQL Firewall:** Restrictive firewall rules
- **Azure AD:** SQL authentication (no SQL username/password in code)

### Secret Management

- **GitHub Secrets:** 8 required + 1 optional
- **No Hardcoding:** All secrets injected at runtime
- **Rotation:** Plan secret rotation schedule
- **Audit:** Enable Azure Activity Log for secret access

See [components/authentication.md](components/authentication.md) for complete authentication guide.

---

## Cost Optimization

### Two-Tier Environment Strategy

<!-- AI_CONTEXT: Two environments only. Code validation in variables.tf:27-28 allows only "dev" or "prod". No staging environment exists. -->

| Environment | SKUs | Monthly Cost | Purpose |
|-------------|------|-------------|---------|
| Development | Basic | ~$45 | Fast iteration, minimal cost |
| Production | Premium | ~$2,800 | High availability, 24/7 operations |

<!-- AI_NOTE: Previous documentation mentioned "staging" but code validation only supports dev/prod. Development cost was corrected from $200 to $45 based on actual locals.tf configuration. -->

**Cost Drivers:**
- App Service Plans: Largest cost (Basic $13/mo → Premium $204/mo)
- SQL Database: Basic DTU for dev (~$5/mo), GP_Gen5_4 for prod (~$1,300/mo)
- Storage: Minimal unless logging enabled

See [reference/cost-management.md](reference/cost-management.md) for optimization strategies.

---

## Scalability Considerations

### Horizontal Scaling

- **App Services:** Multi-instance App Service Plans (scale-out)
- **Database:** Read replicas, geo-replication
- **Storage:** Automatic scaling with Azure Storage

### Vertical Scaling

- **App Services:** Premium SKUs with more resources
- **Database:** DTU/vCore allocation increase
- **Storage:** Higher tier replication (GRS, GZRS)

---

## Related Documentation

- [components/terraform.md](components/terraform.md) - Terraform infrastructure details
- [components/github-actions.md](components/github-actions.md) - Workflow documentation
- [components/authentication.md](components/authentication.md) - Security details
- [components/databases.md](components/databases.md) - Database deployment
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) - Common issues and solutions

---

**Last Updated:** December 2025
**For Navigation:** See [README.md](README.md)
