# Authentication & Secrets

Complete guide to authentication architecture and secret configuration.

## Overview

The Consilient infrastructure uses a **three-tier authentication strategy** to support both cloud deployments (GitHub Actions) and local testing (act tool):

1. **Tier 1: OIDC** - Cloud execution (no long-lived secrets)
2. **Tier 2: Service Principal** - Terraform provider authentication
3. **Tier 3: Fallback** - Local testing with act tool

This architecture provides security (OIDC in cloud), flexibility (fallback for local), and clear separation of concerns.

## Three-Tier Architecture

### Tier 1: OIDC Authentication (Cloud)

**Used for:** GitHub Actions cloud execution

**Characteristics:**
- No long-lived secrets in GitHub
- Short-lived tokens (auto-rotated)
- Requires Azure Entra ID federation
- Best audit trail and compliance

**Secrets Required:**
- `AZURE_CLIENT_ID` - Federated identity app ID
- `AZURE_TENANT_ID` - Azure AD tenant ID
- `AZURE_SUBSCRIPTION_ID` - Azure subscription ID

**How It Works:**
1. GitHub Actions generates OIDC token (JWT)
2. Token exchanged for Azure access token
3. Access token used for Azure operations
4. Token auto-refreshes

**Security:** ✅ Zero long-lived secrets, auto-rotation, claim-based audit trail

---

### Tier 2: Service Principal (Terraform)

**Used for:** Terraform infrastructure management

**Characteristics:**
- Traditional service principal with client secret
- Separate identity for infrastructure changes
- Fine-grained RBAC control
- Clear separation from deployment identity

**Secrets Required:**
- `ARM_CLIENT_ID` - Service principal app ID
- `ARM_CLIENT_SECRET` - Service principal password
- `ARM_TENANT_ID` - Azure tenant ID

**Why Different from OIDC?**

The terraform provider cannot use OIDC directly (Terraform limitation). Instead:
- OIDC (`AZURE_CLIENT_ID`) → General Azure operations, Azure CLI commands
- Service Principal (`ARM_CLIENT_ID`) → Terraform provider authentication

This separation provides security benefits:
- If one is compromised, the other is unaffected
- Separate rotation schedules
- Audit trail shows which identity did what
- Least privilege principle

**Environment Variables:**
```bash
ARM_CLIENT_ID=<service-principal-app-id>
ARM_CLIENT_SECRET=<service-principal-password>
ARM_TENANT_ID=<tenant-id>
```

---

### Tier 3: Fallback (Local Testing with act)

**Used for:** Local workflow testing via `act` tool

**Characteristics:**
- Only active when `ACT` environment variable set
- Fallback when OIDC unavailable
- Service principal in JSON format
- OPTIONAL (not required for cloud)

**Secrets Required (Optional):**
- `AZURE_CREDENTIALS` - JSON: `{"clientId":"...","clientSecret":"...","tenantId":"..."}`

**When to Use:**
- Testing GitHub Actions locally before push
- Troubleshooting workflow behavior
- Validating changes in safe environment

**Format:**
```json
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "your-service-principal-secret",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}
```

---

## AZURE_CLIENT_ID vs ARM_CLIENT_ID

**Critical Distinction:**

| Aspect | AZURE_CLIENT_ID (OIDC) | ARM_CLIENT_ID (Service Principal) |
|--------|------------------------|-----------------------------------|
| **Identity Type** | Federated (OIDC/JWT) | Service Principal |
| **Used By** | `azure/login`, Azure CLI | Terraform provider |
| **Credentials** | None (OIDC token) | `ARM_CLIENT_SECRET` |
| **Authentication** | Token-based (auto-rotated) | Secret-based (manual rotation) |
| **Scope** | General Azure operations | Infrastructure (Terraform) |
| **Rotation** | Automatic via OIDC | Manual secret rotation |

**They are intentionally different services in Azure!**

```
GitHub Actions
    │
    ├─ Azure Login (azure/login action)
    │  └─ Uses: AZURE_CLIENT_ID + OIDC token
    │     └─ Purpose: Authenticate Azure CLI commands
    │
    └─ Terraform Init/Apply
       └─ Uses: ARM_CLIENT_ID + ARM_CLIENT_SECRET
          └─ Purpose: Terraform provider authentication
```

**Why this design?**
- **Security Boundary:** Separates deployment from infrastructure auth
- **Least Privilege:** Each identity has specific permissions
- **Auditability:** Shows which service did what
- **Rotation:** Can rotate secrets independently

---

## Secret Configuration

### Required Secrets (8 total)

All required for cloud execution in GitHub Actions.

| Secret | Purpose | Source | Used By |
|--------|---------|--------|---------|
| `AZURE_CLIENT_ID` | OIDC authentication | Azure Entra ID | `azure-login` action |
| `AZURE_TENANT_ID` | OIDC tenant | Azure Entra ID | `azure-login` action |
| `AZURE_SUBSCRIPTION_ID` | Target subscription | Azure Portal | `azure-login`, Terraform |
| `ARM_CLIENT_ID` | Terraform auth | Service Principal | Terraform provider |
| `ARM_CLIENT_SECRET` | Terraform secret | Service Principal | Terraform provider |
| `ARM_TENANT_ID` | Terraform tenant | Service Principal | Terraform provider |
| `SQL_ADMIN_USERNAME` | SQL auth | Generate | Database deployment |
| `SQL_ADMIN_PASSWORD` | SQL password | Generate | Database deployment |

### Optional Secret (1 total)

Only needed for local testing with `act` tool.

| Secret | Purpose | Source |
|--------|---------|--------|
| `AZURE_CREDENTIALS` | Act fallback auth | Service Principal (JSON) |

---

## Setup Guide

### Step 1: OIDC Configuration

**In Azure Portal:**

1. Create Entra ID application (if not exists):
   ```powershell
   az ad app create --display-name "consilient-app"
   ```

2. Create service principal for app:
   ```powershell
   az ad sp create --id <APP_ID>
   ```

3. Add federated credentials (GitHub repo):
   ```powershell
   az ad app federated-credential create \
     --id <APP_ID> \
     --parameters '{"name":"GitHub","issuer":"https://token.actions.githubusercontent.com","subject":"repo:YOUR_ORG/consilient-webapp:ref:refs/heads/main","audiences":["api://AzureADTokenExchange"]}'
   ```

4. Assign roles:
   ```powershell
   az role assignment create \
     --assignee <APP_ID> \
     --role Contributor \
     --scope /subscriptions/<SUBSCRIPTION_ID>
   ```

**Get values:**
```powershell
az ad app show --id <APP_ID> --query appId -o tsv
# → AZURE_CLIENT_ID

az account show --query tenantId -o tsv
# → AZURE_TENANT_ID

az account show --query id -o tsv
# → AZURE_SUBSCRIPTION_ID
```

### Step 2: Service Principal for Terraform

**In Azure:**
```powershell
az ad sp create-for-rbac --name "consilient-terraform" \
  --role Contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>
```

**Get values:**
```
appId → ARM_CLIENT_ID
password → ARM_CLIENT_SECRET
tenant → ARM_TENANT_ID
```

### Step 3: Add GitHub Secrets

1. Go to: GitHub → Repository Settings → Secrets and Variables → Actions
2. Click "New repository secret"
3. Add each secret from above:
   - Name: `AZURE_CLIENT_ID`, Value: (from step 1)
   - Name: `AZURE_TENANT_ID`, Value: (from step 1)
   - Name: `AZURE_SUBSCRIPTION_ID`, Value: (from step 1)
   - Name: `ARM_CLIENT_ID`, Value: (from step 2)
   - Name: `ARM_CLIENT_SECRET`, Value: (from step 2)
   - Name: `ARM_TENANT_ID`, Value: (from step 2)
   - Name: `SQL_ADMIN_USERNAME`, Value: `dbadmin`
   - Name: `SQL_ADMIN_PASSWORD`, Value: (generate strong password)

### Step 4: Optional - Act Fallback

For local testing with `act`:

1. Create service principal for local testing:
   ```powershell
   az ad sp create-for-rbac --name "consilient-act-local"
   ```

2. Format as JSON:
   ```json
   {
     "clientId": "...",
     "clientSecret": "...",
     "subscriptionId": "...",
     "tenantId": "..."
   }
   ```

3. Add GitHub secret `AZURE_CREDENTIALS` with this JSON

---

## Workflow-Specific Authentication

### Terraform Workflow

```
terraform.yml
├─ Validate Secrets (checks all 8 required)
├─ Azure Login
│  ├─ Input: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID
│  ├─ Outputs: Access token in environment
│  └─ Fallback: AZURE_CREDENTIALS for act
│
├─ Terraform Init
│  ├─ Uses: ARM_CLIENT_ID, ARM_CLIENT_SECRET, ARM_TENANT_ID (env vars)
│  └─ Result: Downloaded providers, initialized backend
│
└─ Terraform Plan/Apply
   └─ Uses: Service principal credentials (via env vars)
```

**Environment Variables Set:**
```
TF_VAR_* → Terraform variables
ARM_CLIENT_ID → Terraform provider
ARM_CLIENT_SECRET → Terraform provider
ARM_TENANT_ID → Terraform provider
```

### Database Workflow

```
databases.yml
├─ Azure Login
│  ├─ OIDC authentication
│  └─ Sets Azure CLI context
│
├─ Discover Databases
│  └─ Lists directories in src/Databases/
│
└─ Deploy Database (for each)
   ├─ sqlcmd -G (Azure AD auth from login context)
   ├─ Executes SQL scripts
   └─ Verifies schema
```

**No explicit credentials needed:** Azure login context provides sqlcmd auth via `-G` flag.

### Composite Action: azure-login

**Location:** [`.github/actions/azure-login/`](../../../.github/actions/azure-login/)

**Behavior:**
```
if (ACT environment variable set)
  ├─ Parse AZURE_CREDENTIALS JSON
  ├─ Extract clientId, clientSecret, tenantId
  └─ az login --service-principal
else
  ├─ Use azure/login@v2.3.0
  ├─ OIDC authentication
  └─ No long-lived secrets
```

**Code:** [action.yml:28-58](../../../.github/actions/azure-login/action.yml#L28-L58)

---

## Troubleshooting

### Client ID Confusion

**Problem:** Which client ID should I use?

**Solution:** See the comparison table above. Different IDs for different purposes.

### OIDC Setup Issues

**Problem:** OIDC token exchange fails

**Solution:**
1. Verify federated credentials created in Azure
2. Check GitHub repo name matches in federated credential
3. Verify AZURE_CLIENT_ID matches app ID
4. Wait 1-2 minutes for RBAC propagation

### Service Principal Permissions

**Problem:** Terraform fails with permission denied

**Solution:**
1. Verify ARM_CLIENT_ID has Contributor role
2. Check on correct subscription
3. Run: `az role assignment list --assignee <ARM_CLIENT_ID>`

### Secret Not Found

**Problem:** Workflow fails with "secret not found"

**Solution:**
1. Go to GitHub Settings → Secrets and Variables
2. Check secret name (case-sensitive)
3. Verify secret not empty
4. Check secret is in correct repo (not organization level)

See [TROUBLESHOOTING.md#authentication-issues](../TROUBLESHOOTING.md#authentication-issues) for more scenarios.

---

## Security Best Practices

1. **Rotation Schedule:**
   - OIDC tokens: Auto-rotated (hourly)
   - Service principal secrets: Rotate quarterly
   - SQL passwords: Rotate annually

2. **Least Privilege:**
   - AZURE_CLIENT_ID: Only needed Azure permissions
   - ARM_CLIENT_ID: Only infrastructure permissions
   - SQL: Limited SQL admin account

3. **Secret Management:**
   - Never commit secrets to Git
   - Use GitHub Secrets (encrypted)
   - Consider Azure Key Vault for production

4. **Audit & Monitoring:**
   - Enable Azure Activity Log
   - Review role assignments quarterly
   - Monitor failed authentication attempts

5. **Incident Response:**
   - Compromise: Rotate affected secrets immediately
   - Revoke old credentials
   - Audit access logs for suspicious activity

---

## Related Files

- [reference/secrets-checklist.md](../reference/secrets-checklist.md) - Quick secret setup
- [components/github-actions.md](github-actions.md) - Workflow details
- [components/local-testing.md](local-testing.md) - Act setup guide
- [TROUBLESHOOTING.md#authentication-issues](../TROUBLESHOOTING.md#authentication-issues) - Error diagnosis

---

**Last Updated:** December 2025
**For Navigation:** See [README.md](../README.md)
