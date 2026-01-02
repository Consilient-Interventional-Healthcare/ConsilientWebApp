# GitHub Variables Setup Guide

This document provides step-by-step instructions for creating GitHub Repository Variables required by the updated workflows.

## Overview

The workflows have been updated to use GitHub Variables for non-sensitive configuration. These variables need to be created manually in GitHub's web interface.

### Variable Categories

- **Repository Variables** (shared across all environments)
- **Environment Variables** (specific to dev/prod)

---

## Repository Variables (Shared Configuration)

These variables are used by all workflows and environments.

**Navigation**: Repository → Settings → Secrets and variables → Actions → **Variables** tab

### Create These 9 Variables

| Variable Name | Value | Description | Used By |
|---------------|-------|-------------|---------|
| `SQL_ADMIN_USERNAME` | `sqladmin` | SQL Server admin username | terraform.yml |
| `AZURE_SQL_SERVER_FQDN` | `<your-server>.database.windows.net` | SQL Server fully qualified domain name | databases.yml |
| `ACR_REGISTRY_URL` | `<your-registry>.azurecr.io` | Azure Container Registry URL | dotnet_apps.yml, react_apps.yml |
| `API_IMAGE_NAME` | `consilientapi` | Docker image name for API | dotnet_apps.yml |
| `REACT_IMAGE_NAME` | `consilientwebapp2` | Docker image name for React app | react_apps.yml |
| `CONTAINER_REGISTRY` | `ghcr.io` | Container registry for runner image | build-runner-image.yml |
| `SQL_SERVER_VERSION` | `2022-latest` | SQL Server Docker image version | docs_db.yml |
| `SCHEMASPY_VERSION` | `6.2.4` | SchemaSpy version for documentation | docs_db.yml |
| `JDBC_DRIVER_VERSION` | `12.4.2.jre11` | JDBC driver version | docs_db.yml |

### Steps to Create Repository Variables

1. Go to your GitHub repository
2. Click **Settings** tab
3. In the left sidebar, click **Secrets and variables**
4. Click **Actions**
5. Click the **Variables** tab (NOT Secrets)
6. Click **New repository variable**
7. For each variable above:
   - **Name**: Copy the variable name exactly
   - **Value**: Enter the appropriate value
   - Click **Add secret**

### Example Values

#### SQL Server
- **AZURE_SQL_SERVER_FQDN**: `myserver.database.windows.net`
  - Format: `<servername>.database.windows.net`
  - Find in: Azure Portal → SQL databases → Server name

#### Azure Container Registry
- **ACR_REGISTRY_URL**: `myregistry.azurecr.io`
  - Format: `<registryname>.azurecr.io`
  - Find in: Azure Portal → Container Registries → Login server

#### Image Names
- **API_IMAGE_NAME**: `consilientapi` (matches your Dockerfile naming)
- **REACT_IMAGE_NAME**: `consilientwebapp2` (matches your Dockerfile naming)

---

## Environment Variables (Per-Environment Configuration)

These variables are scoped to specific environments (dev/prod).

**Navigation**: Repository → Settings → Environments → **[dev|prod]** → Variables

### Already Exist (Keep As-Is)
- `AZURE_REGION`
- `AZURE_RESOURCE_GROUP_NAME`

**No new environment variables needed for this phase.**

---

## Verification

### After Creating All Variables

Test that workflows can access them:

1. **Via GitHub UI**:
   - Go to Settings → Secrets and variables → Actions → Variables
   - Verify all 9 variables are listed

2. **Via GitHub CLI** (optional):
   ```bash
   gh variable list --repo YOUR_OWNER/YOUR_REPO
   ```

   Should show all 9 variables with type "Variables"

3. **Via Workflow Test** (recommended):
   - Trigger a workflow manually
   - Check workflow logs to verify variables are resolved
   - Debug output will show variable sources

### Debug Output in Workflows

When workflows run, they will show:
```
=== Configuration Source ===
AZURE_REGION: canadacentral (from vars)
AZURE_RESOURCE_GROUP_NAME: consilient-resource-group (from vars)
SQL_ADMIN_USERNAME: sqladmin (from vars)
AZURE_SQL_SERVER_FQDN: myserver.database.windows.net (from vars)
ACR_REGISTRY_URL: myregistry.azurecr.io (from vars)
API_IMAGE_NAME: consilientapi (from vars)
REACT_IMAGE_NAME: consilientwebapp2 (from vars)
CONTAINER_REGISTRY: ghcr.io (from vars)
SQL_SERVER_VERSION: 2022-latest (from vars)
SCHEMASPY_VERSION: 6.2.4 (from vars)
JDBC_DRIVER_VERSION: 12.4.2.jre11 (from vars)
```

---

## Troubleshooting

### Variable Not Found in Workflow

**Symptom**: Workflow uses fallback value (hardcoded default) instead of variable

**Solution**:
1. Verify variable exists in GitHub UI
2. Check exact variable name matches (case-sensitive)
3. Verify variable is in **Variables** tab (not Secrets)
4. Wait a few seconds - GitHub may need time to sync

### Workflow Fails with Variable Value

**Symptom**: Workflow runs but fails during execution

**Solution**:
1. Verify variable value is correct (especially URLs)
2. Check special characters aren't present
3. For `AZURE_SQL_SERVER_FQDN`:
   - Must be FQDN: `server.database.windows.net`
   - NOT hostname: `server`
   - NOT connection string
4. For `ACR_REGISTRY_URL`:
   - Must end with `.azurecr.io`
   - Find in Azure Portal under Container Registries

### How Fallbacks Work

If variable is not found, workflows use fallback values:
```yaml
TF_VAR_sql_admin_username: ${{ vars.SQL_ADMIN_USERNAME || 'sqladmin' }}
```

- If `vars.SQL_ADMIN_USERNAME` exists → use it
- If not found → use `'sqladmin'` (fallback)

This ensures workflows don't fail if variables are missing.

---

## Benefits

Once variables are created:

✅ **Easier Updates**: Change configuration without editing code
✅ **Version Management**: Update SQL/SchemaSpy versions in one place
✅ **Better Visibility**: Non-sensitive data visible in logs for debugging
✅ **Local Testing**: Variables propagated to `infra/github_emulator/.env`
✅ **Security**: Truly sensitive data (passwords) stay as secrets

---

## Related Files

- [.github/workflows/terraform.yml](.github/workflows/terraform.yml) - Uses: `SQL_ADMIN_USERNAME`
- [.github/workflows/databases.yml](.github/workflows/databases.yml) - Uses: `AZURE_SQL_SERVER_FQDN`
- [.github/workflows/dotnet_apps.yml](.github/workflows/dotnet_apps.yml) - Uses: `API_IMAGE_NAME`, `ACR_REGISTRY_URL`
- [.github/workflows/react_apps.yml](.github/workflows/react_apps.yml) - Uses: `REACT_IMAGE_NAME`, `ACR_REGISTRY_URL`
- [.github/workflows/docs_db.yml](.github/workflows/docs_db.yml) - Uses: `SQL_SERVER_VERSION`, `SCHEMASPY_VERSION`, `JDBC_DRIVER_VERSION`
- [.github/workflows/build-runner-image.yml](.github/workflows/build-runner-image.yml) - Uses: `CONTAINER_REGISTRY`
- [infra/github_emulator/.env](infra/github_emulator/.env) - Local testing configuration

---

## Next Steps

1. ✅ Create all 9 repository variables in GitHub
2. ✅ Verify variables in GitHub UI
3. Run a test workflow to confirm variables resolve correctly
4. Monitor workflow logs for any variable-related issues

---

## Support

If you encounter issues:
1. Check exact variable names (case-sensitive)
2. Verify values match expected format
3. Look for typos in variable values
4. Check GitHub Actions workflow logs for error messages
5. Refer to [docs/ACR_SETUP.md](ACR_SETUP.md) for Azure resource information
