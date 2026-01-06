# Troubleshooting Guide

Comprehensive guide to diagnosing and fixing common infrastructure issues.

## Terraform Errors

### State Lock Issues

**Error:** `Error acquiring the state lock: ... code: ResourceGroupBeingDeleted`

**Diagnosis:**
- Another terraform operation is in progress
- Previous operation terminated unexpectedly
- Resource group deletion in progress

**Solution:**
```powershell
cd infra/terraform

# Force unlock (use carefully!)
terraform force-unlock <LOCK_ID>

# Or start fresh
rm terraform.tfstate*
terraform init
terraform plan
```

**Prevention:**
- Always use `terraform plan` before `apply`
- Don't interrupt terraform operations
- Use remote backend for team environments

**Related Files:** [`backend.tf`](../../../infra/terraform/backend.tf)

---

### Import Loop Issues

**Error:** Terraform keeps importing resources unnecessarily

**Diagnosis:**
- State file not persisting between runs
- Using `act` without `--bind` flag
- Previous state file deleted

**Solution:**

For `act` users:
```powershell
# The --bind flag is automatically enabled by run-act.ps1
# This ensures state persistence between runs

# Verify state persistence
ls -l infra/terraform/terraform.tfstate*
```

For cloud users:
```powershell
terraform state list
# Should show existing resources
```

**Prevention:**
- Use `--bind` flag with act (already in `run-act.ps1`)
- Don't delete `terraform.tfstate` without reason
- Use remote backend for team environments

**Related Files:**
- [`infra/act/IMPORT_LOOP_FIX.md`](../../../infra/act/IMPORT_LOOP_FIX.md) - Performance details
- [`infra/terraform/.terraform/`](../../../infra/terraform/) - State directory

---

### Provider Authentication Error

**Error:** `Error: Error building AzureRM Provider: ... authentication`

**Diagnosis:**
- Azure CLI not authenticated
- Service Principal credentials invalid
- Wrong subscription selected

**Solution:**

For Cloud (GitHub Actions):
```powershell
# Verify GitHub secrets are set (in GitHub UI)
# Check secret names:
# - AZURE_CLIENT_ID
# - AZURE_TENANT_ID
# - AZURE_SUBSCRIPTION_ID
# - ARM_CLIENT_ID
# - ARM_CLIENT_SECRET
# - ARM_TENANT_ID
```

For Local:
```powershell
# Re-authenticate
az login
az account set --subscription <SUBSCRIPTION_ID>
az account show  # Verify subscription
```

For act:
```powershell
# Verify .env.act has credentials
cd infra/act
cat .env.act | findstr ARM_
```

**Related Files:** [reference/secrets-checklist.md](reference/secrets-checklist.md)

---

## GitHub Actions Failures

### Secret Validation Errors

**Error:** `Error: ... secret ... is required but not set`

**Diagnosis:**
- Secret not configured in GitHub
- Secret name mismatch
- Secret is empty value

**Solution:**

1. List required secrets:
```powershell
# See: reference/secrets-checklist.md
```

2. Verify in GitHub:
   - Go to: Settings → Secrets and Variables → Actions
   - Check each secret exists
   - Verify exact spelling (case-sensitive)

3. Re-add if missing:
   - Name: `AZURE_CLIENT_ID` (exact spelling)
   - Secret: (copy from Azure)

**Prevention:**
- Use [reference/secrets-checklist.md](reference/secrets-checklist.md) as checklist
- Screenshot secret names before creating
- Test with dummy value first

---

### OIDC Authentication Fails

**Error:** `Error: OIDC token exchange failed`

**Diagnosis:**
- Federated credentials not configured
- Client ID not registered in Azure
- OIDC provider not linked to GitHub repo

**Solution:**

1. Verify federated credentials in Azure:
```powershell
az ad app federated-credential list --id <APP_ID>
```

2. Create if missing:
```powershell
# See: components/authentication.md#oidc-authentication
# For detailed setup instructions
```

3. Verify GitHub secrets:
   - AZURE_CLIENT_ID matches app ID
   - AZURE_TENANT_ID correct
   - AZURE_SUBSCRIPTION_ID correct

**Related Files:** [components/authentication.md](components/authentication.md#oidc-authentication)

---

### Workflow Trigger Issues

**Error:** Workflow doesn't trigger on push or manual dispatch

**Diagnosis:**
- Branch protection rules blocking workflow
- Workflow file not in correct location
- Trigger conditions not met
- Syntax error in workflow file

**Solution:**

1. Check workflow location: `/.github/workflows/terraform.yml`

2. Verify syntax:
```powershell
# Test YAML syntax
# Tools: yamllint, online YAML validators
```

3. Check trigger conditions:
```yaml
# See: .github/workflows/terraform.yml:5-15
on:
  push:
    branches: [ main ]
  workflow_dispatch:
  workflow_call:
```

4. Push to correct branch (main, develop, or feature/**)

**Related Files:** [components/github-actions.md](components/github-actions.md#workflow-architecture)

---

## Authentication Issues

### AZURE_CLIENT_ID vs ARM_CLIENT_ID Confusion

**Problem:** "Why are there two different client IDs?"

**Explanation:**

| Variable | Purpose | Source |
|----------|---------|--------|
| AZURE_CLIENT_ID | OIDC Cloud Auth | Entra ID Application |
| ARM_CLIENT_ID | Terraform Provider | Service Principal |

They often have different values:
- AZURE_CLIENT_ID: App-only, for OIDC token exchange
- ARM_CLIENT_ID: Service Principal, for Terraform API calls

**Solution:**
- Don't confuse them
- Use [reference/secrets-checklist.md](reference/secrets-checklist.md) as guide
- Verify each in Azure (they're different objects)

**Related Files:** [components/authentication.md](components/authentication.md#service-principal)

---

### Service Principal Permission Error

**Error:** `Error: creating/updating resource: ... Insufficient permissions`

**Diagnosis:**
- Service Principal missing required role
- RBAC assignment not yet propagated
- Subscription context wrong

**Solution:**

1. Verify service principal has Contributor role:
```powershell
az role assignment list \
  --assignee <ARM_CLIENT_ID> \
  --scope /subscriptions/<SUBSCRIPTION_ID>
```

2. Add role if missing:
```powershell
az role assignment create \
  --assignee <ARM_CLIENT_ID> \
  --role Contributor \
  --scope /subscriptions/<SUBSCRIPTION_ID>
```

3. Wait 1-2 minutes for RBAC to propagate

---

### OIDC Setup Problems

**Problem:** "How do I set up OIDC?"

**Solution:**

See [components/authentication.md#oidc-authentication](components/authentication.md#oidc-authentication) for complete step-by-step guide.

Quick checklist:
- [ ] Azure app registered in Entra ID
- [ ] Federated credentials configured (GitHub repo)
- [ ] Secrets configured in GitHub (AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID)
- [ ] Test with `terraform plan` in cloud

---

## Database Deployment

### Auto-Discovery Not Finding Databases

**Error:** "No databases found" or empty deployment

**Diagnosis:**
- SQL scripts not in correct directory structure
- Directory name not recognized
- Scripts have incorrect names

**Solution:**

1. Verify directory structure:
```
src/Databases/
├── Main/
│   └── Schema/
│       └── 001_*.sql
```

2. Check directory naming:
   - Directory: `Main` → Database: `consilient_main_dev`
   - Directory: `Hangfire` → Database: `consilient_hangfire_dev`
   - New directory auto-creates new database

3. Verify script naming:
   - Schema: `001_`, `002_`, etc.
   - Seed: `seed_*.sql`
   - System: `_*.sql` or `.*` (skipped)

**Related Files:** [components/databases.md](components/databases.md#auto-discovery)

---

### SQL Script Execution Failures

**Error:** `sqlcmd error ...` or `script execution failed`

**Diagnosis:**
- SQL syntax error in script
- Object already exists
- Permission denied
- Timeout (script takes >600 seconds)

**Solution:**

1. Test script locally:
```powershell
sqlcmd -S <SERVER> -d <DATABASE> \
  -U <USERNAME> -P <PASSWORD> \
  -i script.sql
```

2. Check syntax:
```sql
-- Make sure each statement ends with GO
CREATE TABLE Users (
  ID INT PRIMARY KEY,
  Name NVARCHAR(100)
);
GO
```

3. Handle existing objects:
```sql
-- Use IF NOT EXISTS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
  CREATE TABLE Users (...)
END
GO
```

4. For long-running scripts:
```yaml
# In databases.yml, adjust timeout
timeout: '1200'  # 20 minutes instead of 10
```

**Related Files:** [components/databases.md](components/databases.md#sql-script-execution)

---

### Connection String Issues

**Error:** `Cannot connect to database ...` or `Login failed`

**Diagnosis:**
- Database not created yet
- SQL credentials wrong
- Firewall blocking access
- Server name incorrect

**Solution:**

1. Verify database exists:
```powershell
az sql db list --resource-group <RG> --server <SERVER>
```

2. Check credentials:
   - Username: `SQL_ADMIN_PASSWORD` GitHub secret
   - Server: From Terraform outputs
   - Database: `consilient_main_dev`, etc.

3. Verify firewall (dev only):
```powershell
# Add local IP to firewall
az sql server firewall-rule create \
  --resource-group <RG> \
  --server <SERVER> \
  --name AllowLocal \
  --start-ip-address <YOUR_IP> \
  --end-ip-address <YOUR_IP>
```

**Related Files:** [components/databases.md](components/databases.md#connection-configuration)

---

## Database Documentation

### Schema Discovery Fails

**Error:** "Schema discovery failed" or "No schemas discovered"

**Diagnosis:**
- Database doesn't exist yet (Terraform hasn't run)
- Azure SQL Server secret not configured
- Database has no user-created schemas
- Firewall blocking SQL access

**Solution:**

1. Verify database exists:
```powershell
az sql db list \
  --resource-group <RESOURCE_GROUP> \
  --server <SERVER_NAME>
```

2. Check Azure SQL Server secret configured:
   - GitHub Settings → Secrets and variables → Actions
   - Look for `AZURE_SQL_SERVER`
   - Should be FQDN: `server.database.windows.net`

3. Verify database has schemas:
```sql
SELECT name FROM sys.schemas
WHERE schema_id > 4
  AND name NOT IN ('sys','INFORMATION_SCHEMA','guest')
ORDER BY name;
```

4. Check firewall (for local testing with act):
```powershell
az sql server firewall-rule list \
  --resource-group <RESOURCE_GROUP> \
  --server <SERVER_NAME>
```

**Related Files:** [components/database-documentation.md](components/database-documentation.md#schema-discovery-fails)

---

### SchemaSpy Documentation Generation Timeout

**Error:** `Timeout reached after 600 seconds` or `Process killed after timeout`

**Diagnosis:**
- Large database with complex relationships
- Many foreign keys requiring analysis
- Network latency with Azure SQL
- SchemaSpy taking longer than timeout

**Solution:**

**Option 1: Exclude Large Schemas (Recommended)**

Edit `src/Databases/{Name}/db_docs.yml`:
```yaml
schemas:
  exclude:
    - "LargeArchiveSchema"
    - "HistoricalData"
```

Then regenerate: Go to GitHub Actions → Re-run workflow

**Option 2: Increase Timeout**

Edit `.github/workflows/database-docs.yml` (temporary fix):
```yaml
env:
  SCHEMASPY_TIMEOUT_SECONDS: 900  # 15 minutes instead of 10
```

**Option 3: Check Schema Complexity**

See how many tables/relationships:
```sql
SELECT COUNT(*) FROM sys.tables WHERE schema_id = SCHEMA_ID('{SchemaName}');
SELECT COUNT(*) FROM sys.foreign_keys;
```

Large numbers may indicate need for exclusion.

**Note:** SchemaSpy already runs with `-norows` flag (skips expensive row count queries).

**Related Files:** [components/database-documentation.md](components/database-documentation.md#schemaspytimeout)

---

### Documentation Not Generating

**Error:** Workflow completes but no artifact, or "SKIP_DOCUMENTATION=true"

**Diagnosis:**
- `generate_docs: false` in `db_docs.yml`
- All schemas excluded (none to document)
- Workflow manually skipped

**Solution:**

1. Check database configuration:
```bash
cat src/Databases/{Name}/db_docs.yml
# Should show: generate_docs: true
```

2. Verify schema exclusions:
```yaml
schemas:
  exclude: []  # Empty = document all schemas
```

3. Check for all schemas excluded:
```yaml
schemas:
  exclude:
    - "schema1"
    - "schema2"
    # If ALL user schemas are here, nothing gets documented!
```

4. Verify database has schemas:
   See "Schema Discovery Fails" above

5. Check workflow was not skipped:
   - If triggered manually: ensure `skip_db_docs: false` (not true)
   - Check main.yml input parameters

**Solution:** Set `generate_docs: true` and remove unnecessary exclusions, then re-run workflow.

**Related Files:** [components/database-documentation.md](components/database-documentation.md#documentation-not-generating)

---

### db_docs.yml Not Recognized

**Error:** Database discovered but configuration ignored, using defaults

**Diagnosis:**
- File name misspelled (not exactly `db_docs.yml`)
- Wrong location (not in database directory root)
- YAML syntax error (indentation, tabs)

**Solution:**

1. Verify exact file name:
```bash
ls -la src/Databases/{Name}/
# Must show exactly: db_docs.yml (not db-docs.yml or db_docs.yaml)
```

2. Check file location (must be in database root):
```
Correct:   src/Databases/Main/db_docs.yml ✅
Wrong:     src/Databases/Main/Schema/db_docs.yml ❌
Wrong:     src/Databases/Main/db-docs.yml ❌
```

3. Validate YAML syntax:
```bash
# Check for tabs (should be none - only spaces)
cat src/Databases/{Name}/db_docs.yml | grep -P '\t'
# No output = correct
```

4. Use online YAML validator:
   - Copy file contents
   - Paste to https://www.yamllint.com/
   - Fix any errors shown

5. Verify indentation:
   ```yaml
   database:           # 0 spaces
     name: "MyDB"      # 2 spaces
     generate_docs: true  # 2 spaces

   schemas:            # 0 spaces
     exclude: []       # 2 spaces
   ```

**Related Files:** [components/database-documentation.md](components/database-documentation.md#dbdocsyml-not-recognized)

---

### Excluded Schemas Still Appearing in Docs

**Error:** Added schema to `exclude` list but still appears in documentation

**Diagnosis:**
- Artifact is cached from previous run
- Schema name case mismatch
- Configuration changed after workflow ran

**Solution:**

1. **Regenerate documentation:**
   - Go to GitHub Actions
   - Select workflow: "05 - Generate DB Docs"
   - Click "Run workflow"
   - Wait for new artifact

2. **Clear browser cache:**
   - Hard refresh: Ctrl+Shift+R (Windows) or Cmd+Shift+R (Mac)
   - Or download fresh artifact

3. **Verify schema name matching:**
   - Matching is case-INSENSITIVE
   - Both "Sales" and "SALES" match "sales"
   - Check exact name in database:
   ```sql
   SELECT name FROM sys.schemas WHERE schema_id > 4;
   ```

4. **Confirm configuration saved:**
   - Commit `db_docs.yml` changes
   - Push to repository
   - Trigger workflow again

**Related Files:** [components/database-documentation.md](components/database-documentation.md#excluded-schemas-still-appearing-in-docs)

---

## Azure Resources

### Container App Environment Conflicts

**Error:** `... already exists ...` or `CAE conflict`

**Diagnosis:**
- CAE (Container App Environment) shared between environments
- Multiple deployments trying to use same CAE
- Resource name collision

**Solution:**

1. Check CAE configuration in [`locals.tf`](../../../infra/terraform/locals.tf):

```hcl
# For cost savings, can share CAE across environments
use_shared_container_environment = true  # dev/staging share
shared_container_environment_name = "consilient-cae-shared"
```

2. Options:
   - **Option A (Save money):** Use shared CAE (`use_shared_container_environment = true`)
   - **Option B (Isolate):** Use separate CAE (`use_shared_container_environment = false`)

3. After changing:
```powershell
terraform plan  # Review changes
terraform apply
```

---

### App Service Deployment Failures

**Error:** Docker image pull fails or container won't start

**Diagnosis:**
- Docker image not in ACR
- ACR credentials wrong
- Image name incorrect
- Container health check failing

**Solution:**

1. Verify image in ACR:
```powershell
az acr repository list --resource-group <RG> --name <ACR>
az acr repository show-tags \
  --resource-group <RG> \
  --name <ACR> \
  --repository consilientapi
```

2. Check App Service logs:
```powershell
az webapp log stream \
  --resource-group <RG> \
  --name <APP_NAME>
```

3. Verify health check endpoint:
```powershell
curl https://<APP>.azurewebsites.net/health
```

4. Trigger redeployment:
```powershell
# Commit and push to trigger build/deploy
git commit --allow-empty -m "Trigger deployment"
git push
```

**Related Files:** [components/azure-resources.md](components/azure-resources.md#app-services)

---

## Local Testing (Act)

### Docker Not Running

**Error:** `Cannot connect to Docker` or `Docker socket not found`

**Solution:**
1. Start Docker Desktop
2. Wait for Docker to fully start (1-2 minutes)
3. Verify: `docker ps`
4. Try `act` again

---

### act CLI Not Found

**Error:** `act: command not found` or `act is not recognized`

**Solution:**
```powershell
# Install (Windows with Chocolatey)
choco install act-cli

# Or verify installation
act --version

# If still not found, add to PATH or use full path
C:\ProgramData\chocolatey\bin\act.exe workflow_dispatch
```

---

### Secret File Missing

**Error:** `AZURE_CREDENTIALS not found` or auth fails in act

**Solution:**
```powershell
cd infra/act

# Check if .env.act exists
ls -la .env.act

# If missing, copy from template
copy .env.act.example .env.act

# Edit with your credentials
notepad .env.act
```

---

### Terraform State Issues in Act

**Error:** Terraform keeps importing resources on each run

**Solution:**

The `run-act.ps1` script automatically enables state persistence with the `--bind` flag. If you see repeated imports:

```powershell
# Verify state files exist and persist
ls -la infra/terraform/terraform.tfstate*

# If they're missing or dated before your last run, state isn't persisting
# Run the script again - it should use existing state
```

See [infra/act/IMPORT_LOOP_FIX.md](../../../infra/act/IMPORT_LOOP_FIX.md) for performance details.

---

### Performance Issues

**Problem:** Act is running very slowly

**Causes & Solutions:**

| Cause | Solution |
|-------|----------|
| First run (no Docker image) | Normal, takes ~10 min; use cache after |
| No `--bind` flag | State doesn't persist, imports every time (5x slower) |
| Docker resources low | Allocate more CPU/memory to Docker |
| Large workspace | Mount only needed directories |

---

## General Troubleshooting Steps

### 1. Check Logs
- **GitHub Actions:** View in Actions tab
- **Local:** Run in verbose mode: `terraform -v plan`
- **Azure:** Use Activity Log in Azure Portal

### 2. Verify Configuration
- **Terraform:** `terraform validate`
- **GitHub:** Check `.github/workflows/` YAML syntax
- **Secrets:** Verify in GitHub Settings (check names exactly)

### 3. Test Components Individually
- **Terraform:** `terraform plan` without applying
- **Database:** Test SQL script locally with sqlcmd
- **Act:** Run single workflow: `.\run-act.ps1 -SkipTerraform`

### 4. Check Documentation
- **Terraform:** [components/terraform.md](components/terraform.md)
- **CI/CD:** [components/github-actions.md](components/github-actions.md)
- **Database:** [components/databases.md](components/databases.md)
- **Authentication:** [components/authentication.md](components/authentication.md)
- **Local Testing:** [components/local-testing.md](components/local-testing.md)

### 5. Ask for Help
- Create GitHub Issue with error message
- Reference [KNOWN_ISSUES.md](KNOWN_ISSUES.md) for known problems
- Check recent commits for recent changes

---

## Quick Reference: Common Errors

| Error Message | Most Likely Cause | See |
|---------------|-------------------|-----|
| `secret ... is required` | GitHub secret not set | [reference/secrets-checklist.md](reference/secrets-checklist.md) |
| `state lock` | Terraform operation in progress | State Lock Issues (above) |
| `Import loop` | State not persisting in act | [infra/act/IMPORT_LOOP_FIX.md](../../../infra/act/IMPORT_LOOP_FIX.md) |
| `No databases found` | Wrong directory structure | [components/databases.md](components/databases.md) |
| `sqlcmd error` | SQL syntax error | Test script locally |
| `OIDC token exchange failed` | Federated credentials not set | [components/authentication.md](components/authentication.md#oidc-authentication) |
| `Cannot connect to Docker` | Docker not running | Docker Not Running (above) |

---

**Last Updated:** December 2025
**For Navigation:** See [README.md](README.md)
**For Known Issues:** See [KNOWN_ISSUES.md](KNOWN_ISSUES.md)
