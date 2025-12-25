# GitHub Actions Authentication Guide

## Overview

This document describes the authentication architecture used across GitHub Actions workflows in the ConsilientWebApp project. The system uses a three-tier authentication strategy to support both cloud deployments and local testing.

## Three-Tier Authentication Architecture

### Tier 1: OIDC Authentication (Production - Recommended)
**Used for:** Cloud-based GitHub Actions execution

**Characteristics:**
- No long-lived secrets exposed in GitHub Actions
- Tokens are short-lived (default 1 hour)
- Requires Azure Entra ID (formerly Azure AD) federation
- Provides better audit trail and compliance

**Secrets Required:**
- `AZURE_CLIENT_ID` - Federated identity app registration ID
- `AZURE_TENANT_ID` - Azure AD tenant ID
- `AZURE_SUBSCRIPTION_ID` - Azure subscription ID

**How It Works:**
1. GitHub Actions generates a OIDC token (JWT) with federation claims
2. The token is exchanged at Azure's STS endpoint for an access token
3. Access token is used to authenticate all Azure operations
4. Token is automatically refreshed as needed

**Benefits:**
- ✅ Zero long-lived secrets
- ✅ Automatic token rotation
- ✅ Audit-friendly with claim-based identity
- ✅ Industry standard (OIDC/JWT)

---

### Tier 2: Service Principal Authentication (Terraform)
**Used for:** Terraform infrastructure management

**Characteristics:**
- Uses traditional service principal with client secret
- Requires separate Azure service principal for infrastructure changes
- Acts as the "infrastructure identity"
- Allows fine-grained RBAC control over resources

**Secrets Required:**
- `ARM_CLIENT_ID` - Service principal app ID
- `ARM_CLIENT_SECRET` - Service principal password
- `ARM_TENANT_ID` - Azure AD tenant ID
- `ARM_SUBSCRIPTION_ID` - Uses same as OIDC (AZURE_SUBSCRIPTION_ID)

**How It Works:**
1. Terraform provider receives credentials via environment variables
2. Provider authenticates as the service principal
3. All infrastructure changes are attributed to this identity
4. RBAC policies control what the service principal can do

**Why Separate from OIDC?**
- Provides security boundary between deployment (OIDC) and infrastructure (service principal)
- Allows restricting infrastructure changes to specific service principal
- Enables audit trail showing which identity made what changes
- Follows principle of least privilege

---

### Tier 3: Service Principal Fallback (Local Testing with `act`)
**Used for:** Local testing with `act` tool

**Characteristics:**
- Only used when running GitHub Actions locally via `act` tool
- Provides fallback authentication when OIDC is not available
- Uses service principal credentials in JSON format
- OPTIONAL - not required for cloud execution

**Secrets Required (Optional):**
- `AZURE_CREDENTIALS` - JSON format: `{"clientId": "...", "clientSecret": "...", "tenantId": "..."}`

**How It Works:**
1. When `act` tool is detected (ACT environment variable set)
2. Azure login falls back to service principal authentication
3. JSON credentials are parsed and used with `az login`
4. If not provided, auth is skipped with informational warning

**When to Use:**
- You want to test GitHub Actions workflows locally
- You're troubleshooting authentication issues
- You want to verify workflow behavior before pushing to GitHub

---

## Secret Configuration Summary

### Cloud Execution (Required - 8 Secrets)

| Category | Secret | Purpose |
|----------|--------|---------|
| **OIDC** | AZURE_CLIENT_ID | Federated identity for token exchange |
| **OIDC** | AZURE_TENANT_ID | Azure AD tenant for federation |
| **OIDC** | AZURE_SUBSCRIPTION_ID | Target Azure subscription |
| **Terraform** | ARM_CLIENT_ID | Service principal for infrastructure |
| **Terraform** | ARM_CLIENT_SECRET | Service principal password |
| **Terraform** | ARM_TENANT_ID | Service principal tenant |
| **SQL Server** | SQL_ADMIN_USERNAME | SQL Server admin username |
| **SQL Server** | SQL_ADMIN_PASSWORD | SQL Server admin password |

### Local Testing (Optional - 1 Secret)

| Category | Secret | Purpose |
|----------|--------|---------|
| **Act Testing** | AZURE_CREDENTIALS | JSON fallback for local testing |

---

## Important Distinction: AZURE_CLIENT_ID vs ARM_CLIENT_ID

These are **intentionally different** service principals:

### AZURE_CLIENT_ID
- **Identity Type:** Federated identity (OIDC)
- **Used By:** GitHub Actions cloud execution, azure-login composite action
- **Credentials:** None (uses OIDC tokens)
- **Scope:** General Azure operations, permissions vary by environment
- **Rotation:** Token-based, no manual rotation needed

### ARM_CLIENT_ID
- **Identity Type:** Service principal (traditional)
- **Used By:** Terraform Azure provider
- **Credentials:** `ARM_CLIENT_SECRET` (password)
- **Scope:** Infrastructure-as-Code (Terraform state, resources)
- **Rotation:** Requires manual secret rotation

### Why Different?

```
┌─────────────────────────────────────────────────────────────┐
│ GitHub Actions Cloud Execution                              │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Azure Login (OIDC)                                   │  │
│  │ - Uses: AZURE_CLIENT_ID + OIDC token                │  │
│  │ - Purpose: General Azure auth                        │  │
│  └──────────────────────────────────────────────────────┘  │
│                         │                                   │
│          ┌──────────────┴──────────────┐                   │
│          │                             │                   │
│          ▼                             ▼                   │
│  ┌──────────────────┐       ┌──────────────────────────┐  │
│  │ az commands      │       │ Terraform               │  │
│  │ (Container ops)  │       │ (Infrastructure)        │  │
│  └──────────────────┘       │                          │  │
│                              │ Uses: ARM_CLIENT_ID +   │  │
│                              │ ARM_CLIENT_SECRET       │  │
│                              └──────────────────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

**Security Benefits:**
- If AZURE_CLIENT_ID is compromised, only general operations are affected
- If ARM_CLIENT_SECRET is compromised, only Terraform operations are affected
- Separate credential rotation schedules
- Audit trail shows which identity made what changes
- Follows principle of least privilege

---

## Workflow-Specific Authentication

### Terraform Workflow (terraform.yml)

**Flow:**
```
1. Validate Inputs
   └─> Validate Required Secrets (NEW - checks all 8 secrets)
2. Azure Login (OIDC via composite action)
   └─> Gets access token for Azure operations
3. Terraform Init
   └─> Uses ARM_CLIENT_ID + ARM_CLIENT_SECRET (via env vars)
4. Terraform Plan/Apply
   └─> Uses service principal credentials
```

**Secrets Used:**
- OIDC: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID
- Terraform: ARM_CLIENT_ID, ARM_CLIENT_SECRET, ARM_TENANT_ID
- SQL: SQL_ADMIN_USERNAME, SQL_ADMIN_PASSWORD
- Act (optional): AZURE_CREDENTIALS

---

### Database Deployment Workflow (databases.yml)

**Flow:**
```
1. Azure Login (OIDC via composite action)
   └─> Authenticates session for sqlcmd operations
2. Apply SQL Scripts
   └─> Uses sqlcmd -G flag (Azure AD auth from session)
3. Verify Database
   └─> Uses same authenticated session
```

**Secrets Used:**
- OIDC: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID
- Database: AZURE_SQL_SERVER (already authenticated via OIDC)
- Act (optional): AZURE_CREDENTIALS

**Why AZURE_CLIENT_SECRET Removed:**
- sqlcmd with `-G` flag uses authenticated session from `az login`
- No need to pass credentials explicitly
- OIDC token provides sufficient permissions for SQL operations
- Reduces secret exposure surface area

---

### App Deployment Workflows (react_apps.yml, dotnet_apps.yml)

**Flow:**
```
1. Azure Login (OIDC via composite action)
   └─> Gets access token for all Azure operations
2. Log into ACR
   └─> Uses az cli with authenticated session
3. Deploy to App Service
   └─> Uses az cli with authenticated session
4. Rollback (if needed)
   └─> Re-authenticates with same credentials
```

**Secrets Used:**
- OIDC: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID
- Act (optional): AZURE_CREDENTIALS

---

## Composite Action: azure-login

The `[.github/actions/azure-login/action.yml](.github/actions/azure-login/action.yml)` composite action provides unified authentication with smart fallback:

### Inputs
```yaml
client-id:          # OIDC federated identity ID (REQUIRED)
tenant-id:          # Azure AD tenant ID (REQUIRED)
subscription-id:    # Azure subscription ID (REQUIRED)
azure-credentials:  # Service principal JSON (OPTIONAL)
```

### Behavior

**In Cloud (GitHub Actions):**
1. Detects OIDC environment
2. Uses `azure/login@v2.3.0` with OIDC
3. Ignores `azure-credentials` input
4. Authenticates with short-lived token

**In Local (act):**
1. Detects `ACT` environment variable
2. Falls back to `azure-credentials` if provided
3. Parses JSON and uses `az login --service-principal`
4. Shows warning if credentials not provided

### Example Usage

```yaml
- name: Login to Azure (OIDC + act fallback)
  uses: ./.github/actions/azure-login
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
    azure-credentials: ${{ secrets.AZURE_CREDENTIALS || '{}' }}
```

---

## Setting Up Authentication

### Step 1: Create OIDC Federated Identity

**PowerShell:**
```powershell
# Variables
$appName = "ConsilientWebApp-GitHub"
$repoOwner = "your-github-org"
$repoName = "ConsilientWebApp"

# Create app registration
$app = New-AzADApplication -DisplayName $appName

# Create federated credential
New-AzADAppFederatedCredentialConfig `
  -Issuer "https://token.actions.githubusercontent.com" `
  -Subject "repo:${repoOwner}/${repoName}:ref:refs/heads/main" `
  -Audience "api://AzureADTokenExchange" `
  -Credential (New-AzADAppFederatedCredentialConfig) | `
New-AzADAppFederatedCredential -ApplicationObjectId $app.Id

# Assign RBAC roles
New-AzRoleAssignment -ObjectId $app.Id -RoleDefinitionName "Contributor" -Scope "/subscriptions/{subscriptionId}"

Write-Output "App ID: $($app.Id)"
```

### Step 2: Create Service Principal for Terraform

**PowerShell:**
```powershell
# Variables
$spName = "ConsilientWebApp-Terraform"
$subscriptionId = "your-subscription-id"

# Create service principal
$sp = New-AzADServicePrincipal -DisplayName $spName

# Create credential
$secret = New-AzADServicePrincipalCredential -ServicePrincipalObject $sp

# Assign RBAC roles
New-AzRoleAssignment -ObjectId $sp.Id -RoleDefinitionName "Contributor" -Scope "/subscriptions/$subscriptionId"

Write-Output "Client ID: $($sp.AppId)"
Write-Output "Secret: $($secret.SecretText)"
```

### Step 3: Configure GitHub Secrets

In GitHub repository settings, add these secrets:

**Organization Level (optional but recommended):**
```
AZURE_CLIENT_ID = [from OIDC federated credential]
AZURE_TENANT_ID = [your Azure AD tenant ID]
AZURE_SUBSCRIPTION_ID = [your Azure subscription ID]
ARM_CLIENT_ID = [from Terraform service principal]
ARM_CLIENT_SECRET = [from Terraform service principal]
ARM_TENANT_ID = [your Azure AD tenant ID]
```

**Environment Level (prod, dev):**
```
SQL_ADMIN_USERNAME = [SQL Server admin username]
SQL_ADMIN_PASSWORD = [SQL Server admin password]
AZURE_SQL_SERVER = [SQL Server FQDN]
```

**Repository Level (optional - act testing):**
```
AZURE_CREDENTIALS = {
  "clientId": "...",
  "clientSecret": "...",
  "tenantId": "..."
}
```

---

## Troubleshooting Authentication Issues

### Issue: "AZURE_CREDENTIALS is not valid JSON"

**Cause:** The JSON is malformed or not properly escaped

**Solution:**
1. Verify JSON is valid: `echo $AZURE_CREDENTIALS | jq empty`
2. Ensure proper escaping in secret value
3. Check that all required fields are present: `clientId`, `clientSecret`, `tenantId`

---

### Issue: "Missing required secrets" (Validate Required Secrets fails)

**Cause:** One or more required secrets is not configured

**Solution:**
1. Check which secret is missing from error message
2. Go to GitHub repository > Settings > Secrets and variables
3. Add the missing secret for the appropriate environment
4. Verify secret value is not empty

---

### Issue: "OIDC Token Exchange Failed"

**Cause:** The federated credential is not properly configured or the GitHub token doesn't match the condition

**Solution:**
1. Verify federated credential is configured correctly:
   - Issuer: `https://token.actions.githubusercontent.com`
   - Audience: `api://AzureADTokenExchange`
   - Subject matches your repository and branch
2. Check that workflow is running on the correct repository/branch
3. Verify the app registration has the correct subscription scope

---

### Issue: "Unauthorized: The user or service principal does not have permission"

**Cause:** The OIDC identity or service principal doesn't have sufficient Azure RBAC roles

**Solution:**
1. Verify OIDC identity has "Contributor" role on subscription
2. Verify service principal has "Contributor" role on subscription
3. Check if resource-specific RBAC is needed (e.g., Container Registry push)
4. Use Azure Portal to assign missing roles

---

### Issue: "sqlcmd: Error: Unable to authenticate"

**Cause:** Azure SQL Server authentication failed

**Solution:**
1. Verify OIDC authentication completed successfully
2. Check SQL Server has "Azure AD admin" configured
3. Verify the identity (OIDC app) has permissions on the database
4. Try manually: `az login` then `sqlcmd -S server.database.windows.net -d dbname -G`

---

### Issue: Running locally with `act` and "AZURE_CREDENTIALS not provided"

**Cause:** Trying to run `act` without setting AZURE_CREDENTIALS secret

**Solution:**
1. If you don't need Azure auth locally, it's fine to skip
2. If you need it, set AZURE_CREDENTIALS in `.env` file:
   ```
   AZURE_CREDENTIALS={"clientId":"...","clientSecret":"...","tenantId":"..."}
   ```
3. Or use `act` command: `act -s AZURE_CREDENTIALS='{"clientId":"...","clientSecret":"...","tenantId":"..."}' `

---

## Security Best Practices

### ✅ DO:
- ✅ Use OIDC for cloud execution whenever possible
- ✅ Rotate `ARM_CLIENT_SECRET` regularly (quarterly at minimum)
- ✅ Use separate service principals for different environments (dev, prod)
- ✅ Monitor secret access in Azure Activity Log
- ✅ Use managed identities for Azure resources (not applicable to GitHub Actions)
- ✅ Review and minimize RBAC role scope
- ✅ Use environment-specific secrets for production

### ❌ DON'T:
- ❌ Use long-lived credentials in GitHub Actions (use OIDC instead)
- ❌ Share service principal credentials between projects
- ❌ Store credentials in code or commit messages
- ❌ Use overly permissive RBAC roles (Contributor everywhere)
- ❌ Commit `.tfvars` files with secrets
- ❌ Expose secrets in GitHub Action logs (always use GitHub's secret masking)
- ❌ Store multiple projects' secrets in one shared location

---

## Migration from Basic Auth to OIDC

If you're currently using basic authentication or certificate-based auth:

### Phase 1: Set Up OIDC (Non-Breaking)
1. Create OIDC federated credential (as described above)
2. Add new secrets: AZURE_CLIENT_ID, AZURE_TENANT_ID
3. Don't remove old secrets yet

### Phase 2: Update Workflows
1. Update `azure-login` calls to use new composite action
2. Test in dev environment first
3. Verify OIDC authentication works

### Phase 3: Clean Up
1. Once verified, remove old secrets
2. Remove old authentication steps
3. Document the change

---

## See Also

- [Azure OIDC Documentation](https://docs.microsoft.com/en-us/azure/active-directory/workload-identities/workload-identity-federation)
- [GitHub OIDC Documentation](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/about-security-hardening-with-openid-connect)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/guides/service_principal_client_secret)
- [Azure CLI Authentication](https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli)

---

**Last Updated:** December 2025
**Author:** GitHub Actions Architecture Review
**Version:** 2.0 (Authentication Optimization Phase 2)
