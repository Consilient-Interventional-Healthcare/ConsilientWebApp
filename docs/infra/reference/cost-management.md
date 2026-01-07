# Cost Management Reference

<!-- AI_CONTEXT: Two-environment cost model (dev, prod). No staging environment. Code validation in variables.tf:27-28. -->

Quick reference for cost estimation, optimization, and cost profile configuration.

## Monthly Cost Summary

<!-- AI_TABLE: Two-environment cost profile. No staging environment exists. -->

| Environment | Estimated Cost | Primary Resources | Optimization Strategy |
|-------------|-----------------|-------------------|----------------------|
| **Development** | ~$45/month | Basic tier (B1 App Service, Basic SQL) | Minimal resources for testing |
| **Production** | ~$2,800/month | Premium tier (P2v3 App Service, Provisioned SQL) | Zone-redundant, threat protection, full auditing |

<!-- AI_NOTE: Previous documentation mentioned "staging" but code validation (variables.tf:27-28) only allows dev/prod. Staging configuration in locals.tf is not validated and should not be used. -->

**Notes:**
- Estimates from [`infra/terraform/locals.tf:285-288`](../../../infra/terraform/locals.tf#L285-L288)
- SKU configurations in [`infra/terraform/locals.tf:57-82`](../../../infra/terraform/locals.tf#L57-L82)
- Actual costs vary based on usage (storage, data egress, execution time)

## Cost Profile Configuration

All cost tiers are defined in a single location for easy management.

**File:** [`infra/terraform/locals.tf`](../../../infra/terraform/locals.tf)

### Development Tier (Dev)

**Strategy:** Minimal cost (~$45/month)

```hcl
dev = {
  app_service_plan   = "B1"          # Basic - $13/month
  container_registry = "Basic"       # ~$5/month
  sql_basic          = "Basic"       # ~$5/month (Basic DTU, not serverless)
  sql_serverless     = "GP_S_Gen5_2" # Available but not used
}
```

**Characteristics:**
- Basic App Service tier (1 vCPU, 1.75 GB RAM)
- Basic SQL DTU model (~5 DTU = $5/month)
- No auto-pause (runs continuously)
- No threat protection or auditing
- No zone redundancy
- Container registry: Basic tier

**Use Cases:**
- Feature branch testing
- Local integration testing
- Development and debugging
- CI/CD pipeline testing

**Cost Breakdown:**
| Resource | SKU | Monthly Cost |
|----------|-----|--------------|
| API App Service | B1 | ~$13 |
| React App Service | B1 | ~$13 |
| Container Registry | Basic | ~$5 |
| SQL Server (Main) | Basic DTU | ~$5 |
| SQL Server (Hangfire) | Basic DTU | ~$5 |
| Networking/Storage | - | ~$4 |
| **Total** | | **~$45** |

### Production Tier (Prod)

**Strategy:** High availability and performance (~$2,800/month)

```hcl
prod = {
  app_service_plan   = "P2v3"      # Premium v3 - ~$204/month
  container_registry = "Premium"   # ~$40/month (geo-replication)
  sql_serverless     = "GP_Gen5_2" # General Purpose (provisioned)
  sql_provisioned    = "GP_Gen5_4" # General Purpose 4 vCores - ~$1,300/month
}
```

**Characteristics:**
- Premium v3 App Service tier (4 vCPU, 14 GB RAM per instance)
- SQL Provisioned (always-on, no auto-pause)
- Zone-redundant database (HA across availability zones)
- Threat protection enabled
- Auditing enabled with 365-day retention (compliance)
- Premium Container Registry with geo-replication
- Dedicated Container App Environment (isolation)

**Use Cases:**
- Production/live environment
- Customer-facing applications
- Business-critical workloads
- 24/7 availability requirements
- Compliance and audit requirements

**Cost Breakdown:**
| Resource | SKU | Monthly Cost | Notes |
|----------|-----|--------------|-------|
| API App Service | P2v3 | ~$204 | 4 vCPU, 14 GB |
| React App Service | P2v3 | ~$204 | 4 vCPU, 14 GB |
| Container Registry | Premium | ~$40 | Geo-replication |
| SQL Server (Main) | GP_Gen5_4 | ~$1,300 | Always-on, zone-redundant |
| SQL Server (Hangfire) | GP_Gen5_2 | ~$650 | Always-on |
| Loki + Grafana | Premium CAE | ~$150 | Dedicated environment |
| Networking/Storage | - | ~$200 | HA, private endpoints |
| **Total** | | **~$2,800** | |

**High Availability Features:**
```hcl
# From locals.tf:126-130 and 152-156
zone_redundant = {
  prod    = true  # Zone redundant for HA
}

# From locals.tf:160-170
threat_protection_enabled = {
  prod    = true  # SQL threat protection
}

auditing_enabled = {
  prod    = true  # SQL auditing
}

audit_retention_days = {
  prod    = 365   # 1 year retention
}
```

## Cost Optimization Strategies

### 1. Environment-Specific Sizing

**Current Approach:** Use different SKUs for each environment

| Strategy | Dev | Staging | Prod | Savings |
|----------|-----|---------|------|---------|
| Basic | B1 | P1v2 | P2v3 | 62x cheaper than prod |
| SQL Tiers | Basic DTU | Serverless | Provisioned | Lowest in dev |
| Shared Services | Bundled | Shared CAE | Dedicated | ~50% savings in staging |

### 2. Auto-Pause for Databases (Staging Only)

**Configuration:** [`locals.tf:146-150`](../../../infra/terraform/locals.tf#L146-L150)

```hcl
auto_pause_delay = {
  dev     = null      # Not applicable (Basic DTU)
  staging = 120       # Pause after 2 hours
  prod    = null      # Always-on (compliance)
}
```

**Impact:**
- Staging databases pause during off-hours
- Hibernation saves ~80% of database costs
- Wake-up time: ~30 seconds on first query
- Good for testing/staging, not production

### 3. Container App Environment (CAE) Sizing

**Configuration:** [`terraform.tfvars`](../../../infra/terraform/terraform.tfvars.example)

Each environment has its own Container App Environment using the template-based naming convention. Optimize CAE costs through:
- CPU and memory allocation tuning
- Container image optimization
- Workload scheduling

### 4. Reserved Instances for Production

**Opportunity:** Reserve production resources for annual discounts

| Resource | Standard Cost | 1-Year Reserve | 3-Year Reserve | Savings |
|----------|---------------|----------------|----------------|---------|
| P2v3 App Service | $204/month | ~$175/month | ~$145/month | 29% annual |
| GP_Gen5_4 SQL | $1,300/month | ~$1,150/month | ~$975/month | 25% annual |
| Premium ACR | $40/month | $35/month | $30/month | 25% annual |
| **Annual Total** | **$33,600** | **~$28,900** | **~$25,200** | **~25%** |

**Setup:** Configure in Azure Portal or via Terraform (requires Azure policy)

### 5. Off-Hours Shutdown (Dev Only)

**Opportunity:** Auto-stop dev environments after business hours

**Potential Savings:**
- Dev environment: ~70% reduction (stopped 14 hours/day)
- Annual savings: ~$315 (from $45/month to $13/month)

**Implementation:**
```powershell
# Example: Stop all app services at 6 PM
az resource update --ids $(az resource list --resource-type Microsoft.Web/sites --query '[].id' -o tsv) --set properties.enabled=false
```

**Trade-offs:**
- Requires manual/scheduled restart
- Cold start delay (~30 seconds)
- Not suitable for active development

### 6. Spot Instances (Not Currently Used)

**Opportunity:** Use Azure Spot VMs for non-critical workloads

**Potential Savings:** 70-80% discount vs. standard pricing

**Limitation:** Current architecture uses App Services (not VMs), which don't support Spot instances directly.

## Cost Monitoring & Alerts

### Azure Cost Management

**Portal:** Azure → Cost Management + Billing

**Recommended Setup:**
1. Create budget: $100/month for dev, $1,500/month for staging
2. Set alerts: 50%, 75%, 100% of budget
3. Configure notifications: Email alerts to team
4. Review daily: Monitor actual vs. estimated costs

### Cost Analysis by Resource

**PowerShell Example:**
```powershell
# List resources by cost
az costmanagement query \
  --timeframe ActualLastMonth \
  --type "ActualCost" \
  --dataset \
    granularity=Monthly \
    aggregation='{totalCost:{name:PreTaxCost,function:Sum}}' \
    grouping='{type:Dimension,name:ResourceType}' \
  --scope "/subscriptions/<SUBSCRIPTION_ID>"
```

## Cost Comparison by Use Case

### Scenario 1: Long-Lived Dev Environment

**Setup:** Keep dev environment running 24/7

**Monthly Cost:**
- Current: ~$45/month
- Year 1 total: ~$540
- Optimization: Use shared ACR → save ~$5/month ($60/year)

### Scenario 2: Testing & CI/CD

**Setup:** Run workflows, destroy after testing

**Monthly Cost:**
- Infrastructure: $0 (destroyed)
- ACR storage: ~$5 (persistent images)
- GitHub Actions minutes: ~$0 (free tier includes 2,000/month)

**Recommendation:** Destroy infrastructure after testing to save costs.

### Scenario 3: Production Redundancy

**Setup:** Production + hot standby (HA)

**Monthly Cost:**
- Primary: $2,800
- Standby: $2,800
- Total: $5,600
- With reserved instances: ~$4,500-5,000/month

**Recommendation:** Use Azure failover groups (included in provisioned SQL) instead of duplicate infrastructure.

## Billing & Invoicing

### Monthly Bill Breakdown

**Typical Invoice Structure:**

```
Azure Subscription Bill
========================
API App Service          $150
React App Service        $150
SQL Database            $1,000
Container Registry       $20
Storage Account          $50
Networking (VNet/PE)     $100
Loki + Grafana           $150
Data Transfer            $50
Other Services           $50
------------------------
Subtotal             $1,720
Taxes (~13% in Canada)  $223
------------------------
Total                $1,943
```

### Cost Reduction Opportunities

**High Priority (Quick Wins):**
1. Reserved instances: 25% savings = $25/month
2. Off-hours shutdown (dev): 70% savings = $31/month
3. Auto-pause databases (staging): 20% savings = $240/month
4. **Total potential savings: ~$296/month (~25%)**

**Medium Priority (Implementation Effort):**
1. Spot instances: 70% savings (requires architecture change)
2. CDN for static assets: 30-50% bandwidth savings
3. Database optimization: 10-20% savings (auto-indexing, stats)

**Low Priority (Strategic):**
1. Multi-region replication: Higher cost but better performance
2. Advanced monitoring: Better visibility but additional cost

## Further Reading

- [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/)
- [Azure Cost Management Best Practices](https://docs.microsoft.com/azure/cost-management-billing/costs/cost-mgt-best-practices)
- [Terraform Cost Estimation](https://www.terraform.io/docs/cloud/cost-estimation/index.html)
- [Azure Reserved Instances](https://azure.microsoft.com/reservations/)

---

**Last Updated:** December 2025

**Related Documentation:**
- [components/terraform.md](../components/terraform.md) - Infrastructure configuration
- [components/azure-resources.md](../components/azure-resources.md) - Resource details
- [KNOWN_ISSUES.md](../KNOWN_ISSUES.md#cost-optimization-automation) - Future optimizations
