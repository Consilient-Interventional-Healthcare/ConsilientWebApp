# Consilient Infrastructure Documentation

This document provides comprehensive documentation for the Consilient Azure infrastructure, including Terraform configuration, naming conventions, cost management, and resource architecture.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Terraform Configuration](#terraform-configuration)
  - [Getting Started](#getting-started)
  - [Project Structure](#project-structure)
  - [Key Configuration Files](#key-configuration-files)
- [Naming Conventions](#naming-conventions)
  - [Naming Format](#naming-format)
  - [Resource Abbreviations](#resource-abbreviations)
  - [Globally Unique Resources](#globally-unique-resources)
  - [Usage in Terraform](#usage-in-terraform)
- [Cost Management](#cost-management)
  - [Cost Profiles](#cost-profiles)
  - [Cost Breakdown by Service](#cost-breakdown-by-service)
  - [How to Adjust Costs](#how-to-adjust-costs)
  - [Cost Optimization Tips](#cost-optimization-tips)
- [Infrastructure Resources](#infrastructure-resources)
  - [Networking](#networking)
  - [Compute](#compute)
  - [Databases](#databases)
  - [Storage](#storage)
  - [Monitoring](#monitoring)
  - [Container Services](#container-services)
- [Terraform Operations](#terraform-operations)
  - [Initial Provisioning](#initial-provisioning)
  - [Updating Infrastructure](#updating-infrastructure)
  - [Managing State](#managing-state)
- [Security Best Practices](#security-best-practices)
- [Troubleshooting](#troubleshooting)
- [Additional Resources](#additional-resources)

## Overview

The Consilient infrastructure is provisioned on Microsoft Azure using Terraform. The infrastructure supports three environments (dev, staging, prod) with different cost profiles and configurations optimized for each environment's needs.

### Infrastructure Principles

- **Infrastructure as Code**: All resources defined in Terraform
- **Environment Parity**: Consistent structure across dev/staging/prod
- **Cost Optimization**: Environment-specific SKUs to balance cost and performance
- **Single Source of Truth**: Centralized configuration in [locals.tf](../infra/terraform/locals.tf)
- **Security by Default**: TLS, encryption, and access controls enabled
- **Deterministic Naming**: Predictable, unique resource names

## Architecture

The Consilient application suite consists of:

- **Consilient.Api** - Main REST API (.NET 9.0) running on Azure App Service
- **Consilient.WebApp2** - React frontend application on Azure App Service
- **Consilient.BackgroundHost** - Background job processor
- **Azure SQL Database** - Two databases (main application and Hangfire)
- **Loki** - Log aggregation running on Azure Container Apps
- **Grafana** - Monitoring dashboards running on Azure Managed Grafana
- **Azure Container Registry** - Private Docker image registry

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        Azure Region                          │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Virtual Network (VNet)                     │ │
│  │                                                          │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │ │
│  │  │   React App  │  │   API App    │  │  Background  │ │ │
│  │  │ App Service  │  │ App Service  │  │     Host     │ │ │
│  │  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘ │ │
│  │         │                  │                  │         │ │
│  │         └──────────────────┼──────────────────┘         │ │
│  │                            │                            │ │
│  │  ┌─────────────────────────┼──────────────────────┐    │ │
│  │  │                         ▼                       │    │ │
│  │  │  ┌───────────────────────────────────────────┐ │    │ │
│  │  │  │    Azure SQL Server                       │ │    │ │
│  │  │  │  ┌─────────────┐  ┌──────────────────┐   │ │    │ │
│  │  │  │  │  Main DB    │  │  Hangfire DB     │   │ │    │ │
│  │  │  │  └─────────────┘  └──────────────────┘   │ │    │ │
│  │  │  └───────────────────────────────────────────┘ │    │ │
│  │  │                                                 │    │ │
│  │  │  ┌───────────────────────────────────────────┐ │    │ │
│  │  │  │  Storage Account (Loki)                   │ │    │ │
│  │  │  │  └─ Private Endpoint                      │ │    │ │
│  │  │  └───────────────────────────────────────────┘ │    │ │
│  │  └─────────────────────────────────────────────────┘    │ │
│  │                                                          │ │
│  │  ┌──────────────────────────────────────────────────┐  │ │
│  │  │  Container App Environment                        │  │ │
│  │  │  ┌──────────────┐                                │  │ │
│  │  │  │ Loki App     │                                │  │ │
│  │  │  └──────────────┘                                │  │ │
│  │  └──────────────────────────────────────────────────┘  │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Azure Container Registry (ACR)                      │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Azure Managed Grafana                               │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Terraform Configuration

### Getting Started

#### Prerequisites

- [Terraform](https://www.terraform.io/downloads.html) >= 1.5.0
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- Azure subscription with Contributor role or higher

#### Initial Setup

1. **Login to Azure**:
   ```bash
   az login
   az account set --subscription <subscription-id>
   ```

2. **Navigate to Terraform directory**:
   ```bash
   cd infra/terraform
   ```

3. **Initialize Terraform**:
   ```bash
   terraform init
   ```

4. **Create a `terraform.tfvars` file**:
   ```hcl
   subscription_id      = "your-subscription-id"
   region               = "eastus"
   environment          = "dev"
   resource_group_name  = "consilient-rg-dev"
   sql_admin_username   = "sqladmin"
   sql_admin_password   = "YourSecureP@ssw0rd123!"
   ```

   **IMPORTANT**: Never commit `terraform.tfvars` to source control! Add it to `.gitignore`.

5. **Plan and Apply**:
   ```bash
   terraform plan -out=tfplan
   terraform apply tfplan
   ```

### Project Structure

```
infra/terraform/
├── main.tf                    # Main configuration, providers
├── variables.tf               # Variable definitions
├── locals.tf                  # Centralized configuration (CRITICAL)
├── outputs.tf                 # Output values
├── backend.tf                 # Backend configuration for state
│
├── network.tf                 # Virtual Network, subnets
├── acr.tf                     # Azure Container Registry
├── api_app.tf                 # API App Service
├── react_app.tf               # React App Service
├── sql.tf                     # SQL Server and databases
├── storage.tf                 # Storage accounts
├── loki.tf                    # Loki container app
├── grafana.tf                 # Grafana monitoring
│
└── modules/
    ├── app_service/           # Reusable App Service module
    ├── sql_database/          # Reusable SQL Database module
    └── storage_account/       # Reusable Storage Account module
```

### Key Configuration Files

#### locals.tf - Single Source of Truth

The [locals.tf](../infra/terraform/locals.tf) file is the **most important file** in the Terraform configuration. It contains:

- **Naming conventions** for all resources
- **SKU/tier selections** per environment
- **Cost profiles** and estimates
- **Environment-specific settings**
- **Resource tags**

**Why centralized?**
- Change names in one place
- Easy cost adjustments
- Consistent patterns
- Self-documenting
- Reduces errors
- DRY principle

**Structure**:
```hcl
locals {
  # Unique suffix for globally unique resources
  unique_suffix = substr(md5("${var.subscription_id}-${var.resource_group_name}"), 0, 6)

  # Default SKU options per environment
  default_skus = {
    dev     = { ... }
    staging = { ... }
    prod    = { ... }
  }

  # Network configuration
  network = { ... }

  # API App Service configuration
  api = { ... }

  # React App Service configuration
  react = { ... }

  # SQL configuration
  sql = { ... }

  # Cost estimates
  estimated_monthly_cost = { ... }

  # Resource tags
  tags = { ... }
}
```

#### variables.tf - Input Variables

Defines all configurable inputs:

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `project_name` | Project prefix for resources | No | `consilient` |
| `region` | Azure region | Yes | - |
| `environment` | Environment (dev/staging/prod) | Yes | - |
| `resource_group_name` | Resource group name | Yes | - |
| `subscription_id` | Azure subscription ID | Yes | - |
| `sql_admin_username` | SQL admin username | Yes | - |
| `sql_admin_password` | SQL admin password (sensitive) | Yes | - |
| `loki_retention` | Loki log retention period | No | `30d` |

**Security Note**: Use environment variables for sensitive values:
```bash
export TF_VAR_sql_admin_password="YourSecureP@ssw0rd123!"
```

#### outputs.tf - Output Values

Exposes important values after deployment:

```hcl
# Cost transparency
output "estimated_monthly_cost_usd" {
  description = "Estimated monthly cost in USD for this environment"
  value       = local.estimated_monthly_cost[var.environment]
}

output "cost_configuration" {
  description = "Detailed cost configuration for this environment"
  value = {
    api_app_sku           = local.api.sku
    react_app_sku         = local.react.sku
    main_db_sku           = local.sql.main_db.sku
    hangfire_db_sku       = local.sql.hangfire_db.sku
    acr_sku               = local.acr.sku
    sql_auditing          = local.sql.auditing_enabled[var.environment]
    sql_threat_protection = local.sql.threat_protection_enabled[var.environment]
  }
}
```

View outputs after deployment:
```bash
terraform output
terraform output -json > outputs.json
```

## Naming Conventions

All resource names are defined in [locals.tf](../infra/terraform/locals.tf) within the `local` blocks. This provides a **single source of truth** for all resource names.

### Naming Format

```
<project>-<resource-type-prefix>-<resource-name>-<environment>-<unique-suffix-if-needed>
```

**Components:**
- **project**: Project name prefix (default: `consilient`)
- **resource-type-prefix**: Azure resource abbreviation (e.g., `asp`, `sqlsrv`)
- **resource-name**: Descriptive name of the resource's purpose
- **environment**: Deployment environment (`dev`, `staging`, `prod`)
- **unique-suffix**: 6-character hash for globally unique resources

**Note**: For resources that don't allow dashes (ACR, Storage Accounts), names are concatenated without dashes.

### Resource Abbreviations

| Resource Type | Prefix | Example |
|--------------|--------|---------|
| Virtual Network | `vnet` | `consilient-vnet-dev` |
| Subnet | `subnet` | `consilient-subnet-dev` |
| App Service Plan | `asp` | `consilient-asp-api-dev` |
| App Service | (name) | `consilient-api-dev` |
| Container Registry | `acr` | `consilientacrdev123abc` |
| Container App Environment | `cae` | `consilient-cae-shared-dev` |
| SQL Server | `sqlsrv` | `consilient-sqlsrv-dev-123abc` |
| Storage Account | (name) | `consilientlokidev123abc` |
| Private Endpoint | `pe` | `consilient-pe-loki-storage-dev` |
| Private Service Connection | `psc` | `consilient-psc-loki-storage-dev` |
| Managed Identity | (name)-identity | `consilient-loki-identity-dev` |
| Grafana | `grafana` | `consilient-grafana-dev` |

### Globally Unique Resources

These Azure resources require globally unique names across all of Azure:

- **Container Registry** (ACR)
- **SQL Server** (hostname)
- **Storage Account**

For these resources, we append a 6-character unique suffix generated from:
```hcl
substr(md5("${subscription_id}-${resource_group_name}"), 0, 6)
```

**This ensures:**
- Names are unique across Azure
- Names are deterministic (same inputs = same suffix)
- Names are short enough to meet Azure naming length requirements

**Example**:
```
Subscription ID: abc123...
Resource Group: consilient-rg-dev
Hash: md5("abc123...-consilient-rg-dev") = "f4a7b2..."
Suffix: "f4a7b2" (first 6 chars)
ACR Name: consilientacrdevf4a7b2
```

### Usage in Terraform

#### Reference Centralized Names

**Instead of hardcoding names:**
```hcl
# ❌ Don't do this
name = "api-${var.environment}"
```

**Use the centralized names:**
```hcl
# ✅ Do this
name = local.api.service_name
```

#### Adding New Resources

1. **Add configuration to [locals.tf](../infra/terraform/locals.tf)**:
   ```hcl
   my_resource = {
     name = "${var.project_name}-myresource-${var.environment}"
     sku  = local.default_skus[var.environment].some_sku
   }
   ```

2. **Reference it in your resource definition**:
   ```hcl
   resource "azurerm_resource" "example" {
     name = local.my_resource.name
     sku  = local.my_resource.sku
     tags = local.tags
   }
   ```

#### Modifying Names

**WARNING**: Changing names in [locals.tf](../infra/terraform/locals.tf) will cause resources to be recreated!

1. Update the name in [locals.tf](../infra/terraform/locals.tf)
2. Run `terraform plan` to see the impact
3. Resources with new names will be created
4. Old resources will be destroyed

**To avoid recreation**, use `terraform state mv` to rename in state:
```bash
terraform state mv azurerm_resource.old azurerm_resource.new
```

#### Tags

All resources use centralized tags from [locals.tf](../infra/terraform/locals.tf):
```hcl
tags = {
  environment = var.environment
  project     = var.project_name
  managed_by  = "terraform"
}
```

**Usage**:
```hcl
resource "azurerm_resource" "example" {
  tags = local.tags
}
```

**For resource-specific tags**, merge with base tags:
```hcl
tags = merge(local.tags, {
  criticality = "high"
  compliance  = "hipaa"
})
```

## Cost Management

All cost-related settings are centralized in [locals.tf](../infra/terraform/locals.tf). This makes it easy to view all cost-impacting decisions in one place and adjust resource tiers.

### Cost Profiles

We have three pre-configured cost profiles optimized for different use cases:

#### Development (~$200/month)
- **Purpose**: Development and testing
- **Focus**: Minimal cost, serverless where possible
- **App Services**: Basic tier (B1) - ~$13/month each
- **Databases**: Serverless with auto-pause - ~$150/month (when active)
- **Container Registry**: Basic - ~$5/month
- **Security**: Disabled (auditing, threat protection)
- **Redundancy**: None
- **Best for**: Local development, feature testing, experimentation

#### Staging (~$1,200/month)
- **Purpose**: Pre-production testing
- **Focus**: Balance between cost and production-like environment
- **App Services**: Premium v2 (P1v2) - ~$146/month each
- **Databases**: Mix of General Purpose and Serverless
- **Container Registry**: Standard - ~$20/month
- **Security**: Enabled (90-day audit retention)
- **Redundancy**: Single zone
- **Best for**: QA testing, user acceptance testing, performance testing

#### Production (~$2,800/month)
- **Purpose**: Production workloads
- **Focus**: High availability and performance
- **App Services**: Premium v3 (P2v3) - ~$204/month each
- **Databases**: General Purpose with higher vCores
- **Container Registry**: Premium - ~$40/month (supports geo-replication)
- **Security**: Fully enabled (365-day audit retention)
- **Redundancy**: Zone redundant for critical databases
- **Best for**: Live customer traffic, business-critical operations

### Cost Breakdown by Service

| Service | Dev | Staging | Production |
|---------|-----|---------|------------|
| **API App Service** | B1 (~$13) | P1v2 (~$146) | P2v3 (~$204) |
| **React App Service** | B1 (~$13) | P1v2 (~$146) | P2v3 (~$204) |
| **Container Registry** | Basic (~$5) | Standard (~$20) | Premium (~$40) |
| **Main Database** | GP_S_Gen5_2 Serverless (~$150*) | GP_Gen5_2 (~$650) | GP_Gen5_4 (~$1,300) |
| **Hangfire Database** | GP_S_Gen5_2 Serverless (~$150*) | GP_S_Gen5_2 Serverless (~$150*) | GP_Gen5_2 (~$650) |
| **SQL Auditing** | Disabled ($0) | Enabled (~$10) | Enabled (~$15) |
| **Container Apps** | Minimal (~$5) | Minimal (~$10) | Minimal (~$20) |
| **Storage** | Standard LRS (~$5) | Standard ZRS (~$10) | Standard ZRS (~$20) |
| **Grafana** | Free tier ($0) | Standard (~$50) | Standard (~$50) |
| **Network** | Minimal (~$5) | Standard (~$20) | Standard (~$50) |
| **Total (approx)** | **~$200** | **~$1,200** | **~$2,800** |

*Serverless databases only cost when active. These estimates assume ~50% utilization. Actual costs may vary based on usage patterns.

### How to Adjust Costs

#### Method 1: Change Environment

The easiest way to adjust costs is to use the appropriate environment:

```hcl
# In terraform.tfvars
environment = "dev"      # ~$200/month
environment = "staging"  # ~$1,200/month
environment = "prod"     # ~$2,800/month
```

#### Method 2: Customize Cost Profile in locals.tf

Edit [locals.tf](../infra/terraform/locals.tf) to adjust settings for a specific environment:

```hcl
# Example: Reduce production costs
default_skus = {
  prod = {
    app_service_plan   = "P1v3"      # Down from P2v3 (saves ~$100/month)
    sql_provisioned    = "GP_Gen5_2" # Down from GP_Gen5_4 (saves ~$650/month)
    container_registry = "Standard"  # Down from Premium (saves ~$20/month)
  }
}
```

#### Method 3: Temporarily Scale Down

For non-production environments:

```hcl
# Temporary dev cost reduction
default_skus = {
  dev = {
    app_service_plan = "F1"  # Free tier (limited features, good for nights/weekends)
  }
}
```

**Note**: Free tier has limitations (60 CPU minutes/day, no custom domains, etc.)

#### Method 4: View Cost Configuration

After deployment, view the cost estimates:

```bash
terraform output estimated_monthly_cost_usd
terraform output cost_configuration
```

Example output:
```
estimated_monthly_cost_usd = 200
cost_configuration = {
  "acr_sku" = "Basic"
  "api_app_sku" = "B1"
  "hangfire_db_sku" = "GP_S_Gen5_2"
  "main_db_sku" = "GP_S_Gen5_2"
  "react_app_sku" = "B1"
  "sql_auditing" = false
  "sql_threat_protection" = false
}
```

### Cost Optimization Tips

#### 1. Use Serverless Databases (Dev/Staging)

**Benefits:**
- **Auto-pause**: Databases pause when idle (after 60-120 minutes)
- **Pay-per-use**: Only pay when active
- **Cost savings**: Up to 70% vs always-on
- **Auto-resume**: Resumes automatically on first connection

**Best for**: Development, testing, intermittent workloads

**Configuration in [locals.tf](../infra/terraform/locals.tf)**:
```hcl
sql = {
  main_db = {
    sku = var.environment == "prod" ? "GP_Gen5_4" : "GP_S_Gen5_2"
    min_capacity = {
      dev     = 0.5  # Minimum 0.5 vCores when active
      staging = 0.5
      prod    = null # Not serverless
    }
    auto_pause_delay = {
      dev     = 60   # Pause after 1 hour
      staging = 120  # Pause after 2 hours
      prod    = null # Not serverless
    }
  }
}
```

#### 2. Disable SQL Auditing in Dev

**Savings**: ~$50/month in storage costs

**Trade-off**: No audit logs (acceptable for development)

**Configuration**:
```hcl
sql = {
  auditing_enabled = {
    dev     = false
    staging = true
    prod    = true
  }
}
```

#### 3. Use Basic Tier in Dev

**Savings**: ~$260/month vs Premium

**Trade-off**:
- Less CPU/memory
- No auto-scaling
- No deployment slots
- Acceptable for development

**Configuration**:
```hcl
default_skus = {
  dev = {
    app_service_plan = "B1"  # Basic
  }
}
```

#### 4. Right-size Production

Don't over-provision:
- **Monitor actual usage** in Azure Portal
- **Start with recommended tiers** from cost profiles
- **Scale up only when needed** based on metrics
- **Use auto-scaling** where possible

**Key metrics to watch**:
- CPU > 80% consistently → Scale up
- Memory > 85% consistently → Scale up
- CPU < 30% for 7+ days → Scale down
- Database DTU > 80% → Scale up

#### 5. Use Azure Cost Management

**Set up budgets and alerts**:
```bash
# Create budget with alert
az consumption budget create \
  --budget-name "consilient-dev-budget" \
  --amount 250 \
  --time-grain Monthly \
  --category Cost
```

**Recommended alert thresholds**:
- Dev: Alert at $250/month (125% of estimate)
- Staging: Alert at $1,500/month (125% of estimate)
- Production: Alert at $3,500/month (125% of estimate)

#### 6. Consider Azure Reservations (Production)

**Savings**: Up to 72% for 1-year or 3-year commitment

**Best for**:
- Stable production workloads
- Predictable usage patterns
- Long-term projects

**Resources to reserve**:
- App Service Plan (P-series)
- SQL Database (General Purpose)

#### 7. Delete Unused Environments

**Action**: Destroy dev/staging environments when not in use:
```bash
terraform destroy
```

**Restore**: Re-provision when needed:
```bash
terraform apply
```

**Consideration**: Database data will be lost unless backed up

#### 8. Enable Azure Hybrid Benefit

If you have existing SQL Server licenses:
```hcl
resource "azurerm_mssql_database" "example" {
  license_type = "BasePrice"  # Use existing license
}
```

**Savings**: Up to 55% on SQL Database costs

### Monthly Cost Review Process

**Week 1**: Review actual costs vs estimates
- Check Azure Cost Management dashboard
- Compare to Terraform output estimates
- Identify discrepancies

**Week 2**: Identify cost anomalies
- Look for unexpected spikes
- Check for resources not in Terraform
- Review data transfer costs

**Week 3**: Adjust profiles if needed
- Update [locals.tf](../infra/terraform/locals.tf) if over budget
- Consider scaling down unused resources
- Test changes in dev first

**Week 4**: Implement and monitor
- Apply changes to production
- Set up alerts if not already configured
- Document changes

## Infrastructure Resources

### Networking

**Virtual Network (VNet)**
- **File**: [network.tf](../infra/terraform/network.tf)
- **Purpose**: Private network for all resources
- **Address Space**: Environment-specific CIDR block
- **Subnets**:
  - Application subnet (App Services)
  - Database subnet (SQL, storage)
  - Container Apps subnet (Loki)
- **Features**: Network security groups, service endpoints

**Configuration**:
```hcl
# In locals.tf
network = {
  vnet = {
    name = "${var.project_name}-vnet-${var.environment}"
  }
  subnet = {
    name = "${var.project_name}-subnet-${var.environment}"
  }
}
```

**Private Endpoints**
- Secure connections to PaaS services
- Loki Storage Account uses private endpoint
- Database can use private endpoint (optional, additional cost)

### Compute

#### API App Service

- **File**: [api_app.tf](../infra/terraform/api_app.tf)
- **Purpose**: Hosts the .NET 9.0 REST API
- **Platform**: Azure App Service (Linux)
- **Container**: Docker container from ACR
- **SKU**: Environment-specific (B1/P1v2/P2v3)

**Features**:
- Docker container support
- Health checks (`/health` endpoint)
- Auto-scaling (Premium tiers)
- Deployment slots (Premium tiers)
- VNet integration
- Managed certificates (SSL/TLS)

**Configuration**:
```hcl
# In locals.tf
api = {
  service_plan_name = "${var.project_name}-asp-api-${var.environment}"
  service_name      = "${var.project_name}-api-${var.environment}"
  sku               = local.default_skus[var.environment].app_service_plan
}
```

**App Settings** (configured separately):
- `ASPNETCORE_ENVIRONMENT`
- `WEBSITES_PORT=8090`
- `ConnectionStrings__Default`
- `ConnectionStrings__Hangfire`

#### React App Service

- **File**: [react_app.tf](../infra/terraform/react_app.tf)
- **Purpose**: Hosts the React frontend
- **Platform**: Azure App Service (Linux)
- **SKU**: Environment-specific (B1/P1v2/P2v3)

**Features**:
- Static web hosting
- CDN integration (optional)
- Custom domains
- Managed certificates

**Configuration**:
```hcl
# In locals.tf
react = {
  service_plan_name = "${var.project_name}-asp-react-${var.environment}"
  service_name      = "${var.project_name}-react-${var.environment}"
  sku               = local.default_skus[var.environment].app_service_plan
}
```

### Databases

**SQL Server**
- **File**: [sql.tf](../infra/terraform/sql.tf)
- **Type**: Azure SQL Server (PaaS)
- **Version**: Latest (automatically updated)
- **Authentication**: SQL authentication
- **Firewall**: Allows Azure services
- **Security**: TLS 1.2+ enforced

**Configuration**:
```hcl
# In locals.tf
sql = {
  server_name = "${var.project_name}-sqlsrv-${var.environment}-${local.unique_suffix}"

  threat_protection_enabled = {
    dev     = false
    staging = true
    prod    = true
  }

  auditing_enabled = {
    dev     = false
    staging = true
    prod    = true
  }

  audit_retention_days = {
    dev     = 0
    staging = 90
    prod    = 365  # 1 year for compliance
  }
}
```

**Main Application Database**
- **Name Pattern**: `consilient_main_{environment}`
- **Purpose**: Primary application data
- **SKU**:
  - Dev: GP_S_Gen5_2 (Serverless, 0.5-2 vCores, auto-pause)
  - Staging: GP_Gen5_2 (Provisioned, 2 vCores)
  - Prod: GP_Gen5_4 (Provisioned, 4 vCores, zone redundant)

**Configuration**:
```hcl
sql = {
  main_db = {
    name = "${var.project_name}_main_${var.environment}"
    sku  = var.environment == "prod" ? "GP_Gen5_4" : "GP_S_Gen5_2"

    min_capacity = {
      dev     = 0.5
      staging = null  # Not serverless
      prod    = null  # Not serverless
    }

    auto_pause_delay = {
      dev     = 60    # 1 hour
      staging = null
      prod    = null
    }

    zone_redundant = {
      dev     = false
      staging = false
      prod    = true  # High availability
    }
  }
}
```

**Hangfire Background Jobs Database**
- **Name Pattern**: `consilient_hangfire_{environment}`
- **Purpose**: Background job scheduling and state
- **SKU**:
  - Dev: GP_S_Gen5_2 (Serverless)
  - Staging: GP_S_Gen5_2 (Serverless)
  - Prod: GP_Gen5_2 (Provisioned)

**Configuration**:
```hcl
sql = {
  hangfire_db = {
    name = "${var.project_name}_hangfire_${var.environment}"
    sku  = var.environment == "prod" ? "GP_Gen5_2" : "GP_S_Gen5_2"

    min_capacity = {
      dev     = 0.5
      staging = 0.5
      prod    = null
    }

    auto_pause_delay = {
      dev     = 60
      staging = 120   # 2 hours
      prod    = null
    }

    zone_redundant = {
      dev     = false
      staging = false
      prod    = false  # Not critical
    }
  }
}
```

### Storage

**Loki Storage Account**
- **File**: [storage.tf](../infra/terraform/storage.tf)
- **Purpose**: Stores log data for Loki
- **Type**: Azure Storage Account (General Purpose v2)
- **Redundancy**:
  - Dev: LRS (Locally Redundant)
  - Staging: ZRS (Zone Redundant)
  - Prod: ZRS (Zone Redundant)
- **Access**: Private endpoint only (secure)
- **Container**: `loki-data`

**Features**:
- Encryption at rest (default)
- HTTPS only
- Blob storage
- Lifecycle management (optional, for cost savings)

**Configuration**:
```hcl
# In locals.tf
loki = {
  storage = {
    account_name               = "${var.project_name}loki${var.environment}${local.unique_suffix}"
    container_name             = "loki-data"
    private_endpoint           = "${var.project_name}-pe-loki-storage-${var.environment}"
    private_service_connection = "${var.project_name}-psc-loki-storage-${var.environment}"
  }
}
```

### Monitoring

#### Azure Managed Grafana

- **File**: [grafana.tf](../infra/terraform/grafana.tf)
- **Purpose**: Visualization dashboards
- **Type**: Azure Managed Grafana (PaaS)
- **Tier**:
  - Dev: Free (limited features)
  - Staging/Prod: Standard
- **Data Sources**: Loki, Azure Monitor
- **Access**: RBAC-based (Azure AD integration)

**Features**:
- Pre-built dashboards
- Custom dashboard creation
- Alerting
- Multi-tenant support
- High availability (Standard tier)

**Configuration**:
```hcl
# In locals.tf
grafana = {
  name = "${var.project_name}-grafana-${var.environment}"
}
```

#### Loki (Log Aggregation)

- **File**: [loki.tf](../infra/terraform/loki.tf)
- **Platform**: Azure Container Apps
- **Purpose**: Log aggregation and indexing
- **Storage**: Azure Storage Account (blob)
- **Retention**: Configurable (default: 30 days)
- **Query Language**: LogQL

**Features**:
- Label-based indexing
- Horizontal scaling
- S3-compatible storage (Azure Blob)
- Grafana integration
- Low cost (indexes labels, not full text)

**Configuration**:
```hcl
# In locals.tf
loki = {
  container_app_env_name = "${var.project_name}-cae-shared-${var.environment}"
  container_app_name     = "${var.project_name}-loki-${var.environment}"
  identity_name          = "${var.project_name}-loki-identity-${var.environment}"
}
```

**Variables**:
- `loki_retention` - Retention period (default: 30d)
- `loki_cpu_request` - CPU allocation (default: 0.5)
- `loki_memory_request` - Memory allocation (default: 1.0Gi)

### Container Services

#### Azure Container Registry (ACR)

- **File**: [acr.tf](../infra/terraform/acr.tf)
- **Purpose**: Private Docker registry
- **Type**: Azure Container Registry
- **SKU**:
  - Dev: Basic (~$5/month, 10 GB storage)
  - Staging: Standard (~$20/month, 100 GB storage)
  - Prod: Premium (~$40/month, 500 GB storage, geo-replication)

**Images stored**:
- `consilientapi` - API Docker image
- `consilientwebapp2` - React app Docker image
- `consilientbackgroundhost` - Background jobs Docker image
- `loki` - Loki logging (optional, can use public image)

**Features**:
- Vulnerability scanning (Standard/Premium)
- Geo-replication (Premium only)
- Webhook notifications
- Azure AD authentication
- Managed identity support

**Configuration**:
```hcl
# In locals.tf
acr = {
  name = "${var.project_name}acr${var.environment}${local.unique_suffix}"
  sku  = local.default_skus[var.environment].container_registry
}
```

#### Container App Environment

- **File**: [loki.tf](../infra/terraform/loki.tf)
- **Purpose**: Shared environment for container apps
- **Type**: Azure Container Apps Environment
- **Apps**: Loki (more can be added)
- **Networking**: Integrated with VNet
- **Monitoring**: Application Insights integration

**Features**:
- Auto-scaling (based on HTTP, CPU, memory, custom metrics)
- Revision management
- Blue-green deployments
- Dapr integration (optional)
- Managed identity

**Configuration**:
```hcl
# In locals.tf
loki = {
  container_app_env_name = "${var.project_name}-cae-shared-${var.environment}"
}
```

**Can be shared** across multiple container apps for cost efficiency.

## Terraform Operations

### Initial Provisioning

1. **Prepare configuration**:
   ```bash
   cd infra/terraform
   cp terraform.tfvars.example terraform.tfvars
   # Edit terraform.tfvars
   ```

2. **Initialize Terraform**:
   ```bash
   terraform init
   ```

3. **Validate configuration**:
   ```bash
   terraform validate
   terraform fmt -check
   ```

4. **Plan infrastructure**:
   ```bash
   terraform plan -out=tfplan
   ```

5. **Review plan output**:
   - Check resources to be created
   - Verify naming conventions
   - Review cost estimates
   - Confirm SKU selections

6. **Apply infrastructure**:
   ```bash
   terraform apply tfplan
   ```

7. **Save outputs**:
   ```bash
   terraform output -json > outputs.json
   ```

### Updating Infrastructure

#### Minor changes (SKU adjustments, configuration)

```bash
# Make changes to locals.tf or other .tf files
terraform plan
terraform apply
```

#### Major changes (adding resources)

```bash
# Add new resource configuration
terraform plan -out=tfplan
# Review carefully
terraform apply tfplan
```

#### Targeted updates (specific resource)

```bash
terraform plan -target=azurerm_app_service.api
terraform apply -target=azurerm_app_service.api
```

**Note**: Targeted applies can lead to inconsistent state. Use sparingly.

### Managing State

#### Remote State (Recommended)

Configure remote state in [backend.tf](../infra/terraform/backend.tf):

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "terraform-state-rg"
    storage_account_name = "tfstate12345"
    container_name       = "tfstate"
    key                  = "consilient.terraform.tfstate"
  }
}
```

**Benefits**:
- Team collaboration
- State locking
- Versioning
- Backup/recovery

#### State Commands

**List resources**:
```bash
terraform state list
```

**Show resource details**:
```bash
terraform state show azurerm_app_service.api
```

**Move resource** (rename without recreating):
```bash
terraform state mv azurerm_app_service.old azurerm_app_service.new
```

**Remove resource** (from state only, doesn't delete):
```bash
terraform state rm azurerm_app_service.api
```

**Import existing resource**:
```bash
terraform import azurerm_resource_group.main /subscriptions/{sub-id}/resourceGroups/{rg-name}
```

### Workspaces (Optional)

Use workspaces for managing multiple environments with same code:

```bash
# Create workspaces
terraform workspace new dev
terraform workspace new staging
terraform workspace new prod

# Switch workspace
terraform workspace select dev

# List workspaces
terraform workspace list
```

**Alternative**: Use separate directories per environment (simpler).

## Security Best Practices

### 1. Secrets Management

**Never commit secrets to source control**:
```bash
# Add to .gitignore
*.tfvars
*.tfvars.json
.terraform/
terraform.tfstate*
```

**Use environment variables**:
```bash
export TF_VAR_sql_admin_password="SecurePassword123!"
terraform apply
```

**Use Azure Key Vault** (recommended for production):
```hcl
data "azurerm_key_vault_secret" "sql_password" {
  name         = "sql-admin-password"
  key_vault_id = data.azurerm_key_vault.main.id
}

resource "azurerm_mssql_server" "main" {
  administrator_login_password = data.azurerm_key_vault_secret.sql_password.value
}
```

### 2. Network Security

**Enable VNet integration**:
```hcl
resource "azurerm_app_service" "api" {
  # ... other config ...

  site_config {
    vnet_route_all_enabled = true
  }
}
```

**Use private endpoints** for databases and storage:
```hcl
resource "azurerm_private_endpoint" "sql" {
  name                = "${local.sql.server_name}-pe"
  location            = var.region
  resource_group_name = azurerm_resource_group.main.name
  subnet_id           = azurerm_subnet.main.id

  private_service_connection {
    name                           = "${local.sql.server_name}-psc"
    private_connection_resource_id = azurerm_mssql_server.main.id
    subresource_names             = ["sqlServer"]
    is_manual_connection          = false
  }
}
```

**Configure NSG rules** to restrict traffic:
```hcl
resource "azurerm_network_security_rule" "allow_https" {
  name                        = "AllowHTTPS"
  priority                    = 100
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range          = "*"
  destination_port_range     = "443"
  source_address_prefix      = "*"
  destination_address_prefix = "*"
  resource_group_name        = azurerm_resource_group.main.name
  network_security_group_name = azurerm_network_security_group.main.name
}
```

### 3. Identity and Access Management

**Use Managed Identity** instead of connection strings:
```hcl
resource "azurerm_app_service" "api" {
  identity {
    type = "SystemAssigned"
  }
}

# Grant access to SQL
resource "azurerm_sql_active_directory_administrator" "main" {
  server_name         = azurerm_mssql_server.main.name
  resource_group_name = azurerm_resource_group.main.name
  login               = azurerm_app_service.api.identity[0].principal_id
  tenant_id           = data.azurerm_client_config.current.tenant_id
  object_id           = azurerm_app_service.api.identity[0].principal_id
}
```

**Implement RBAC**:
- Least privilege principle
- Separate roles for dev/staging/prod
- Service principals for automation
- Azure AD authentication

### 4. Data Protection

**Enable encryption at rest** (enabled by default):
```hcl
resource "azurerm_storage_account" "loki" {
  enable_https_traffic_only = true
  min_tls_version          = "TLS1_2"

  # Optional: Customer-managed keys
  # identity {
  #   type = "SystemAssigned"
  # }
}
```

**Enable SQL TDE** (Transparent Data Encryption, enabled by default):
```hcl
resource "azurerm_mssql_database" "main" {
  # TDE is enabled by default
  # Optional: Customer-managed key
}
```

**Enable backup retention**:
```hcl
resource "azurerm_mssql_database" "main" {
  short_term_retention_policy {
    retention_days = 7
  }

  long_term_retention_policy {
    weekly_retention  = "P1W"
    monthly_retention = "P1M"
    yearly_retention  = "P1Y"
  }
}
```

### 5. Monitoring and Auditing

**Enable SQL auditing** (staging/prod):
```hcl
resource "azurerm_mssql_server_extended_auditing_policy" "main" {
  count                = local.sql.auditing_enabled[var.environment] ? 1 : 0
  server_id            = azurerm_mssql_server.main.id
  storage_endpoint     = azurerm_storage_account.audit.primary_blob_endpoint
  retention_in_days    = local.sql.audit_retention_days[var.environment]
}
```

**Enable threat protection** (staging/prod):
```hcl
resource "azurerm_mssql_server_security_alert_policy" "main" {
  count                = local.sql.threat_protection_enabled[var.environment] ? 1 : 0
  resource_group_name  = azurerm_resource_group.main.name
  server_name          = azurerm_mssql_server.main.name
  state                = "Enabled"
  email_account_admins = true
}
```

**Enable diagnostic logs**:
```hcl
resource "azurerm_monitor_diagnostic_setting" "app_service" {
  name                       = "${local.api.service_name}-diag"
  target_resource_id         = azurerm_app_service.api.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  log {
    category = "AppServiceHTTPLogs"
    enabled  = true
  }

  metric {
    category = "AllMetrics"
    enabled  = true
  }
}
```

### 6. Compliance

**Tag resources for compliance**:
```hcl
tags = merge(local.tags, {
  compliance  = "hipaa"
  criticality = "high"
  data_class  = "confidential"
})
```

**Document compliance requirements** in code comments and README.

**Regular reviews**:
- Quarterly security audits
- Access reviews
- Vulnerability scanning
- Penetration testing (production)

## Troubleshooting

### Terraform Errors

#### Error: "Resource already exists"

**Cause**: Resource created outside of Terraform or in different state

**Solutions**:

1. **Import the resource**:
   ```bash
   terraform import azurerm_resource_group.main /subscriptions/{sub-id}/resourceGroups/{name}
   ```

2. **Delete and recreate** (if safe):
   ```bash
   az group delete --name {resource-group}
   terraform apply
   ```

3. **Rename in Terraform** to avoid conflict:
   ```hcl
   # Change resource name in locals.tf
   resource_group_name = "consilient-rg-dev-2"
   ```

#### Error: "Name already in use" (ACR, SQL Server, Storage)

**Cause**: Globally unique name is taken

**Solutions**:

1. **Check the unique suffix** calculation in [locals.tf](../infra/terraform/locals.tf):
   ```hcl
   unique_suffix = substr(md5("${var.subscription_id}-${var.resource_group_name}"), 0, 6)
   ```

2. **Change project name**:
   ```hcl
   # In terraform.tfvars
   project_name = "consilient2"
   ```

3. **Manually verify availability**:
   ```bash
   # Check ACR
   az acr check-name --name consilientacrdev123abc

   # Check SQL Server
   az sql server list --query "[?name=='consilient-sqlsrv-dev-123abc']"

   # Check Storage Account
   az storage account check-name --name consilientlokidev123abc
   ```

#### Error: "Invalid SKU"

**Cause**: SKU not available in selected region or tier

**Solutions**:

1. **Check available SKUs**:
   ```bash
   # App Service
   az appservice list-locations --sku P1v2

   # SQL Database
   az sql db list-editions --location eastus --output table
   ```

2. **Change region** in terraform.tfvars:
   ```hcl
   region = "westus2"  # Try different region
   ```

3. **Use different SKU** in [locals.tf](../infra/terraform/locals.tf):
   ```hcl
   default_skus = {
     dev = {
       app_service_plan = "B1"  # Instead of F1
     }
   }
   ```

#### Error: "Insufficient permissions"

**Cause**: Service principal or user lacks required permissions

**Solutions**:

1. **Check current permissions**:
   ```bash
   az role assignment list --assignee {user-or-sp-id}
   ```

2. **Grant Contributor role**:
   ```bash
   az role assignment create \
     --role "Contributor" \
     --assignee {user-or-sp-id} \
     --scope /subscriptions/{subscription-id}
   ```

3. **Grant specific permissions**:
   ```bash
   # For SQL admin
   az role assignment create \
     --role "SQL DB Contributor" \
     --assignee {user-or-sp-id} \
     --scope /subscriptions/{subscription-id}/resourceGroups/{rg}
   ```

#### Error: "State lock"

**Cause**: Another Terraform process is running or crashed

**Solutions**:

1. **Wait for lock to release** (if another process is running)

2. **Force unlock** (if process crashed):
   ```bash
   terraform force-unlock {lock-id}
   ```

3. **Check Azure Storage** (if using remote state):
   ```bash
   az storage blob list \
     --account-name tfstate12345 \
     --container-name tfstate \
     --query "[?name contains 'lock']"
   ```

### Azure Resource Errors

#### SQL Server firewall blocking connections

**Symptoms**: Can't connect to SQL Server

**Solutions**:

1. **Allow Azure services**:
   ```bash
   az sql server firewall-rule create \
     --resource-group {rg} \
     --server {server} \
     --name AllowAzureServices \
     --start-ip-address 0.0.0.0 \
     --end-ip-address 0.0.0.0
   ```

2. **Add your IP**:
   ```bash
   MY_IP=$(curl -s ifconfig.me)
   az sql server firewall-rule create \
     --resource-group {rg} \
     --server {server} \
     --name AllowMyIP \
     --start-ip-address $MY_IP \
     --end-ip-address $MY_IP
   ```

#### App Service won't start

**Symptoms**: App shows "Application Error" or 503

**Solutions**:

1. **Check logs**:
   ```bash
   az webapp log tail --name {app-name} --resource-group {rg}
   ```

2. **Verify app settings**:
   ```bash
   az webapp config appsettings list --name {app-name} --resource-group {rg}
   ```

3. **Check container logs** (if using containers):
   ```bash
   az webapp log download --name {app-name} --resource-group {rg}
   ```

4. **Restart app**:
   ```bash
   az webapp restart --name {app-name} --resource-group {rg}
   ```

#### Container Registry authentication fails

**Symptoms**: Can't pull images from ACR

**Solutions**:

1. **Enable admin user** (temporary, for testing):
   ```bash
   az acr update --name {acr-name} --admin-enabled true
   az acr credential show --name {acr-name}
   ```

2. **Use managed identity** (recommended):
   ```bash
   az webapp identity assign --name {app-name} --resource-group {rg}
   az acr show --name {acr-name} --query id -o tsv
   # Grant AcrPull role to managed identity
   ```

3. **Check service principal**:
   ```bash
   az ad sp credential list --id {sp-id}
   ```

### Cost Issues

#### Unexpected high costs

**Solutions**:

1. **Review Azure Cost Management**:
   ```bash
   az consumption usage list --start-date 2025-01-01 --end-date 2025-01-31
   ```

2. **Check for over-provisioned resources**:
   - View actual CPU/memory usage in Portal
   - Look for unused App Services
   - Check database DTU usage

3. **Verify serverless databases are pausing**:
   ```bash
   az sql db show --name {db-name} --server {server} --resource-group {rg} \
     --query status
   ```

4. **Look for unexpected data transfer**:
   - Check networking costs
   - Review bandwidth usage
   - Consider VNet peering costs

#### Budget exceeded

**Solutions**:

1. **Scale down immediately**:
   ```bash
   # Scale down App Service
   az appservice plan update --name {plan} --resource-group {rg} --sku B1

   # Scale down database
   az sql db update --name {db} --server {server} --resource-group {rg} \
     --service-objective S0
   ```

2. **Delete non-production environments**:
   ```bash
   terraform workspace select dev
   terraform destroy
   ```

3. **Enable auto-pause** on databases:
   ```bash
   az sql db update --name {db} --server {server} --resource-group {rg} \
     --auto-pause-delay 60 --min-capacity 0.5
   ```

## Additional Resources

### Azure Documentation
- [Azure App Service](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure SQL Database](https://learn.microsoft.com/en-us/azure/azure-sql/)
- [Azure Container Registry](https://docs.microsoft.com/en-us/azure/container-registry/)
- [Azure Container Apps](https://docs.microsoft.com/en-us/azure/container-apps/)
- [Azure Cost Management](https://docs.microsoft.com/en-us/azure/cost-management-billing/)
- [Azure Virtual Networks](https://docs.microsoft.com/en-us/azure/virtual-network/)

### Terraform Documentation
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Terraform Best Practices](https://www.terraform.io/docs/cloud/guides/recommended-practices/)
- [Terraform State Management](https://www.terraform.io/docs/language/state/)

### Pricing Calculators
- [Azure Pricing Calculator](https://azure.microsoft.com/en-us/pricing/calculator/)
- [App Service Pricing](https://azure.microsoft.com/en-us/pricing/details/app-service/)
- [SQL Database Pricing](https://azure.microsoft.com/en-us/pricing/details/sql-database/)
- [Container Registry Pricing](https://azure.microsoft.com/en-us/pricing/details/container-registry/)

### Project Files
- Infrastructure code: [infra/terraform/](../infra/terraform/)
- Naming conventions: [infra/terraform/locals.tf](../infra/terraform/locals.tf)
- Cost profiles: [infra/terraform/locals.tf](../infra/terraform/locals.tf)