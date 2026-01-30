# Workflows Quick Reference

<!-- AI_CONTEXT: Catalog of all GitHub Actions workflows. Files have descriptive names without number prefixes. Two environments: dev and prod. -->

## Active Workflows

<!-- AI_TABLE: All workflow files in .github/workflows/ directory -->

| Workflow | File | Triggers | Primary Input | Duration | Purpose |
|----------|------|----------|---|----------|---------|
| Infrastructure | terraform.yml | push (main), workflow_dispatch | environment, skip_terraform | ~5 min | Provision Azure resources via Terraform |
| Databases | databases.yml | Called by main.yml | environment, recreate_database_objects | ~3 min | Deploy SQL schemas with matrix parallelization |
| .NET API | dotnet-apps.yml | Called by main.yml | environment, api_app_name, acr_registry_url | ~7 min | Build and deploy API Docker containers |
| React App | react-apps.yml | Called by main.yml | environment, react_app_name, acr_registry_url | ~8 min | Build and deploy React SPA containers |
| Orchestrator | main.yml | push (main), workflow_dispatch | environment, skip_terraform, skip_databases | ~20 min | Master workflow orchestrating all jobs |
| Runner Image | build-runner-image.yml | workflow_dispatch, schedule | - | ~15 min | Build custom Docker runner with pre-installed tools |
| DB Documentation | database-docs.yml | Called by main.yml, workflow_dispatch | environment, database_configs | ~5 min | Generate SchemaSpy HTML documentation |

<!-- AI_NOTE: GitHub Actions UI may display numbered names (e.g., "05 - Generate DB Docs") but actual YAML files use descriptive names without prefixes. -->

---

## Workflow Dependencies

```
main.yml (Orchestrator - Triggered by: push main, manual dispatch)
│
├──► terraform.yml (Infrastructure provisioning)
│    └─ Outputs: api_app_name, react_app_name, acr_registry_url
│
├──► databases.yml (Database deployment - depends_on: terraform)
│    ├─ Discovers databases from src/Databases/
│    └─ Matrix job: parallel per database
│
├──► dotnet-apps.yml (API deployment - depends_on: databases)
│    ├─ Docker build
│    ├─ Push to ACR
│    └─ Deploy to App Service + health check
│
└──► react-apps.yml (React deployment - depends_on: databases)
     ├─ Docker build
     ├─ Push to ACR
     ├─ Deploy to App Service
     └─ Lighthouse quality gates

database-docs.yml (Documentation - Called from main.yml on PR)
│
├─ Discovers databases from src/Databases/
├─ Generates SchemaSpy docs per schema
└─ Uploads artifacts
```

---

## Composite Actions

<!-- AI_TABLE: Reusable composite actions used by workflows -->

| Action Name | File | Purpose | Used By | Lines Saved |
|-------------|------|---------|---------|------------|
| Azure Login | .github/actions/azure-login | OIDC + Service Principal fallback authentication | All workflows | ~15/workflow |
| Validate Inputs | .github/actions/validate-inputs | Environment and input validation | All workflows | ~10/workflow |
| Debug Variables | .github/actions/debug-variables | Output debug information | Terraform, Databases | ~20/workflow |
| SQL Execute | .github/actions/sqlcmd-execute | Execute SQL scripts with retry logic | databases.yml | ~30 |
| Discover Databases | .github/actions/discover-databases | Auto-discover databases in src/Databases/ | databases.yml, database-docs.yml | ~40 |
| Initialize | .github/actions/initialize | Workflow initialization and setup | All workflows | ~15/workflow |
| Health Check API | .github/actions/health-check-api | Verify API /health endpoint | dotnet-apps.yml | ~25 |
| Health Check DB | .github/actions/health-check-database | Verify SQL connectivity | databases.yml | ~20 |
| Health Check Lighthouse | .github/actions/health-check-lighthouse | React quality gates (Lighthouse CI) | react-apps.yml | ~35 |

**Code Reduction:** ~200 lines of YAML eliminated through reusable actions

---

## Workflow File References

### Primary Workflows

- [`.github/workflows/terraform.yml`](../../../.github/workflows/terraform.yml) - 571 lines
  - **Job:** terraform
  - **Conditions:** Runs on push main or manual workflow_dispatch
  - **Environment:** dev or prod
  - **Output:** api_app_name, react_app_name, acr_registry_url

- [`.github/workflows/databases.yml`](../../../.github/workflows/databases.yml) - 402 lines
  - **Jobs:** discover-databases, deploy-database (matrix)
  - **Conditions:** Called by main.yml
  - **Features:** Auto-discovery, parallel matrix jobs, Azure AD auth
  - **Script Execution:** Alphabetical order, seed/system skipped

- [`.github/workflows/dotnet-apps.yml`](../../../.github/workflows/dotnet-apps.yml) - 238 lines
  - **Jobs:** build-and-push, deploy
  - **Conditions:** Called by main.yml
  - **Features:** Docker build, ACR push, health check, auto-rollback

- [`.github/workflows/react-apps.yml`](../../../.github/workflows/react-apps.yml) - 255 lines
  - **Jobs:** build-and-push, deploy
  - **Conditions:** Called by main.yml
  - **Features:** Docker build, Lighthouse CI quality gates

- [`.github/workflows/main.yml`](../../../.github/workflows/main.yml)
  - **Purpose:** Orchestrator workflow
  - **Conditions:** Triggered by push main or manual dispatch
  - **Calls:** terraform.yml, databases.yml, dotnet-apps.yml, react-apps.yml, database-docs.yml
  - **Concurrency:** Controls prevent parallel runs of same workflow

- [`.github/workflows/build-runner-image.yml`](../../../.github/workflows/build-runner-image.yml)
  - **Purpose:** Build custom Docker runner image
  - **Triggers:** Manual dispatch or schedule (weekly)
  - **Output:** consilient-runner:latest image

- [`.github/workflows/database-docs.yml`](../../../.github/workflows/database-docs.yml)
  - **Purpose:** Generate SchemaSpy HTML documentation
  - **Triggers:** Called from main.yml on PR, manual dispatch
  - **Jobs:** discover-databases, validate-prerequisites, generate-db-docs (matrix)
  - **Output:** artifact with database-documentation-{name}-{suffix}.zip

### Composite Actions

- [`.github/actions/azure-login/`](../../../.github/actions/azure-login/) - GitHub → Azure authentication
- [`.github/actions/validate-inputs/`](../../../.github/actions/validate-inputs/) - Input validation
- [`.github/actions/debug-variables/`](../../../.github/actions/debug-variables/) - Debug output
- [`.github/actions/sqlcmd-execute/`](../../../.github/actions/sqlcmd-execute/) - SQL execution
- [`.github/actions/discover-databases/`](../../../.github/actions/discover-databases/) - Database discovery
- [`.github/actions/initialize/`](../../../.github/actions/initialize/) - Workflow init
- [`.github/actions/health-check-api/`](../../../.github/actions/health-check-api/) - API health
- [`.github/actions/health-check-database/`](../../../.github/actions/health-check-database/) - DB health
- [`.github/actions/health-check-lighthouse/`](../../../.github/actions/health-check-lighthouse/) - React quality

---

## Common Workflow Variables

### Environment Variables

All workflows accept or use:
- `environment` - "dev" or "prod" (validated)
- `skip_terraform` - Skip Terraform job if true
- `skip_databases` - Skip database deployment if true
- `recreate_database_objects` - Recreate DB objects in dev if true

### Matrix Strategies

**Database Deployment:**
- Matrix over: database names discovered from src/Databases/
- Parallel: Multiple databases deployed simultaneously
- Speed: 3 databases in ~same time as 1 database

**Database Documentation:**
- Matrix over: schemas per database
- Parallel: All schemas documented simultaneously
- Speed: Multiple schemas in ~3 minutes

---

## Workflow Triggers

### GitHub Events

| Event | Workflow | Behavior |
|-------|----------|----------|
| push main | main.yml | Automatic deployment |
| workflow_dispatch | main.yml, terraform.yml, database-docs.yml | Manual trigger |
| schedule | build-runner-image.yml | Weekly runner rebuild |
| pull_request | database-docs.yml | Generate docs for review |

### Workflow Calls

| Called Workflow | Called From | Condition |
|-----------------|-------------|-----------|
| terraform.yml | main.yml | Always (unless skip_terraform=true) |
| databases.yml | main.yml | After terraform succeeds (unless skip_databases=true) |
| dotnet-apps.yml | main.yml | After databases succeeds |
| react-apps.yml | main.yml | After databases succeeds |
| database-docs.yml | main.yml | After apps deploy |

---

## Workflow Execution Timeline

**Total execution:** ~20-25 minutes for full deployment

```
T+0 min:    main.yml starts
T+0 min:    terraform.yml starts
T+5 min:    terraform.yml ends
            ├─ databases.yml starts
            ├─ dotnet-apps.yml starts
            └─ react-apps.yml starts
T+8 min:    databases.yml ends
T+12 min:   dotnet-apps.yml ends (health check)
T+13 min:   react-apps.yml ends (Lighthouse)
            └─ database-docs.yml starts
T+18 min:   database-docs.yml ends
T+20 min:   main.yml completes
```

<!-- AI_NOTE: Times are approximate. Network latency and Azure API response times affect actual durations. -->

---

## Environment-Specific Behavior

### Development Environment (dev)

- **Terraform:** Uses B1 App Service Plan, Basic SQL
- **Databases:** Seed data included, recreate option available
- **Firewall:** Can enable local firewall for act testing
- **Cost:** ~$45/month
- **Uptime SLA:** Not required

### Production Environment (prod)

- **Terraform:** Uses P2v3 App Service Plan, GP_Gen5_4 SQL
- **Databases:** No seed data, zone-redundant, threat protection
- **Firewall:** Highly restrictive, no public access
- **Cost:** ~$2,800/month
- **Uptime SLA:** 99.95% (App Service), 99.99% (SQL)

---

## Debugging Workflows

### View Workflow Logs

1. Go to GitHub → Repository → Actions
2. Select workflow run
3. Click job name to expand steps
4. Click step to see output

### Enable Debug Logging

Set GitHub secret: `ACTIONS_STEP_DEBUG = true`

Output will include:
- Variable values
- File contents
- Command outputs
- Diagnostic information

### Local Testing

Test workflows locally using `act`:

```powershell
# Test database deployment
.\infra\act\run-act.ps1 -Environment dev -SkipTerraform

# Test full deployment
.\infra\act\run-act.ps1 -Environment dev

# With debug output
.\infra\act\run-act.ps1 -Environment dev -EnableDebugMode
```

See [components/local-testing.md](../components/local-testing.md) for details.

---

## Related Documentation

- [ARCHITECTURE.md](../ARCHITECTURE.md) - System architecture and flow diagrams
- [components/github-actions.md](../components/github-actions.md) - Detailed workflow documentation
- [components/local-testing.md](../components/local-testing.md) - Local testing with act
- [reference/secrets-variables.md](secrets-variables.md) - Secrets and variables reference

---

**Last Updated:** January 2026
**Version:** 1.0
