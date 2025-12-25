# GitHub Secrets Configuration Reference

Quick reference for all GitHub secrets required by ConsilientWebApp CI/CD pipelines.

## Secret Configuration Checklist

### ✅ Required Secrets (Must Configure - 8 Total)

#### OIDC Authentication (Cloud Execution)
- [ ] `AZURE_CLIENT_ID` - Federated identity app ID
- [ ] `AZURE_TENANT_ID` - Azure AD tenant ID
- [ ] `AZURE_SUBSCRIPTION_ID` - Target Azure subscription

#### Terraform Infrastructure
- [ ] `ARM_CLIENT_ID` - Service principal app ID
- [ ] `ARM_CLIENT_SECRET` - Service principal password
- [ ] `ARM_TENANT_ID` - Service principal tenant ID

#### SQL Server Operations
- [ ] `SQL_ADMIN_USERNAME` - SQL Server admin username
- [ ] `SQL_ADMIN_PASSWORD` - SQL Server admin password

### ⭕ Optional Secrets (For Local Testing Only)

- [ ] `AZURE_CREDENTIALS` - JSON format for `act` tool (only if using local testing)

---

## Secret Acquisition Guide

### Find Your Azure IDs

**Tenant ID:**
```bash
az account show --query tenantId -o tsv
```

**Subscription ID:**
```bash
az account show --query id -o tsv
```

---

### Create OIDC Federated Identity

**PowerShell - Create App Registration:**
```powershell
# Register new app
$app = New-AzADApplication -DisplayName "ConsilientWebApp-GitHub"
$appId = $app.Id

Write-Output "Application ID: $appId"
```

**PowerShell - Create Federated Credential:**
```powershell
$appId = "your-app-id"
$repoOwner = "your-github-org"
$repoName = "ConsilientWebApp"

New-AzADAppFederatedCredential `
  -ApplicationObjectId $appId `
  -Issuer "https://token.actions.githubusercontent.com" `
  -Subject "repo:${repoOwner}/${repoName}:ref:refs/heads/main" `
  -Audience "api://AzureADTokenExchange"
```

**Result:** Use the app ID as `AZURE_CLIENT_ID`

---

### Create Terraform Service Principal

**PowerShell:**
```powershell
# Create service principal
$sp = New-AzADServicePrincipal -DisplayName "ConsilientWebApp-Terraform"
$clientId = $sp.AppId

# Create password credential
$secret = New-AzADServicePrincipalCredential -ServicePrincipalObject $sp

Write-Output "Client ID: $clientId"
Write-Output "Secret: $($secret.SecretText)"
Write-Output "Save these immediately - secret cannot be retrieved later"
```

**Assign Permissions:**
```powershell
$subscriptionId = "your-subscription-id"
New-AzRoleAssignment `
  -ObjectId $sp.Id `
  -RoleDefinitionName "Contributor" `
  -Scope "/subscriptions/$subscriptionId"
```

**Result:**
- Use `$clientId` as `ARM_CLIENT_ID`
- Use `$secret` as `ARM_CLIENT_SECRET`

---

### Create SQL Server Admin Account

**In Azure Portal:**
1. Navigate to SQL Server
2. Click "SQL and ADLS gen2 admin"
3. Set Azure AD admin (recommended)
4. Or create SQL user with strong password

---

## GitHub Configuration

### 1. Go to Repository Settings

**Path:** GitHub Repo > Settings > Secrets and variables

### 2. Add Organization Secrets (Recommended)

For shared secrets across multiple repositories:

**Settings > Organization > Secrets**

```
Name: AZURE_CLIENT_ID
Value: [your-app-id-from-federated-credential]

Name: AZURE_TENANT_ID
Value: [your-azure-tenant-id]

Name: AZURE_SUBSCRIPTION_ID
Value: [your-azure-subscription-id]

Name: ARM_CLIENT_ID
Value: [your-service-principal-app-id]

Name: ARM_CLIENT_SECRET
Value: [your-service-principal-secret]

Name: ARM_TENANT_ID
Value: [your-azure-tenant-id]
```

### 3. Add Environment Secrets (Per Environment)

**Settings > Environments > dev > Secrets**

```
Name: SQL_ADMIN_USERNAME
Value: sqladmin

Name: SQL_ADMIN_PASSWORD
Value: [strong-password]

Name: AZURE_SQL_SERVER
Value: consilient-sql-dev.database.windows.net
```

**Settings > Environments > prod > Secrets**

```
Name: SQL_ADMIN_USERNAME
Value: sqladmin

Name: SQL_ADMIN_PASSWORD
Value: [strong-password-different-from-dev]

Name: AZURE_SQL_SERVER
Value: consilient-sql-prod.database.windows.net
```

### 4. Add Repository Secrets (Optional - For act Testing Only)

**Settings > Secrets and variables > Repository secrets**

```
Name: AZURE_CREDENTIALS
Value: {"clientId":"...","clientSecret":"...","tenantId":"..."}
```

---

## Secret Lifecycle Management

### Access Control

| Secret | Who Creates | Who Can Use | Rotation Frequency |
|--------|------------|-----------|-------------------|
| AZURE_CLIENT_ID | DevOps/Azure Admin | All workflows | Never* |
| AZURE_TENANT_ID | DevOps/Azure Admin | All workflows | Never* |
| AZURE_SUBSCRIPTION_ID | DevOps/Azure Admin | All workflows | Never* |
| ARM_CLIENT_ID | DevOps/Azure Admin | Terraform | Never* |
| **ARM_CLIENT_SECRET** | DevOps/Azure Admin | Terraform only | **Quarterly** |
| ARM_TENANT_ID | DevOps/Azure Admin | Terraform | Never* |
| SQL_ADMIN_USERNAME | SQL/Database Admin | Database workflows | Annually |
| **SQL_ADMIN_PASSWORD** | SQL/Database Admin | Database workflows | **Quarterly** |
| AZURE_CREDENTIALS | DevOps/Azure Admin | Local testing (act) | As needed |

*Never = Not tied to time, only rotated if compromise suspected

---

## Monitoring Secret Usage

### Azure Activity Log

**Check who/what used service principals:**

```bash
# View service principal usage
az monitor activity-log list \
  --caller "[service-principal-object-id]" \
  --output table

# View in Azure Portal:
# Monitor > Activity Log > Filter by Caller = [your service principal]
```

### GitHub Audit Log

**Check secret access in your organization:**

Path: Organization > Settings > Audit log

Search for:
- `secret.access` - Secret was accessed
- `secrets_removed` - Secret was deleted
- `token.accessed` - Token was used

---

## Validation Checklist

Before deploying, verify:

- [ ] All 8 required secrets configured
- [ ] No secrets committed to code repository
- [ ] Secrets are not exposed in GitHub Action logs
- [ ] OIDC federated credential matches repository/branch
- [ ] Service principal has Contributor RBAC role
- [ ] SQL Server has Azure AD admin configured
- [ ] Secrets are not shared with untrusted repositories
- [ ] Secrets are unique per environment (dev vs prod)

---

## Troubleshooting Secret Issues

### Issue: Secret Not Found Error

```
Error: Secret [NAME] is not defined
```

**Causes & Solutions:**
- [ ] Secret not added to repository: Add in Settings > Secrets
- [ ] Wrong organization: Check organization vs repository secrets
- [ ] Wrong environment: Check environment name matches workflow
- [ ] Typo in secret reference: Verify `${{ secrets.NAME }}` spelling

---

### Issue: Secret is Null or Empty

```
[Secret] is null or empty
```

**Causes:**
- Secret was deleted
- Secret value is only whitespace
- Secret not inherited from organization

**Solution:**
1. Verify secret exists in GitHub settings
2. Verify secret value is not empty
3. Check organization-level secret is being used

---

### Issue: OIDC Authentication Failing

```
Error: OIDC token exchange failed
```

**Causes & Solutions:**
- [ ] Federated credential not configured
- [ ] Federated credential subject doesn't match branch/repo
- [ ] App registration doesn't exist
- [ ] Issuer is wrong (`https://token.actions.githubusercontent.com`)

**Fix:**
1. Verify federated credential exists in Azure AD
2. Check subject pattern: `repo:[owner]/[repo]:ref:refs/heads/[branch]`
3. Verify app registration has OIDC enabled

---

### Issue: Service Principal Authorization Failed

```
Error: The user, group or application does not have the
sufficient permissions to perform the requested operation
```

**Causes:**
- RBAC role not assigned
- RBAC role scope is too narrow
- Wrong service principal used

**Solution:**
1. Verify service principal has "Contributor" role
2. Check RBAC role scope includes your subscription
3. Verify using correct `ARM_CLIENT_ID`

---

## Quick Start for New Team Members

1. **Get the secret names and descriptions from this doc**
2. **Ask DevOps for secret values** (security best practice - don't store in shared docs)
3. **Add secrets to GitHub Settings** (follow sections above)
4. **Run a test workflow** to verify everything works
5. **Report any errors** to DevOps team

---

## Security Reminders

🔒 **GOLDEN RULES:**
- ⛔ Never commit secrets to Git
- ⛔ Never hardcode secrets in workflows
- ⛔ Never share secret values via email/chat
- ⛔ Never use one secret across multiple purposes
- ⛔ Never ignore secret rotation notifications

✅ **DO THIS INSTEAD:**
- Store secrets only in GitHub Secrets
- Use GitHub's secret masking automatically
- Rotate `ARM_CLIENT_SECRET` quarterly
- Use environment-specific secrets
- Monitor access in Activity Logs

---

**Last Updated:** December 2025
**Version:** 2.0
