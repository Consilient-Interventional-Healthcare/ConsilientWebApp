# Quick Start Guide

Fast-track guides for common infrastructure tasks. Each section designed to take 5-20 minutes.

## 1. Deploy Infrastructure to Dev (~10 min)

Deploy the complete Azure infrastructure for development environment.

### Prerequisites
- Azure CLI installed and authenticated: `az login`
- Terraform installed: `terraform version`
- Azure subscription access

### Steps

#### Step 1: Configure Terraform Variables
```powershell
cd infra/terraform
copy terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars` with your values:
```hcl
environment           = "dev"
region                = "canadacentral"
subscription_id       = "your-subscription-id"
resource_group_name   = "consilient-rg-dev"
sql_admin_username    = "dbadmin"
sql_admin_password    = "YourSecurePassword123!"
```

See [reference/naming-conventions.md](reference/naming-conventions.md) for naming patterns.

#### Step 2: Initialize Terraform
```powershell
terraform init
```

This downloads the Azure provider and sets up the state file.

#### Step 3: Plan & Review
```powershell
terraform plan -out=tfplan
```

Review the output to see what resources will be created. Check [reference/cost-management.md](reference/cost-management.md) for cost estimates.

#### Step 4: Apply Configuration
```powershell
terraform apply tfplan
```

This creates all Azure resources. First run typically takes 10-15 minutes.

### Troubleshooting
- **Terraform init fails:** Check [`backend.tf`](../../../infra/terraform/backend.tf) configuration
- **Provider authentication error:** Run `az login` again
- **Resource already exists:** Check [TROUBLESHOOTING.md#terraform-errors](TROUBLESHOOTING.md#terraform-errors)

See [components/terraform.md](components/terraform.md) for detailed Terraform guide.

---

## 2. Deploy a Database Change (~5 min)

Deploy SQL changes to your database automatically via GitHub Actions.

### Prerequisites
- GitHub secrets configured (see [reference/secrets-checklist.md](reference/secrets-checklist.md))
- Database scripts in proper structure

### Script Organization

SQL scripts are auto-discovered from `src/Databases/` directory structure:

```
src/Databases/
├── Main/                    # consilient_main_dev, consilient_main_prod
│   ├── Schema/
│   │   ├── 001_create_users_table.sql
│   │   └── 002_create_orders_table.sql
│   └── Seeds/
│       └── seed_users.sql   # Only run with recreate flag in dev
├── Hangfire/                # consilient_hangfire_dev, consilient_hangfire_prod
│   └── Schema/
│       └── 001_create_job_table.sql
└── CustomDB/                # Auto-creates consilient_customdb_dev, etc.
    └── Schema/
        └── 001_setup.sql
```

### Steps

#### Step 1: Create/Modify SQL Script
Create your SQL file in the appropriate database folder under `src/Databases/`:

```sql
-- src/Databases/Main/Schema/003_add_email_column.sql
ALTER TABLE Users
ADD Email NVARCHAR(255);
```

**Naming Conventions:**
- Schema scripts: `NNN_description.sql` (executed in order)
- Seed scripts: Start with `seed_` (skipped in production)
- System scripts: Start with `_` or `.` (skipped)

#### Step 2: Commit & Push
```bash
git add src/Databases/
git commit -m "Add email column to Users table"
git push origin main
```

#### Step 3: Monitor Workflow
Go to GitHub → Actions → Databases workflow to see progress.

The [`databases.yml`](../../../.github/workflows/databases.yml) workflow will:
1. Auto-discover all databases in `src/Databases/`
2. Create naming: `{folder}_{environment}` (e.g., `Main_dev` → `consilient_main_dev`)
3. Execute scripts in order
4. Verify deployment

### Troubleshooting
- **Scripts not running:** Check naming in `src/Databases/` structure
- **"Database not found":** Ensure Terraform created the database first
- **Permission denied:** Check SQL authentication in [TROUBLESHOOTING.md#database-deployment](TROUBLESHOOTING.md#database-deployment)

See [components/databases.md](components/databases.md) for detailed database guide.

---

## 3. Test Workflows Locally with Act (~15 min)

Test GitHub Actions workflows on your machine without consuming GitHub Actions minutes.

### Prerequisites
- Docker Desktop running
- `act` CLI installed:
  - Windows (Chocolatey): `choco install act-cli`
  - Windows (Scoop): `scoop install act`
  - macOS: `brew install act`
  - Linux: Download from [GitHub releases](https://github.com/nektos/act/releases)

### Verify Installation
```powershell
act --version
docker ps  # Should show Docker running
```

### Steps

#### Step 1: Configure Secrets
Navigate to `infra/act/` and configure `.env.act` with your Azure credentials:

```bash
cd infra/act
# Edit .env.act with your values:
# AZURE_CREDENTIALS={"clientId":"...","clientSecret":"...","tenantId":"...","subscriptionId":"..."}
# ARM_CLIENT_ID=your-client-id
# ARM_CLIENT_SECRET=your-client-secret
# etc.
```

See [`infra/act/.env.act`](../../../infra/act/.env.act) template for all required values.

#### Step 2: Run the Script
```powershell
# Interactive mode (recommended for first time)
.\run-act.ps1

# Or parameterized mode
.\run-act.ps1 -Environment dev -SkipTerraform
```

#### Step 3: Follow Prompts
The `run-act.ps1` script will:
- Validate Docker and `act` are installed
- Build custom runner image (first time only, ~10 min)
- Prompt for configuration options
- Run the workflow locally

### Common Usage Patterns

**Test database deployment only:**
```powershell
.\run-act.ps1 -Environment dev -SkipTerraform
```

**Full infrastructure deployment:**
```powershell
.\run-act.ps1 -Environment dev
```

**Debug mode with extra logging:**
```powershell
.\run-act.ps1 -Environment dev -EnableDebugMode -SkipHealthChecks
```

**Non-interactive (for automation):**
```powershell
.\run-act.ps1 -Environment dev -SkipTerraform -SkipDatabases -NonInteractive
```

### Performance Notes
- First run includes Docker image build (~10 minutes)
- Subsequent runs use cached image (~1 minute with `--bind` flag)
- The `--bind` flag enables state persistence (5x faster than without)

See [components/local-testing.md](components/local-testing.md) for comprehensive act guide.

---

## 4. Add a New Azure Resource (~20 min)

Add a new Azure resource (e.g., Storage Account, App Service) to your infrastructure.

### Prerequisites
- Basic understanding of Terraform
- Terraform initialized (`terraform init` completed)

### Step-by-Step

#### Step 1: Define Resource Configuration
Add or update configuration in [`locals.tf`](../../../infra/terraform/locals.tf):

```hcl
# In locals.tf, add to the appropriate environment block:
locals {
  # ... existing config ...

  dev = {
    # Existing settings...
    storage_account_sku = "Standard_LRS"
    storage_replication = "LRS"
  }
}
```

See [`locals.tf:85-120`](../../../infra/terraform/locals.tf#L85-L120) for cost profile structure.

#### Step 2: Create Resource File
Create or update a Terraform file (e.g., `storage.tf`):

```hcl
# infra/terraform/storage.tf
resource "azurerm_storage_account" "app_storage" {
  name                     = "consilient${local.unique_suffix}"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = local.env.storage_replication

  tags = local.common_tags
}

# Output the ID for other resources
output "storage_account_id" {
  value       = azurerm_storage_account.app_storage.id
  description = "ID of the storage account"
}
```

#### Step 3: Validate Configuration
```powershell
terraform validate
```

#### Step 4: Plan & Review
```powershell
terraform plan
```

Review the output to confirm the new resource will be created.

#### Step 5: Apply
```powershell
terraform apply
```

#### Step 6: Update Workflow (if needed)
If the new resource needs to be used in GitHub Actions, update the output in [`outputs.tf`](../../../infra/terraform/outputs.tf):

```hcl
output "storage_account_id" {
  value = azurerm_storage_account.app_storage.id
}
```

Then reference it in workflows as: `${{ needs.terraform.outputs.storage_account_id }}`

### Resource Examples
- **Storage Account:** See [`storage.tf`](../../../infra/terraform/storage.tf)
- **App Service:** See [`api_app.tf`](../../../infra/terraform/api_app.tf)
- **SQL Database:** See [`sql.tf`](../../../infra/terraform/sql.tf)
- **Virtual Network:** See [`network.tf`](../../../infra/terraform/network.tf)

See [components/terraform.md](components/terraform.md#resources-by-category) for all resource types.

---

## 5. Configure GitHub Secrets (~10 min)

Set up authentication secrets required for GitHub Actions workflows.

### Prerequisites
- Azure subscription with contributor access
- GitHub repository access (Settings → Secrets and Variables)

### Required Secrets

See [reference/secrets-checklist.md](reference/secrets-checklist.md) for complete list with acquisition instructions.

**Quick list (8 required + 1 optional):**

| Secret | Required | Source |
|--------|----------|--------|
| AZURE_CLIENT_ID | Yes | Azure Entra ID (OIDC) |
| AZURE_TENANT_ID | Yes | Azure Entra ID (OIDC) |
| AZURE_SUBSCRIPTION_ID | Yes | Azure Subscription |
| ARM_CLIENT_ID | Yes | Service Principal (Terraform) |
| ARM_CLIENT_SECRET | Yes | Service Principal (Terraform) |
| ARM_TENANT_ID | Yes | Service Principal (Terraform) |
| SQL_ADMIN_PASSWORD | Yes | Generate securely |
| AZURE_CREDENTIALS | No | Service Principal JSON (for act) |

### Steps

#### Step 1: Gather Secret Values
Use Azure CLI to obtain values:

```powershell
# Get subscription ID
az account show --query id -o tsv

# Get tenant ID
az account show --query tenantId -o tsv

# Get service principal details (if already created)
az ad sp show --id <client-id>
```

See [reference/secrets-checklist.md](reference/secrets-checklist.md#acquisition-guide) for detailed instructions.

#### Step 2: Configure in GitHub
1. Go to: GitHub → Repository → Settings → Secrets and Variables → Actions
2. Click "New repository secret"
3. Add each secret with its value:

**Example:**
- Name: `AZURE_CLIENT_ID`
- Secret: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`

#### Step 3: Verify Secrets
Secrets are masked in workflow logs. To verify they're set:
1. Push a commit to trigger a workflow
2. Go to Actions → Workflow Run
3. Expand a job step (secrets show as `***`)
4. If a secret is missing, you'll see `null` or empty value errors

### Troubleshooting
- **Secret not found:** Check exact name matches (case-sensitive)
- **Authentication fails:** Verify service principal permissions
- **OIDC errors:** See [components/authentication.md](components/authentication.md#oidc-authentication)

See [components/authentication.md](components/authentication.md) for complete authentication guide.

---

## Checklists

### Initial Setup Checklist
- [ ] Created `terraform.tfvars` from example
- [ ] Ran `terraform init`
- [ ] Ran `terraform plan` and reviewed costs
- [ ] Configured all 8 GitHub secrets
- [ ] Tested database deployment with sample script
- [ ] (Optional) Tested act locally with `run-act.ps1`

### Before Production Deployment
- [ ] All tests passing in GitHub Actions
- [ ] Terraform plan reviewed by team
- [ ] Database backups configured
- [ ] Monitoring and alerting set up
- [ ] Disaster recovery plan documented

### Daily Development
- [ ] Run `terraform plan` before making changes
- [ ] Test database scripts locally if possible
- [ ] Monitor GitHub Actions for failures
- [ ] Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md) if issues arise

---

## Next Steps

1. **Understand the Architecture:** Read [ARCHITECTURE.md](ARCHITECTURE.md)
2. **Deep Dive into Components:** Explore [`components/`](components/) directory
3. **Troubleshoot Issues:** See [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
4. **Optimize Costs:** Review [reference/cost-management.md](reference/cost-management.md)
5. **Secure Setup:** Check [components/authentication.md](components/authentication.md)

---

## Common Issues

| Problem | Solution |
|---------|----------|
| Terraform state lock | See [TROUBLESHOOTING.md#terraform-errors](TROUBLESHOOTING.md#terraform-errors) |
| Database script not running | Check script naming in `src/Databases/` |
| GitHub Actions secrets not working | Verify exact secret names in [reference/secrets-checklist.md](reference/secrets-checklist.md) |
| Docker not running | Start Docker Desktop |
| Act command not found | Run `act --version`, reinstall if needed |

See [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for complete troubleshooting guide.

---

**Last Updated:** December 2025
**For Help:** See [README.md](README.md) for navigation guide
