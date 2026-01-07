# Azure Resources Quick Reference

<!-- AI_CONTEXT: Complete inventory of Azure resources managed by Terraform. Two environments: dev ($45/mo) and prod ($2,800/mo). 19 resources per environment. -->

## For Non-Technical Stakeholders

Azure resources are the cloud services that run the Consilient application. This includes servers for the API and website, databases for storing data, storage for logs, and networking to connect everything securely. All resources are defined in Terraform code and deployed automatically.

---

## Resource Inventory

<!-- AI_TABLE: All Azure resources with Terraform file references and monthly costs -->

| Resource Type | Name Pattern | Terraform File | Module | SKU (dev/prod) | Monthly Cost |
|---------------|--------------|----------------|--------|----------------|--------------|
| **Resource Group** | consilient-rg-{env} | main.tf | - | N/A | Free |
| **Virtual Network** | consilient-vnet-{env} | network.tf | - | N/A | Free |
| **Subnet** | consilient-subnet-{env} | network.tf | - | N/A | Free |
| **Container Registry** | consilientacr{env}{hash} | acr.tf | - | Basic/Premium | $5/$40 |
| **SQL Server** | consilient-sqlsrv-{env}-{hash} | sql.tf | - | N/A (logical) | Free |
| **SQL Database (Main)** | consilient_main_{env} | sql.tf | sql_database | Basic/GP_Gen5_4 | $5/$1,300 |
| **SQL Database (Hangfire)** | consilient_hangfire_{env} | sql.tf | sql_database | Basic/GP_Gen5_2 | $5/$650 |
| **App Service Plan (API)** | consilient-asp-api-{env} | api_app.tf | app_service | B1/P2v3 | $13/$204 |
| **App Service Plan (React)** | consilient-asp-react-{env} | react_app.tf | app_service | B1/P2v3 | $13/$204 |
| **App Service (API)** | consilient-api-{env}[-tier] | api_app.tf | app_service | Linux, Docker | Included |
| **App Service (React)** | consilient-react-{env}[-tier] | react_app.tf | app_service | Linux, Docker | Included |
| **Key Vault** | consilient-kv-{env}-{hash} | keyvault.tf | - | Standard | ~$1 |
| **App Configuration** | consilient-appconfig-{env} | app_configuration.tf | - | Standard | ~$1 |
| **Storage Account (Loki)** | consilientloki{env}{hash} | storage.tf | storage_account | Standard_LRS | ~$1 |
| **Container App Environment** | consilient-cae-{env} | loki.tf | - | Consumption | ~$1 |
| **Container App (Loki)** | consilient-loki-{env} | loki.tf | - | 0.5 vCPU, 1Gi | ~$15 |
| **Managed Grafana** | consilient-grafana-{env} | grafana.tf | - | Standard | ~$100 |
| **Managed Identity (Loki)** | consilient-loki-identity-{env} | loki.tf | - | N/A | Free |
| **Private Endpoint (Loki)** | consilient-pe-loki-storage-{env} | loki.tf | - | N/A | ~$7 |

**Total Resources:** 19 per environment
**Total Monthly Cost:** ~$116 (dev, Loki included), ~$2,920 (prod, Loki included)

<!-- AI_NOTE: Name suffixes {hash} are 6-char MD5 of subscription+RG. Tier suffix [-tier] only present if multi-tier hostname strategy uses tier 1 or 2. -->

---

## Resource Dependencies

```
Azure Subscription
│
└─ Resource Group
   │
   ├─ Virtual Network: 10.10.0.0/16
   │  │
   │  └─ Subnet: 10.10.2.0/24 (App Services)
   │     │
   │     ├─ App Service Plan (API): B1/P2v3
   │     │  └─ App Service (API)
   │     │     └─ Managed Identity (assigned)
   │     │
   │     └─ App Service Plan (React): B1/P2v3
   │        └─ App Service (React)
   │           └─ Managed Identity (assigned)
   │
   ├─ Container Registry
   │  └─ API images: consilientapi:vN-{sha}
   │  └─ React images: consilient-react:vN-{sha}
   │
   ├─ SQL Server
   │  ├─ SQL Database (Main): consilient_main_{env}
   │  │  └─ Tables, stored procedures, indexes
   │  └─ SQL Database (Hangfire): consilient_hangfire_{env}
   │     └─ Background job queue
   │
   ├─ Key Vault
   │  ├─ Secret: sql-connection-string-main
   │  ├─ Secret: sql-connection-string-hangfire
   │  ├─ Secret: jwt-signing-secret
   │  ├─ Secret: grafana-loki-url
   │  └─ Secret: oauth-client-secret (optional)
   │
   ├─ App Configuration
   │  ├─ Key Vault references (above)
   │  └─ Runtime configuration values
   │
   ├─ Storage Account (Loki logs)
   │  └─ Blob Container: loki-data
   │
   ├─ Container App Environment
   │  └─ Container App (Loki)
   │     ├─ Managed Identity (assigned)
   │     └─ Private Endpoint to Storage
   │
   └─ Managed Grafana
      └─ Data source: Loki
```

---

## Compute Resources

### App Service Plans

| Resource | Name | Dev SKU | Prod SKU | Dev Cost | Prod Cost | Instances |
|----------|------|---------|----------|----------|-----------|-----------|
| API Plan | consilient-asp-api-{env} | B1 | P2v3 | $13/mo | $204/mo | 1 (dev), 1+ (prod) |
| React Plan | consilient-asp-react-{env} | B1 | P2v3 | $13/mo | $204/mo | 1 (dev), 1+ (prod) |

**Dev (B1):**
- 1 vCPU, 1.75 GB RAM
- Shared compute with other customers
- Good for dev/test

**Prod (P2v3):**
- 4 vCPU, 14 GB RAM per instance
- Dedicated compute
- Auto-scaling capable

### Web Apps

| Resource | Name | Runtime | Docker | Port |
|----------|------|---------|--------|------|
| API App | consilient-api-{env} | Linux | .NET 8 image | 8080 |
| React App | consilient-react-{env} | Linux | Nginx image | 80 |

Both apps:
- Health check endpoint (API only: /health)
- Eviction time: 5 minutes
- Auto-restart on failure
- Managed Identity for Azure resource access

### Container Apps

| Resource | Name | Config | Purpose |
|----------|------|--------|---------|
| Loki Container App | consilient-loki-{env} | 0.5 CPU, 1Gi RAM | Log aggregation |
| Container App Environment | consilient-cae-{env} | Per-environment | Runtime for Loki |

---

## Data Resources

### SQL Server

| Attribute | Value |
|-----------|-------|
| Name | consilient-sqlsrv-{env}-{hash} |
| Authentication | Azure AD (no SQL passwords) |
| Firewall | Restrictive, allows Azure services |
| Threat Protection | Enabled in prod only |
| Auditing | Enabled in prod, 365-day retention |

### SQL Databases

| Database | Size (Dev/Prod) | Purpose |
|----------|-----------------|---------|
| consilient_main_{env} | Basic / GP_Gen5_4 | Application data |
| consilient_hangfire_{env} | Basic / GP_Gen5_2 | Background jobs |

**Dev Configuration:**
- Basic DTU model (~5 DTU)
- No auto-pause
- No threat protection

**Prod Configuration:**
- Provisioned vCore model
- Always-on (no auto-pause)
- Zone-redundant (HA across zones)
- Threat protection enabled
- Auditing enabled

### Storage Account (Loki)

| Attribute | Value |
|-----------|-------|
| Account Name | consilientloki{env}{hash} |
| Tier | Standard |
| Replication | Locally redundant (LRS) |
| Access | Private endpoint only |
| Purpose | Loki log storage |
| Container | loki-data |

---

## Configuration Resources

### Key Vault

| Attribute | Value |
|-----------|-------|
| Name | consilient-kv-{env}-{hash} |
| SKU | Standard (premium optional) |
| RBAC | Role-based access control |
| Access | Managed Identity from App Services |
| Secrets | 5 total (see secrets-variables.md) |

**Access Method:**
- App Services use Managed Identity
- App Configuration uses Managed Identity
- Applications access via Key Vault references

### App Configuration

| Attribute | Value |
|-----------|-------|
| Name | consilient-appconfig-{env} |
| SKU | Standard (premium optional) |
| Store | Key-value pairs with labels |
| Key Vault | Integrated references |
| Purpose | Runtime configuration source of truth |

**Configuration Keys:**
- Api:Authentication:Jwt:Issuer
- Api:Logging:LogLevel:Default
- React:ApiBaseUrl
- ConnectionStrings:* (Key Vault references)

---

## Network Resources

### Virtual Network

| Attribute | Value |
|-----------|-------|
| Name | consilient-vnet-{env} |
| Address Space | 10.10.0.0/16 |
| Subnets | 2 (Container Apps, App Services) |

### Subnets

| Subnet | CIDR | Delegation | Purpose |
|--------|------|-----------|---------|
| casubnet | 10.10.1.0/24 | ContainerApps | Loki runtime |
| appservicesubnet | 10.10.2.0/24 | Web/App | API and React |

### Private Endpoint

| Attribute | Value |
|-----------|-------|
| Name | consilient-pe-loki-storage-{env} |
| Target | Storage Account |
| Service | Blob storage |
| VNet | consilient-vnet-{env} |
| Private IP | Assigned from subnet |

**Benefits:**
- No internet exposure for storage
- Prevents data exfiltration
- Complies with security requirements

---

## Monitoring Resources

### Managed Grafana

| Attribute | Value |
|-----------|-------|
| Name | consilient-grafana-{env} |
| Version | 11 (latest) |
| SKU | Standard |
| Authentication | Azure AD |
| Data Source | Loki |
| Cost | ~$100/month per environment |

**Dashboards:**
- Application metrics
- Infrastructure health
- Log analysis
- Custom dashboards

---

## Identity & Access

### Managed Identities

| Identity | Assigned To | Purpose |
|----------|-------------|---------|
| API App Service | App Service (API) | Access Key Vault, App Configuration |
| React App Service | App Service (React) | Access App Configuration |
| Loki | Container App (Loki) | Access Storage Account |

**Benefits:**
- No credentials in code
- Azure AD-based authentication
- Automatic key rotation
- Audit trail in Activity Log

---

## Cost Summary

### Development Environment

| Resource | Monthly Cost |
|----------|--------------|
| App Services (2 x B1) | $26 |
| SQL Databases (Basic) | $10 |
| Container Registry | $5 |
| Storage/Networking | $4 |
| **Subtotal (Core)** | **$45** |
| Monitoring (Loki/Grafana) | ~$116 |
| **Total (with Monitoring)** | **~$161** |

### Production Environment

| Resource | Monthly Cost |
|----------|--------------|
| App Services (2 x P2v3) | $408 |
| SQL Databases (Provisioned) | $1,950 |
| Container Registry (Premium) | $40 |
| Storage/Networking | $200 |
| **Subtotal (Core)** | **~$2,598** |
| Monitoring (Loki/Grafana) | ~$120 |
| **Total (with Monitoring)** | **~$2,920** |

<!-- AI_WARNING: These are estimates. Actual costs vary by usage (data egress, storage consumption, transaction volume). Monitor Azure Cost Management for actual costs. -->

---

## Terraform Modules

<!-- AI_TABLE: Reusable Terraform modules -->

| Module | Path | Purpose | Resources |
|--------|------|---------|-----------|
| app_service | modules/app_service/ | App Service with custom domains and health checks | Plan + Web App + custom domain binding |
| sql_database | modules/sql_database/ | SQL Database with environment-specific SKUs | SQL Database + threat protection + auditing |
| storage_account | modules/storage_account/ | Storage account with blob container | Storage account + container + private endpoint |

---

## Related Documentation

- [components/terraform.md](../components/terraform.md) - Detailed Terraform configuration guide
- [components/azure-resources.md](../components/azure-resources.md) - Comprehensive resource details
- [reference/cost-management.md](cost-management.md) - Cost optimization strategies
- [reference/naming-conventions.md](naming-conventions.md) - Resource naming patterns

---

**Last Updated:** January 2026
**Version:** 1.0
