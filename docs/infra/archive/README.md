# Consilient Infrastructure Documentation

> Last Updated: December 2025 | Quick Links: [Terraform](#terraform-infrastructure) • [GitHub Actions](#github-actions-workflows) • [Troubleshooting](#troubleshooting) • [Architecture](#architecture)

## Quick Start

**New to the project?** Start here with these common tasks:

### Common Tasks (5-15 minutes each)

#### 1. Deploy Infrastructure to Dev (~10 min)

Prerequisites: Azure CLI, Terraform
Files: [`infra/terraform/terraform.tfvars.example`](../../../infra/terraform/terraform.tfvars.example)

Steps:
1. Copy `terraform.tfvars.example` to `terraform.tfvars` in [`infra/terraform/`](../../../infra/terraform/)
2. Update values with your Azure subscription info
3. Run: `terraform init && terraform apply`

See [QUICK_START.md](QUICK_START.md#deploy-infrastructure) for detailed steps.

#### 2. Deploy a Database Change (~5 min)

Prerequisites: GitHub secrets configured
Files: [`src/Databases/`](../../../src/Databases/)

Steps:
1. Add or modify SQL scripts in the appropriate database folder
2. Push to `main` or `develop` branch
3. Database deployment workflow [`databases.yml`](../../../.github/workflows/databases.yml) auto-discovers and deploys

See [QUICK_START.md](QUICK_START.md#deploy-database) for details on script organization and naming patterns.

#### 3. Test Workflows Locally with Act (~15 min)

Prerequisites: Docker, `act` CLI installed
Files: [`infra/act/`](../../../infra/act/)

Steps:
1. Navigate to `infra/act/` folder
2. Configure `.env.act` with your Azure credentials
3. Run: `.\run-act.ps1`
4. Follow prompts to test workflows locally

See [QUICK_START.md](QUICK_START.md#test-with-act) and [components/local-testing.md](components/local-testing.md) for setup and troubleshooting.

#### 4. Add a New Azure Resource (~20 min)

Prerequisites: Terraform knowledge
Files: [`infra/terraform/locals.tf`](../../../infra/terraform/locals.tf)

Steps:
1. Update resource configuration in [`locals.tf:85-120`](../../../infra/terraform/locals.tf#L85-L120) (cost profiles) if needed
2. Create or modify resource file (e.g., `network.tf`, `api_app.tf`)
3. Run `terraform plan` to validate
4. Apply changes with `terraform apply`

See [QUICK_START.md](QUICK_START.md#add-azure-resource) and [components/terraform.md](components/terraform.md) for detailed guidance.

#### 5. Configure GitHub Secrets (~10 min)

Prerequisites: Azure subscription access
Reference: [reference/secrets-checklist.md](reference/secrets-checklist.md)

Steps:
1. Review required secrets in [secrets-checklist.md](reference/secrets-checklist.md)
2. Obtain values from Azure or Terraform outputs
3. Configure in GitHub repository Settings → Secrets and Variables

See [reference/secrets-checklist.md](reference/secrets-checklist.md) for acquisition instructions.

---

## Component Deep-Dives

### Terraform Infrastructure

[`components/terraform.md`](components/terraform.md) - Complete Terraform infrastructure guide

**Key Topics:**
- [Naming Conventions](components/terraform.md#naming-conventions) - [`locals.tf:32-65`](../../../infra/terraform/locals.tf#L32-L65)
- [Cost Management](components/terraform.md#cost-management) - [`locals.tf:85-120`](../../../infra/terraform/locals.tf#L85-L120)
- [Resource Configuration](components/terraform.md#resources) - Files: [`main.tf`](../../../infra/terraform/main.tf), [`api_app.tf`](../../../infra/terraform/api_app.tf), [`sql.tf`](../../../infra/terraform/sql.tf)
- [State Management](components/terraform.md#state) - [`backend.tf`](../../../infra/terraform/backend.tf)

**Key Files:**
- [`infra/terraform/locals.tf`](../../../infra/terraform/locals.tf) - Single source of truth for all configuration
- [`infra/terraform/variables.tf`](../../../infra/terraform/variables.tf) - Input variables
- [`infra/terraform/outputs.tf`](../../../infra/terraform/outputs.tf) - Output values

### GitHub Actions Workflows

[`components/github-actions.md`](components/github-actions.md) - Complete CI/CD guide

**Key Topics:**
- [Workflow Architecture](components/github-actions.md#workflow-architecture)
- [Composite Actions](components/github-actions.md#composite-actions) - 4 reusable actions
- [Active Workflows](components/github-actions.md#active-workflows) - 7 total workflows
- [GitHub Variables Configuration](components/github-actions.md#github-variables)

**Active Workflows (7):**
- [`terraform.yml`](../../../.github/workflows/terraform.yml) - Infrastructure provisioning
- [`databases.yml`](../../../.github/workflows/databases.yml) - Database deployment
- [`dotnet_apps.yml`](../../../.github/workflows/dotnet_apps.yml) - .NET API deployment
- [`react_apps.yml`](../../../.github/workflows/react_apps.yml) - React app deployment
- [`main.yml`](../../../.github/workflows/main.yml) - Orchestrator workflow
- [`build-runner-image.yml`](../../../.github/workflows/build-runner-image.yml) - Custom runner builds
- [`docs_db.yml`](../../../.github/workflows/docs_db.yml) - Database documentation

**Composite Actions (4):**
- [`azure-login/`](../../../.github/actions/azure-login/) - OIDC + service principal auth
- [`validate-inputs/`](../../../.github/actions/validate-inputs/) - Input validation
- [`debug-variables/`](../../../.github/actions/debug-variables/) - Secure variable logging
- [`sqlcmd-execute/`](../../../.github/actions/sqlcmd-execute/) - SQL script execution

### Azure Resources

[`components/azure-resources.md`](components/azure-resources.md) - Azure service configuration

**Key Topics:**
- [App Services](components/azure-resources.md#app-services) - API and React apps
- [SQL Database](components/azure-resources.md#sql-database) - Database configuration
- [Container Registry](components/azure-resources.md#container-registry) - ACR setup
- [Networking](components/azure-resources.md#networking) - VNet, subnets, security
- [Monitoring Stack](components/azure-resources.md#monitoring-stack) - Grafana, Loki

**All resources defined in:** [`infra/terraform/`](../../../infra/terraform/)

### Authentication & Secrets

[`components/authentication.md`](components/authentication.md) - Three-tier authentication architecture

**Key Topics:**
- [OIDC Authentication](components/authentication.md#oidc-authentication) - Cloud execution (Tier 1)
- [Service Principal](components/authentication.md#service-principal) - Terraform (Tier 2)
- [Local Testing Fallback](components/authentication.md#local-testing-fallback) - Act tool (Tier 3)
- [Secret Configuration](components/authentication.md#secret-configuration) - Setup guide

**Quick Reference:**
- 8 Required Secrets (OIDC + Terraform + SQL)
- 1 Optional Secret (AZURE_CREDENTIALS for act)
- Setup Guide: [components/authentication.md](components/authentication.md#setup)
- Troubleshooting: [TROUBLESHOOTING.md#authentication-issues](TROUBLESHOOTING.md#authentication-issues)

### Database Management

[`components/databases.md`](components/databases.md) - Database deployment and operations

**Key Topics:**
- [Auto-Discovery](components/databases.md#auto-discovery-mechanism) - Scripts in [`src/Databases/`](../../../src/Databases/)
- [Naming Pattern](components/databases.md#naming-pattern) - `{directory}_{environment}`
- [Deployment Pipeline](components/databases.md#deployment-pipeline) - [`databases.yml:45-60`](../../../.github/workflows/databases.yml#L45-L60)
- [Matrix Jobs](components/databases.md#matrix-jobs) - Parallel deployment

**Databases:**
- `consilient_main_{env}` - Main application database
- `consilient_hangfire_{env}` - Background jobs database
- Custom databases via auto-discovery from `src/Databases/` structure

### Local Testing with Act

[`components/local-testing.md`](components/local-testing.md) - Complete act tool guide

**Key Topics:**
- [Setup Guide](components/local-testing.md#setup-guide) - [`infra/act/.env.act`](../../../infra/act/.env.act)
- [run-act.ps1 Script](components/local-testing.md#using-run-actps1-script) - All parameters and modes
- [Performance Optimization](components/local-testing.md#performance-optimization) - `--bind` flag details
- [Troubleshooting](components/local-testing.md#troubleshooting)

**Key Files:**
- [`infra/act/run-act.ps1`](../../../infra/act/run-act.ps1) - Main execution script
- [`infra/act/.env`](../../../infra/act/.env) - Environment variables
- [`infra/act/.env.act`](../../../infra/act/.env.act) - Secrets file (sensitive)

---

## Troubleshooting

[`TROUBLESHOOTING.md`](TROUBLESHOOTING.md) - Comprehensive troubleshooting guide

**Quick Links:**
- [Terraform Errors](TROUBLESHOOTING.md#terraform-errors) - State issues, imports, provider auth
- [GitHub Actions Failures](TROUBLESHOOTING.md#github-actions-failures) - Secret validation, OIDC, triggers
- [Authentication Issues](TROUBLESHOOTING.md#authentication-issues) - Client ID confusion, permissions
- [Database Deployment](TROUBLESHOOTING.md#database-deployment) - Discovery, execution, connections
- [Azure Resources](TROUBLESHOOTING.md#azure-resources) - CAE conflicts, deployment failures
- [Local Testing (Act)](TROUBLESHOOTING.md#local-testing-act) - Docker, credentials, firewall

---

## Known Issues & Opportunities

[`KNOWN_ISSUES.md`](KNOWN_ISSUES.md) - Track issues, limitations, and improvements

**Quick Links:**
- [Active Issues](KNOWN_ISSUES.md#active-issues) - Bugs and problems
- [Limitations](KNOWN_ISSUES.md#limitations) - Technology constraints
- [Performance Considerations](KNOWN_ISSUES.md#performance-considerations) - Optimization opportunities
- [Configuration Gaps](KNOWN_ISSUES.md#configuration-gaps) - Missing setup
- [Future Enhancements](KNOWN_ISSUES.md#future-enhancements) - Planned improvements
- [Community Contributions](KNOWN_ISSUES.md#community-contributions-wanted) - Help wanted

---

## Architecture

[`ARCHITECTURE.md`](ARCHITECTURE.md) - System architecture and design

**Diagrams & Flows:**
- [High-Level Architecture](ARCHITECTURE.md#high-level-architecture) - GitHub → Terraform → Azure
- [CI/CD Pipeline Flow](ARCHITECTURE.md#cicd-pipeline-flow) - Workflow orchestration
- [Authentication Flow](ARCHITECTURE.md#authentication-flow) - OIDC, Service Principal, Fallback
- [Database Deployment Flow](ARCHITECTURE.md#database-deployment-flow) - Auto-discovery and deploy
- [Resource Dependencies](ARCHITECTURE.md#resource-dependencies) - Terraform modules and Azure resources

---

## Quick Reference

### Secrets Checklist
[`reference/secrets-checklist.md`](reference/secrets-checklist.md) - Secret configuration quick reference

| Secret | Purpose | Source |
|--------|---------|--------|
| AZURE_CLIENT_ID | OIDC authentication | Azure Entra ID |
| AZURE_TENANT_ID | OIDC authentication | Azure Entra ID |
| AZURE_SUBSCRIPTION_ID | OIDC authentication | Azure |
| ARM_CLIENT_ID | Terraform authentication | Service Principal |
| ARM_CLIENT_SECRET | Terraform authentication | Service Principal |
| ARM_TENANT_ID | Terraform authentication | Azure |
| SQL_ADMIN_PASSWORD | SQL Server admin | Generate |
| AZURE_CREDENTIALS | Act fallback (optional) | Service Principal JSON |

See [reference/secrets-checklist.md](reference/secrets-checklist.md) for acquisition instructions.

### Naming Conventions
[`reference/naming-conventions.md`](reference/naming-conventions.md) - Resource naming patterns

**Pattern:** `{project}-{type}-{name}-{env}-{unique_suffix}`

**Example:** `consilient-api-app-dev-a1b2c3`

**Databases:** `{name}_{environment}` (e.g., `consilient_main_dev`)

See [reference/naming-conventions.md](reference/naming-conventions.md) for all patterns.

### Cost Management
[`reference/cost-management.md`](reference/cost-management.md) - Cost profiles by environment

| Environment | Monthly Cost | SKU Strategy |
|-------------|-------------|--------------|
| Development | ~$200 | Basic/Standard (low cost) |
| Staging | ~$1,200 | Standard (realistic) |
| Production | ~$2,800 | Premium (high availability) |

See [reference/cost-management.md](reference/cost-management.md) for optimization tips.

### Code References Index
[`reference/code-references.md`](reference/code-references.md) - Complete file index with line numbers

Helps AI agents quickly locate relevant code:
- All Terraform files with key sections
- All GitHub Actions workflows with jobs
- All composite actions with implementations
- Database script locations

---

## For AI Agents

This documentation uses **inline code references** to help AI navigate between docs and code:

**Format:** `[description](relative/path/to/file#L123-L456)` or `[file:line](path#L123)`

**Example References:**
- Terraform naming: [`locals.tf:32-65`](../../../infra/terraform/locals.tf#L32-L65)
- Workflow triggers: [`terraform.yml:5-15`](../../../.github/workflows/terraform.yml#L5-L15)
- Composite action: [`azure-login/action.yml:28-38`](../../../.github/actions/azure-login/action.yml#L28-L38)

**Navigation Tips:**
- All references use relative paths from `docs/infra/`
- Line numbers provided for specific sections
- File structure mirrors component organization
- Use [reference/code-references.md](reference/code-references.md) to find any file quickly

---

## Documentation Structure

```
docs/infra/
├── README.md                           # You are here
├── QUICK_START.md                      # 5-10 minute tasks
├── ARCHITECTURE.md                     # System design & diagrams
├── TROUBLESHOOTING.md                  # Error diagnosis & solutions
├── KNOWN_ISSUES.md                     # Issues, limitations, improvements
│
├── components/                         # Deep-dive documentation
│   ├── terraform.md                    # Infrastructure as Code
│   ├── github-actions.md               # CI/CD workflows
│   ├── azure-resources.md              # Azure services
│   ├── authentication.md               # OIDC & secrets
│   ├── databases.md                    # Database deployment
│   └── local-testing.md                # Act tool & local development
│
├── reference/                          # Quick lookups
│   ├── secrets-checklist.md            # Secret configuration
│   ├── naming-conventions.md           # Resource patterns
│   ├── cost-management.md              # Cost profiles
│   └── code-references.md              # File index for AI
│
└── archive/                            # Previous documentation
    └── [original files for reference]
```

---

## Suggested Learning Path

**For New Developers:**
1. Start with [QUICK_START.md](QUICK_START.md) - Get hands-on quickly
2. Read [ARCHITECTURE.md](ARCHITECTURE.md) - Understand the system
3. Explore component files as needed for deep dives
4. Use [TROUBLESHOOTING.md](TROUBLESHOOTING.md) when things break

**For DevOps/Infrastructure:**
1. Review [components/terraform.md](components/terraform.md) - IaC management
2. Study [components/github-actions.md](components/github-actions.md) - CI/CD pipelines
3. Check [components/authentication.md](components/authentication.md) - Security setup
4. Reference [reference/cost-management.md](reference/cost-management.md) - Cost optimization

**For AI Tools/Automation:**
1. Review inline code references format (see "For AI Agents" section)
2. Use [reference/code-references.md](reference/code-references.md) for file locations
3. Cross-reference component files with inline links
4. Check [ARCHITECTURE.md](ARCHITECTURE.md) for system context

---

## Need Help?

- **Quick answers:** Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
- **Implementation details:** See component files under `components/`
- **Code locations:** Use [reference/code-references.md](reference/code-references.md)
- **Known problems:** Check [KNOWN_ISSUES.md](KNOWN_ISSUES.md)
- **System design:** Review [ARCHITECTURE.md](ARCHITECTURE.md)

---

**Last Updated:** December 2025
**Maintainer:** Infrastructure Team
**Status:** Production-ready documentation
