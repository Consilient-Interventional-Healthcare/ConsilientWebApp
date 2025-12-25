# Resource Naming Conventions

Standardized naming patterns for all Azure resources.

## Global Pattern

**Format:** `{project}-{type}-{name}-{environment}-{unique_suffix}`

**Example:** `consilient-api-app-dev-a1b2c3`

**Components:**
- `project` = "consilient" (fixed)
- `type` = resource category
- `name` = specific resource name
- `environment` = dev, staging, prod
- `unique_suffix` = 6-char hash (ensures global uniqueness)

**Defined in:** [`infra/terraform/locals.tf:32-65`](../../../infra/terraform/locals.tf#L32-L65)

## Unique Suffix Calculation

Generated from subscription ID + resource group name (deterministic):

```hcl
unique_suffix = substr(md5("${var.subscription_id}-${var.resource_group_name}"), 0, 6)
```

**Result:** Same suffix for same subscription + RG combination

**Why:** Ensures globally unique names (Azure requires this for some resources)

## Resource Type Abbreviations

| Resource | Type | Example |
|----------|------|---------|
| App Service | api, web | consilient-api-app-dev-x1y2z3 |
| Container Registry | acr | consilient-acr-dev-x1y2z3 |
| SQL Server | sql | consilient-sql-dev-x1y2z3 |
| Storage Account | st | consilientst dev x1y2z3 |
| Virtual Network | vnet | consilient-vnet-dev-x1y2z3 |
| Subnet | subnet | consilient-subnet-dev-x1y2z3 |
| Container App Env | cae | consilient-cae-dev-x1y2z3 |

## Database Naming

**Different pattern for databases** (no unique suffix):

```
consilient_{database}_{environment}
```

**Examples:**
- `consilient_main_dev` - Main app database (dev)
- `consilient_main_prod` - Main app database (prod)
- `consilient_hangfire_dev` - Background jobs (dev)
- `consilient_hangfire_prod` - Background jobs (prod)
- `consilient_customdb_dev` - Custom database (dev)

**Auto-Discovery:** Directory name maps to database name:
- Directory: `Main/` → `consilient_main_{env}`
- Directory: `Hangfire/` → `consilient_hangfire_{env}`
- Directory: `CustomDB/` → `consilient_customdb_{env}`

See [components/databases.md](../components/databases.md#naming-convention) for details.

## Environment Suffixes

| Environment | Code | Example |
|------------|------|---------|
| Development | dev | consilient-api-app-dev-x1y2z3 |
| Staging | staging | consilient-api-app-staging-x1y2z3 |
| Production | prod | consilient-api-app-prod-x1y2z3 |

## Examples by Resource Type

### App Services

**API:**
```
consilient-api-app-dev-a1b2c3
consilient-api-app-prod-a1b2c3
```

**React:**
```
consilient-react-app-dev-a1b2c3
consilient-react-app-prod-a1b2c3
```

### Container Registry

```
consilientacrdeveca75c    (dev)
consilientacrprode74c2a   (prod)
```

Note: ACR names don't use hyphens (Azure requirement)

### SQL Server

```
consilient-sql-dev-a1b2c3
consilient-sql-prod-a1b2c3
```

### Storage Account

```
consilientstdevx1y2z3     (dev)
consilientstprodx1y2z3    (prod)
```

Note: Storage names don't use hyphens, lowercase only

## Implementation

All naming logic centralized in [`locals.tf`](../../../infra/terraform/locals.tf):

```hcl
locals {
  # Naming components
  unique_suffix = substr(md5("${var.subscription_id}-${var.resource_group_name}"), 0, 6)

  # Resource names
  api_app_name = "consilient-api-app-${var.environment}-${local.unique_suffix}"
  react_app_name = "consilient-react-app-${var.environment}-${local.unique_suffix}"
  acr_name = "consilient-acr-${var.environment}-${local.unique_suffix}"

  # Database names
  main_database_name = "consilient_main_${var.environment}"
  hangfire_database_name = "consilient_hangfire_${var.environment}"
}
```

**Benefits:**
- Single source of truth
- Easy to change naming convention
- Consistent across all resources
- Deterministic (same inputs = same names)

## Best Practices

1. **Keep Names Short** - 30-63 character limit on most Azure resources
2. **Use Meaningful Names** - Clearly indicate resource purpose
3. **Avoid Sensitive Data** - Don't include passwords, keys, or personal info
4. **Be Consistent** - Follow pattern for all resources
5. **Document Custom Names** - If deviating from pattern, document why

## Related Documentation

- [`infra/terraform/locals.tf`](../../../infra/terraform/locals.tf) - Implementation
- [reference/cost-management.md](cost-management.md) - Resource organization
- [components/terraform.md](../components/terraform.md) - Terraform guide
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System design

---

**Last Updated:** December 2025
**For Navigation:** See [README.md](../README.md)
