# Secrets & Variables Reference

<!-- AI_CONTEXT: Complete reference for all secrets and variables needed to run infrastructure. GitHub secrets/variables, Terraform variables, Azure Key Vault secrets, and local testing configuration. -->

## For Non-Technical Stakeholders

Secrets are sensitive values like passwords and API keys that must be kept confidential. Variables are non-sensitive configuration values. This document lists all secrets and variables needed to run the infrastructure, where to get them, and how to configure them securely.

---

## Quick Reference Tables

### GitHub Secrets (14 Total)

<!-- AI_TABLE: Required GitHub repository secrets for authentication and infrastructure -->

| Secret Name | Type | Required | Purpose | How to Get |
|-------------|------|----------|---------|------------|
| AZURE_CLIENT_ID | UUID | Yes | OIDC authentication | `az ad app list --display-name consilient-app` |
| AZURE_TENANT_ID | UUID | Yes | Azure AD tenant | `az account show --query tenantId -o tsv` |
| AZURE_SUBSCRIPTION_ID | UUID | Yes | Azure subscription | `az account show --query id -o tsv` |
| ARM_CLIENT_ID | UUID | Yes | Terraform provider | Service Principal app ID |
| ARM_CLIENT_SECRET | String | Yes | Terraform authentication | Service Principal password |
| ARM_TENANT_ID | UUID | Yes | Terraform tenant | Same as AZURE_TENANT_ID |
| SQL_ADMIN_USERNAME | String | Yes | SQL Server admin | User-defined (e.g., "sqladmin") |
| SQL_ADMIN_PASSWORD | String | Yes | SQL Server password | Generate: `openssl rand -base64 32` |
| JWT_SIGNING_SECRET | Base64 | Yes | JWT token signing | Generate: `openssl rand -base64 64` |
| OAUTH_CLIENT_SECRET | String | No | OAuth (optional) | OAuth provider dashboard |
| AZURE_CREDENTIALS | JSON | No | Local act testing | Service Principal JSON |
| AZURE_API_WEBAPP_NAME | String | No | Legacy fallback | Not recommended |
| AZURE_WEBAPP_NAME | String | No | Legacy fallback | Not recommended |
| GITHUB_TOKEN | Auto | No | Auto-provided | GitHub provides automatically |

<!-- AI_WARNING: Never commit secrets to source control. Always use GitHub Secrets UI to configure. -->

### GitHub Variables (13 Total)

<!-- AI_TABLE: Non-sensitive configuration values used by workflows -->

| Variable | Value | Used By | Purpose |
|----------|-------|---------|---------|
| CONTAINER_REGISTRY | ghcr.io | All workflows | Container registry URL |
| ACR_REGISTRY_URL | {acr}.azurecr.io | Docker workflows | Azure Container Registry |
| API_IMAGE_NAME | consilient-api | API workflow | Docker image name |
| REACT_IMAGE_NAME | consilient-react | React workflow | Docker image name |
| ACTIONS_RUNNER_IMAGE | consilient-runner:latest | All workflows | Custom runner image |
| AZURE_SQL_SERVER_FQDN | {server}.database.windows.net | Database workflows | SQL Server endpoint |
| SCHEMASPY_VERSION | 6.2.4 | DB docs workflow | SchemaSpy version |
| JDBC_DRIVER_VERSION | 12.4.2.jre11 | DB docs workflow | JDBC driver version |
| AZURE_REGION | canadacentral | All workflows | Azure region |
| AZURE_RESOURCE_GROUP_NAME | consilient-resource-group | All workflows | Resource group name |
| TERRAFORM_VERSION | 1.6.0 | Terraform workflow | Terraform version |
| TF_STATE_STORAGE_ACCOUNT | consilienttfstate{env}{hash} | Terraform workflow | Terraform state storage |
| TF_STATE_CONTAINER | tfstate | Terraform workflow | State container name |

### Terraform Variables (22 Total)

<!-- AI_TABLE: Variables defined in infra/terraform/variables.tf -->

| Variable | Type | Default | Required | Description |
|----------|------|---------|----------|-------------|
| project_name | string | "consilient" | No | Project name prefix |
| environment | string | - | **Yes** | "dev" or "prod" only (validated) |
| region | string | - | **Yes** | Azure region (e.g., canadacentral) |
| resource_group_name | string | - | **Yes** | Resource group name |
| subscription_id | string | - | **Yes** | Azure subscription ID |
| sql_admin_username | string | - | **Yes** | SQL Server admin username |
| sql_admin_password | string (sensitive) | - | **Yes** | SQL Server admin password |
| jwt_signing_secret | string (sensitive) | - | **Yes** | JWT signing key |
| oauth_client_secret | string (sensitive) | "" | No | OAuth client secret |
| api_custom_domain | string | "" | No | API custom domain name |
| react_custom_domain | string | "" | No | React custom domain name |
| loki_retention | string | "30d" | No | Log retention period |
| loki_cpu_request | number | 0.5 | No | Loki CPU request |
| loki_cpu_limit | number | 1.0 | No | Loki CPU limit |
| loki_memory_request | string | "1.0Gi" | No | Loki memory request |
| loki_memory_limit | string | "2.0Gi" | No | Loki memory limit |
| grafana_major_version | number | 11 | No | Grafana major version |
| create_container_app_environment | bool | true | No | Create new CAE vs use existing |
| existing_container_app_environment_id | string | "" | No | Existing CAE ID if using existing |
| container_app_environment_name_template | string | "consilient-cae-{environment}" | No | CAE naming template |
| enable_local_firewall | bool | false | No | SQL firewall for act (dev only) |
| hostname_naming_tier | number | 0 | No | Hostname tier (0/1/2) |

<!-- AI_NOTE: environment variable only accepts "dev" or "prod" per validation in variables.tf:27-28. Code uses local.tf lines 285-288 for cost estimates. -->

### Azure Key Vault Secrets (5 Runtime Secrets)

<!-- AI_TABLE: Secrets stored in Azure Key Vault for application runtime -->

| Secret Name | Purpose | Source | Access Method |
|-------------|---------|--------|---------------|
| sql-connection-string-main | Main DB connection | Terraform from SQL credentials | Managed Identity |
| sql-connection-string-hangfire | Hangfire DB connection | Terraform from SQL credentials | Managed Identity |
| jwt-signing-secret | JWT token signing | Terraform from JWT_SIGNING_SECRET | Managed Identity |
| grafana-loki-url | Loki endpoint URL | Terraform from Loki resource | Managed Identity |
| oauth-client-secret | OAuth secret (optional) | Terraform from OAUTH_CLIENT_SECRET | Managed Identity |

---

## Setup Checklists

### Initial GitHub Setup

<!-- AI_CONTEXT: First-time repository configuration for new environment -->

**OIDC Service Principal:**
- [ ] Create Entra ID app registration for GitHub
- [ ] Add federated credential for GitHub repository
- [ ] Note AZURE_CLIENT_ID for GitHub secrets
- [ ] Note AZURE_TENANT_ID for GitHub secrets

**Terraform Service Principal:**
- [ ] Run `az ad sp create-for-rbac --name consilient-terraform --role Contributor`
- [ ] Note ARM_CLIENT_ID (app ID)
- [ ] Note ARM_CLIENT_SECRET (password)
- [ ] Note ARM_TENANT_ID (tenant)

**Generate Secrets:**
- [ ] SQL password: `openssl rand -base64 32`
- [ ] JWT secret: `openssl rand -base64 64`
- [ ] Verify passwords meet Azure requirements

**Add GitHub Secrets:**
- [ ] AZURE_CLIENT_ID
- [ ] AZURE_TENANT_ID
- [ ] AZURE_SUBSCRIPTION_ID
- [ ] ARM_CLIENT_ID
- [ ] ARM_CLIENT_SECRET
- [ ] ARM_TENANT_ID
- [ ] SQL_ADMIN_USERNAME
- [ ] SQL_ADMIN_PASSWORD
- [ ] JWT_SIGNING_SECRET

**Add GitHub Variables:**
- [ ] AZURE_REGION = canadacentral
- [ ] AZURE_RESOURCE_GROUP_NAME = consilient-rg-{env}
- [ ] TERRAFORM_VERSION = 1.6.0
- [ ] All other variables from table above

**Verify Secrets:**
- [ ] Secrets are masked in workflow logs (shown as ***)
- [ ] Test workflow run succeeds
- [ ] No "secret not found" errors in logs

### Local Act Testing Setup (Optional)

<!-- AI_CONTEXT: Configuration for local GitHub Actions testing -->

- [ ] Install act CLI: `choco install act-cli` or `brew install act`
- [ ] Install Docker Desktop
- [ ] Navigate to `infra/act/`
- [ ] Copy `.env.act.template` to `.env.act` (gitignored)
- [ ] Add AZURE_CREDENTIALS JSON to .env.act
- [ ] Add all ARM_* secrets to .env.act
- [ ] Test with `.\run-act.ps1 -Environment dev -SkipTerraform`
- [ ] Verify act can access Docker

---

## How to Set Up New Environment

### Step 1: Create Service Principals

```bash
# OIDC Service Principal (GitHub to Azure)
az ad app create --display-name "consilient-github"
az ad sp create --id {app-id}

# Add federated credential
az ad app federated-credential create --id {app-id} --parameters @- <<EOF
{
  "name": "github-main",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:YOUR-ORG/consilient-webapp:ref:refs/heads/main",
  "audiences": ["api://AzureADTokenExchange"]
}
EOF

# Terraform Service Principal
az ad sp create-for-rbac --name "consilient-terraform" \
  --role Contributor \
  --scopes /subscriptions/{subscription-id}
```

### Step 2: Add GitHub Secrets

1. Go to: GitHub → Repository → Settings → Secrets and Variables → Actions
2. Click "New repository secret"
3. Add all 9 required secrets from table above

### Step 3: Add GitHub Variables

1. Go to: GitHub → Repository → Settings → Secrets and Variables → Actions → Variables
2. Click "New repository variable"
3. Add all 13 variables from table above

### Step 4: Configure Terraform

```bash
cd infra/terraform
cp terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars`:
```hcl
environment           = "dev"
region                = "canadacentral"
subscription_id       = "your-subscription-id"
resource_group_name   = "consilient-rg-dev"
sql_admin_username    = "sqladmin"
sql_admin_password    = "YourSecurePassword123!"
jwt_signing_secret    = "your-jwt-secret"
```

### Step 5: Initialize and Deploy

```bash
cd infra/terraform
terraform init
terraform plan -out=tfplan
terraform apply tfplan
```

---

## Security Best Practices

### Secret Management

1. **Never commit secrets** - Use `.gitignore` to exclude `terraform.tfvars` and `.env.act`
2. **Use GitHub Secrets UI** - Never paste secrets in workflows or commits
3. **Rotate secrets regularly** - Set quarterly rotation reminders
4. **Audit access** - Check Azure Activity Log for Key Vault access
5. **Use Managed Identity** - Applications access secrets via Azure AD, not secrets in code

### Terraform Variables

1. **Sensitive flag** - Mark sensitive variables with `sensitive = true`
2. **Environment variables** - Use TF_VAR_* for GitHub Actions injection
3. **State file protection** - Remote state in Azure Storage with versioning
4. **No defaults** - Passwords/secrets have no defaults, must be provided

### Local Testing

1. **Separate .env.act file** - Never use production secrets locally
2. **Service Principal only** - Act doesn't support OIDC, use fallback SP
3. **Limited lifetime** - Rotate act credentials more frequently than GitHub
4. **Docker security** - Act runs in Docker, ensure Docker is trusted

---

## Common Issues & Solutions

### "Secret not found" Error

**Problem:** Workflow fails with secret not found

**Solution:**
1. Check secret name matches exactly (case-sensitive)
2. Verify secret is in repository (not organization)
3. Check environment-specific secrets if applicable
4. Confirm GitHub Actions permission to read secrets

### OIDC Authentication Fails

**Problem:** "Error: Invalid credentials" from Azure login

**Solution:**
1. Verify AZURE_CLIENT_ID is correct
2. Confirm federated credential includes correct repo/branch
3. Check token expiration (OIDC tokens expire in 1 hour)
4. Verify service principal has required RBAC roles

### "Access Denied" to Key Vault

**Problem:** Application can't read secrets from Key Vault

**Solution:**
1. Verify Managed Identity is enabled on App Service
2. Check RBAC role on Key Vault (should have "Key Vault Secrets User")
3. Confirm App Configuration has access to Key Vault
4. Check network connectivity (private endpoints)

### Local Act Testing Fails

**Problem:** `.env.act` secrets not being loaded

**Solution:**
1. Verify `.env.act` exists and is gitignored
2. Check file is in `infra/act/` directory
3. Ensure format matches template (KEY=VALUE per line)
4. Run `.\run-act.ps1` from `infra/act/` directory

---

## Related Documentation

- [components/terraform.md](../components/terraform.md) - Terraform infrastructure guide
- [components/authentication.md](../components/authentication.md) - Authentication details and rotation
- [components/local-testing.md](../components/local-testing.md) - Local testing with act
- [reference/naming-conventions.md](naming-conventions.md) - Resource naming patterns
- [QUICK_START.md](../QUICK_START.md#6-configure-github-secrets) - Quick setup guide

---

**Last Updated:** January 2026
**Merged From:** secrets-variables-reference.md, secrets-checklist.md
**Note:** This is the consolidated version. Archive files secrets-reference.md and secrets-checklist.md can be deleted.
