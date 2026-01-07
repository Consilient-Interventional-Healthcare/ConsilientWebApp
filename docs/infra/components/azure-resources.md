# Azure Resources Configuration

<!-- AI_CONTEXT: Complete overview of 19 Azure resources managed by Terraform. Per-environment deployment. Two environments: dev and prod. Resource dependencies documented. -->

## For Non-Technical Stakeholders

Azure resources are the cloud services that run the Consilient application. This includes servers for the API and website, databases for storing data, storage for logs, and networking to connect everything securely. All resources are defined in Terraform code and deployed automatically.

---

Overview of all Azure services managed by Terraform.

## Resource Overview

All resources defined in [`infra/terraform/`](../../../infra/terraform/) using azurerm provider v4.57.0.

**Managed via:** Terraform (Infrastructure as Code)
**Configuration:** [`locals.tf`](../../../infra/terraform/locals.tf) (single source of truth)
**Naming:** See [reference/naming-conventions.md](../reference/naming-conventions.md)

## Key Resources

### App Services

**Files:**
- API: [`api_app.tf`](../../../infra/terraform/api_app.tf)
- React: [`react_app.tf`](../../../infra/terraform/react_app.tf)

**Configuration:**
- Linux App Service Plans
- Docker container deployment from ACR
- Health checks enabled
- Deployment slots for zero-downtime updates

**SKU by Environment:** [`locals.tf:145-180`](../../../infra/terraform/locals.tf#L145-L180)
- Dev: Basic
- Staging: Standard
- Production: Premium

### SQL Database

**File:** [`sql.tf`](../../../infra/terraform/sql.tf)

**Components:**
- SQL Server (v12.0)
- Main database: `consilient_main_{env}`
- Hangfire database: `consilient_hangfire_{env}`

**Configuration:** [`locals.tf:185-220`](../../../infra/terraform/locals.tf#L185-L220)
- Serverless for dev/staging (auto-pause capability)
- Azure AD admin authentication
- Advanced Threat Protection
- Automatic auditing
- Backup retention

**Security:**
- No SQL username/password in workflows
- Uses Azure AD auth (via `sqlcmd -G`)
- Managed identities for app access

### Container Registry (ACR)

**File:** [`acr.tf`](../../../infra/terraform/acr.tf)

**Features:**
- Docker image storage
- OIDC authentication (GitHub Actions)
- Service principal auth (local docker login)
- Built-in security scanning

**Image Locations:**
- API: `{registry}/consilientapi:v{N}-{SHA}`
- React: `{registry}/consilient-react:v{N}-{SHA}`

### Networking

**File:** [`network.tf`](../../../infra/terraform/network.tf)

**Components:**
- Virtual Network: 10.10.0.0/16
- Subnet: 10.10.1.0/24
- Network Security Groups (NSGs)
- Service endpoints where applicable

**Purpose:**
- Internal communication between services
- Security boundary enforcement
- Firewall rules for SQL access

### Storage Account

**File:** [`storage.tf`](../../../infra/terraform/storage.tf)

**Purpose:**
- Log storage for Loki
- Private endpoint for secure access
- TLS 1.2 enforcement
- Blob containers for archival

**Replication:**
- Dev: LRS (locally redundant)
- Prod: GRS (geo-redundant, optional)

### Monitoring Stack

**Loki:** [`loki.tf`](../../../infra/terraform/loki.tf)
- Container App Environment
- Log aggregation service
- Stores logs in Storage Account

**Grafana:** [`grafana.tf`](../../../infra/terraform/grafana.tf)
- Azure Managed Grafana
- Dashboard and visualization
- Data source: Loki
- Access control: RBAC

## Cost Optimization

**Three-Tier Strategy:**

| Tier | SKUs | Cost |
|------|------|------|
| Dev | Basic, Serverless | ~$200/month |
| Staging | Standard, Serverless | ~$1,200/month |
| Prod | Premium, Full SQL | ~$2,800/month |

**Optimization Strategies:**
- Use serverless SQL for dev/staging (auto-pause)
- Schedule resource shutdown (dev) during off-hours
- Use shared Container App Environment
- Reserved instances for production

See [reference/cost-management.md](../reference/cost-management.md) for details.

## Deployment Process

1. **Terraform Plan** - Reviews changes
2. **Terraform Apply** - Creates resources in Azure
3. **Database Scripts** - Populates databases
4. **Application Deploy** - Deploys Docker containers
5. **Health Checks** - Verifies everything works

**Workflow:** [ARCHITECTURE.md#deployment-sequence](../ARCHITECTURE.md#deployment-sequence)

## Security

**Authentication:**
- Managed Identities (where possible)
- Azure AD authentication
- OIDC for cloud (no long-lived secrets)
- Service principals with Contributor role

**Encryption:**
- Encryption at rest (Azure defaults)
- TLS 1.2+ for data in transit
- Private endpoints for storage

**Network:**
- NSGs for traffic control
- Virtual network isolation
- Service endpoints

## Related Resources

- [components/terraform.md](terraform.md) - Terraform infrastructure guide
- [components/databases.md](databases.md) - Database deployment
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System design
- [reference/cost-management.md](../reference/cost-management.md) - Cost optimization
- [INFRASTRUCTURE.md](../../INFRASTRUCTURE.md) - Detailed original guide

---

**Last Updated:** December 2025
**For Navigation:** See [README.md](../README.md)
