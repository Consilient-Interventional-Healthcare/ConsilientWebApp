# Tier 2 Implementation Checklist

## Status: Code Complete ✅ | Manual Setup Required 📋

This checklist tracks the setup required to complete **Tier 2: Should Do (Best Practices)**.

---

## What's Been Done (Code Level)

### Phase 2.1: Workflow Updates ✅
- [x] Updated 6 workflows to use GitHub Variables
  - [x] terraform.yml
  - [x] databases.yml
  - [x] dotnet_apps.yml
  - [x] react_apps.yml
  - [x] docs_db.yml
  - [x] build-runner-image.yml

### Phase 2.2: Local Testing Configuration ✅
- [x] Updated `.env` file with 9 new variables
- [x] All workflows have fallback values (safe defaults)
- [x] Added comprehensive comments in workflows

### Phase 3B: OIDC Security ✅
- [x] Extended OIDC authentication to terraform.yml
- [x] Extended OIDC authentication to databases.yml
- [x] Maintained service principal fallback for local `act` testing
- [x] All workflows now support OIDC + act dual-auth pattern

### Documentation ✅
- [x] Created GITHUB_VARIABLES_SETUP.md guide
- [x] Added terraform.tfvars.example template
- [x] Updated .env for local testing

---

## What You Need to Do (Manual GitHub Setup)

### Create 9 GitHub Repository Variables

Navigate to: **Settings → Secrets and variables → Actions → Variables tab**

**Click "New repository variable" for each:**

- [ ] `SQL_ADMIN_USERNAME` = `sqladmin`
- [ ] `AZURE_SQL_SERVER_FQDN` = `<your-server>.database.windows.net`
- [ ] `ACR_REGISTRY_URL` = `<your-registry>.azurecr.io`
- [ ] `API_IMAGE_NAME` = `consilientapi`
- [ ] `REACT_IMAGE_NAME` = `consilientwebapp2`
- [ ] `CONTAINER_REGISTRY` = `ghcr.io`
- [ ] `SQL_SERVER_VERSION` = `2022-latest`
- [ ] `SCHEMASPY_VERSION` = `6.2.4`
- [ ] `JDBC_DRIVER_VERSION` = `12.4.2.jre11`

**Time Required**: ~5-10 minutes

---

## What You Need to Find (From Your Azure/GitHub)

Before creating variables, gather these values:

- [ ] **SQL Server FQDN**: Find in Azure Portal → SQL databases → Server name
  - Format: `servername.database.windows.net`
  - Example: `consilient-dev.database.windows.net`

- [ ] **ACR Registry URL**: Find in Azure Portal → Container Registries → Login server
  - Format: `registryname.azurecr.io`
  - Example: `consilientacr.azurecr.io`

---

## Testing Checklist

After creating variables:

### Quick Test (GitHub UI)
- [ ] Go to Settings → Secrets and variables → Actions
- [ ] Click **Variables** tab
- [ ] Count 9 variables listed

### Workflow Test (Recommended)
- [ ] Trigger any workflow manually
- [ ] Check workflow logs for variable resolution
- [ ] Look for debug output showing variables:
  ```
  SQL_ADMIN_USERNAME: sqladmin (from vars)
  AZURE_SQL_SERVER_FQDN: myserver.database.windows.net (from vars)
  ```

### Full Integration Test (Optional)
- [ ] Run terraform plan workflow
- [ ] Run database deployment workflow
- [ ] Run dotnet/react app deployment
- [ ] Verify all use variables instead of hardcoded values

---

## What This Achieves

After completing Tier 2:

✅ **Better Debugging**
- Non-sensitive configuration visible in logs
- Easier to troubleshoot configuration issues

✅ **Flexible Configuration**
- Change versions without editing workflows
- Update server/registry URLs easily
- No code changes needed for config updates

✅ **Security Improvement**
- Reduces noise in secrets management
- Makes truly sensitive data (passwords) stand out
- Non-sensitive data treated appropriately

✅ **Local Testing Support**
- `.env` file enables `act` testing with variables
- All workflows work locally and in cloud

✅ **OIDC Authentication**
- No long-lived client secrets in GitHub Actions cloud
- Service principal fallback for local testing
- Industry-standard authentication pattern

---

## After Tier 2 is Complete

### Optional: Tier 3 Enhancements

When ready, consider:
- [ ] Parameterize additional hardcoded values
- [ ] Standardize secret naming conventions
- [ ] Create staging environment
- [ ] Rotate exposed SQL password (if needed)

### Future: Tier 1 Maintenance

- [ ] Rotate secrets periodically (90 days)
- [ ] Review variable values for accuracy
- [ ] Document any custom values

---

## Quick Links

- **Setup Guide**: [docs/GITHUB_VARIABLES_SETUP.md](docs/GITHUB_VARIABLES_SETUP.md)
- **Terraform Example**: [infra/terraform/terraform.tfvars.example](infra/terraform/terraform.tfvars.example)
- **Local Testing**: [infra/github_emulator/.env](infra/github_emulator/.env)
- **Workflows**: [.github/workflows/](.github/workflows/)

---

## Support

If you need help:

1. Check [GITHUB_VARIABLES_SETUP.md](docs/GITHUB_VARIABLES_SETUP.md) for detailed instructions
2. Verify variable names match exactly (case-sensitive)
3. Ensure values are correct (especially URLs)
4. Check workflow logs for error messages
5. Look for warnings about missing variables

---

**Status**: Ready for GitHub UI setup! 🚀
