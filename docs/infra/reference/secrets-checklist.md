# Secrets Configuration Checklist

Quick reference for all GitHub Actions secrets required for cloud execution.

## Required Secrets (8 Total)

All must be configured in GitHub: Settings → Secrets and Variables → Actions

### OIDC Secrets (Cloud Authentication)

| Secret | Value Format | Source | Used By |
|--------|-----------|--------|---------|
| `AZURE_CLIENT_ID` | UUID `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` | Azure Entra ID App ID | `azure-login` action |
| `AZURE_TENANT_ID` | UUID format | Azure AD Tenant ID | `azure-login` action |
| `AZURE_SUBSCRIPTION_ID` | UUID format | Azure Subscription ID | Terraform, Azure CLI |

**Get Values:**
```powershell
# Client ID (app registration ID)
az ad app list --display-name "consilient-app" --query '[0].appId' -o tsv

# Tenant ID
az account show --query tenantId -o tsv

# Subscription ID
az account show --query id -o tsv
```

### Terraform Secrets (Infrastructure)

| Secret | Value Format | Source | Used By |
|--------|-----------|--------|---------|
| `ARM_CLIENT_ID` | UUID format | Service Principal App ID | Terraform provider |
| `ARM_CLIENT_SECRET` | Random string (password) | Service Principal Secret | Terraform provider |
| `ARM_TENANT_ID` | UUID format | Service Principal Tenant | Terraform provider |

**Get Values:**
```powershell
# Create service principal
az ad sp create-for-rbac --name "consilient-terraform" \
  --role Contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>

# Output includes: appId (ARM_CLIENT_ID), password (ARM_CLIENT_SECRET), tenant (ARM_TENANT_ID)
```

### SQL Secrets

| Secret | Value Format | Source | Used By |
|--------|-----------|--------|---------|
| `SQL_ADMIN_PASSWORD` | Strong password | Generate securely | SQL Server deployment |
| `SQL_ADMIN_USERNAME` | Username | Usually: `dbadmin` | SQL Server deployment |

**Generation:**
```powershell
# Generate strong password (16+ chars, uppercase, lowercase, numbers, special)
# Example: P@ssw0rd!SecureT3st

# Username: typically "dbadmin" or "sqladmin"
```

## Optional Secret (Local Testing Only)

| Secret | Value Format | Source | Used By |
|--------|-----------|--------|---------|
| `AZURE_CREDENTIALS` | JSON (service principal) | Service Principal | `act` fallback auth |

**Format:**
```json
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "your-secret-here",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}
```

**Only needed for:** Local testing with `act` tool

## Setup Steps

### Step 1: Gather All Values

```powershell
# Show all values you'll need
az account show

# For OIDC (federated credentials already configured)
az ad app list --display-name "consilient-app"

# For service principal (if needed)
az ad sp list --display-name "consilient-terraform"
```

### Step 2: Create Service Principals (if not exists)

**OIDC Application:**
```powershell
az ad app create --display-name "consilient-app"
az ad sp create --id <APP_ID>
```

**Terraform Service Principal:**
```powershell
az ad sp create-for-rbac --name "consilient-terraform" \
  --role Contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>
```

**Local Testing Service Principal (optional):**
```powershell
az ad sp create-for-rbac --name "consilient-act-local"
```

### Step 3: Add to GitHub

1. Go to: GitHub Repo → Settings → Secrets and Variables → Actions
2. Click "New repository secret"
3. Add each secret:

**Example:**
- Name: `AZURE_CLIENT_ID`
- Secret: `12345678-1234-1234-1234-123456789012`
- Click "Add secret"

Repeat for all 8 required secrets.

### Step 4: Verify Configuration

Push a test commit to trigger workflow:

```powershell
git commit --allow-empty -m "Test secret configuration"
git push
```

Go to Actions tab to verify workflow runs:
- Look for step that uses secrets
- Should see `***` (masked) instead of actual values
- If errors, check secret names (case-sensitive)

## Verification Checklist

- [ ] AZURE_CLIENT_ID configured
- [ ] AZURE_TENANT_ID configured
- [ ] AZURE_SUBSCRIPTION_ID configured
- [ ] ARM_CLIENT_ID configured
- [ ] ARM_CLIENT_SECRET configured
- [ ] ARM_TENANT_ID configured
- [ ] SQL_ADMIN_PASSWORD configured
- [ ] SQL_ADMIN_USERNAME configured (optional if using Azure AD)
- [ ] AZURE_CREDENTIALS configured (optional, only for act)
- [ ] Workflow triggered and ran successfully
- [ ] No "secret ... not found" errors

## Security Notes

1. **Never Commit Secrets**
   - `.gitignore` should exclude `.env.act`, `.secrets`
   - Secrets stored in GitHub, not code

2. **Secret Rotation**
   - Rotate secrets quarterly (recommended)
   - Use Azure Key Vault for production secrets

3. **Access Control**
   - Limit who can view/edit secrets (GitHub team settings)
   - Audit secret access in Azure Activity Log

4. **Compromise Response**
   - If secret leaked: immediately rotate it in Azure
   - Create new secrets in GitHub
   - Revoke old credentials
   - Audit access logs

## Troubleshooting

**Secret not found in workflow:**
- Check exact spelling (case-sensitive)
- Verify secret exists in GitHub UI
- Ensure repository-level secret (not organization-level)

**Workflow fails with auth error:**
- Verify secret values are correct
- Check service principal has Contributor role
- Wait 1-2 minutes for RBAC propagation

**Local act testing fails:**
- Only AZURE_CREDENTIALS optional (not required for cloud)
- Verify `.env.act` file has correct JSON format

See [components/authentication.md](../components/authentication.md) for detailed auth guide.

## Related Documentation

- [components/authentication.md](../components/authentication.md) - Complete auth guide
- [reference/naming-conventions.md](naming-conventions.md) - Resource patterns
- [QUICK_START.md#5-configure-github-secrets](../QUICK_START.md#5-configure-github-secrets) - Step-by-step guide
- [TROUBLESHOOTING.md#secret-validation-errors](../TROUBLESHOOTING.md#secret-validation-errors) - Troubleshooting

---

**Last Updated:** December 2025
**For Navigation:** See [README.md](../README.md)
