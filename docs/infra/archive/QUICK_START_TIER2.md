# Quick Start: Tier 2 GitHub Variables Setup

**Time Required**: 5-10 minutes
**Complexity**: Easy (no coding)
**Impact**: All 6 workflows automatically updated

---

## The Simplest Path Forward

### Step 1: Gather Your Azure Values (2 minutes)

Find these in Azure Portal:

1. **SQL Server FQDN**
   - Go to: Azure Portal → SQL databases
   - Find your server
   - Copy the "Server name" (format: `servername.database.windows.net`)

2. **ACR Registry URL**
   - Go to: Azure Portal → Container Registries
   - Find your registry
   - Copy the "Login server" (format: `registryname.azurecr.io`)

### Step 2: Create 9 GitHub Variables (5-8 minutes)

1. Go to your GitHub repository
2. Click **Settings** tab
3. Click **Secrets and variables** (left sidebar)
4. Click **Actions**
5. Click **Variables** tab (⚠️ NOT Secrets!)
6. Click **New repository variable**

**Copy-paste these 9 variables** (click "Add secret" after each):

```
1. Name: SQL_ADMIN_USERNAME
   Value: sqladmin

2. Name: AZURE_SQL_SERVER_FQDN
   Value: [PASTE YOUR SQL SERVER FQDN HERE]

3. Name: ACR_REGISTRY_URL
   Value: [PASTE YOUR ACR REGISTRY URL HERE]

4. Name: API_IMAGE_NAME
   Value: consilientapi

5. Name: REACT_IMAGE_NAME
   Value: consilientwebapp2

6. Name: CONTAINER_REGISTRY
   Value: ghcr.io

7. Name: SQL_SERVER_VERSION
   Value: 2022-latest

8. Name: SCHEMASPY_VERSION
   Value: 6.2.4

9. Name: JDBC_DRIVER_VERSION
   Value: 12.4.2.jre11
```

### Step 3: Verify (1-2 minutes)

1. Go to Settings → Secrets and variables → Actions → **Variables** tab
2. Count: Should see 9 variables listed

Done! 🎉

---

## That's It!

Your workflows now:
- ✅ Use GitHub Variables for configuration
- ✅ Support OIDC authentication (secure)
- ✅ Work locally with `act` tool
- ✅ Display non-sensitive data in logs (easier debugging)

---

## Optional: Test It

Run any workflow manually to see it pick up the variables:

1. Go to **Actions** tab
2. Click any workflow (e.g., "Terraform")
3. Click **Run workflow** button
4. Check logs to see variables resolving

Look for output like:
```
SQL_ADMIN_USERNAME: sqladmin (from vars)
AZURE_SQL_SERVER_FQDN: myserver.database.windows.net (from vars)
```

---

## Need Help?

- **Step-by-step guide**: [docs/GITHUB_VARIABLES_SETUP.md](docs/GITHUB_VARIABLES_SETUP.md)
- **Implementation checklist**: [TIER_2_SETUP_CHECKLIST.md](TIER_2_SETUP_CHECKLIST.md)
- **Full overview**: [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)

---

## What Changed in Your Workflows?

All 6 workflows now use variables for configuration:

| Workflow | Before | After |
|----------|--------|-------|
| terraform.yml | Hardcoded values | Uses `vars.*` |
| databases.yml | Secrets | Uses `vars.AZURE_SQL_SERVER_FQDN` |
| dotnet_apps.yml | Hardcoded image | Uses `vars.API_IMAGE_NAME` |
| react_apps.yml | Hardcoded image | Uses `vars.REACT_IMAGE_NAME` |
| docs_db.yml | Hardcoded versions | Uses `vars.*VERSION` |
| build-runner-image.yml | Hardcoded registry | Uses `vars.CONTAINER_REGISTRY` |

All variables have **fallback values**, so if you forget one, it still works!

---

## Security Improvements You Get

✅ **terraform.tfvars protected** (no secrets in git)
✅ **OIDC authentication** (no long-lived secrets in cloud)
✅ **Local testing works** (with act tool)
✅ **Better visibility** (non-sensitive data visible in logs)
✅ **Easy updates** (change config in GitHub UI, not code)

---

## Troubleshooting

**Variable not showing in GitHub?**
- Check it's in **Variables** tab (not Secrets)
- Check spelling (case-sensitive)

**Workflow using fallback value?**
- Wait a few seconds (GitHub syncs)
- Verify variable exists and has correct name

**Value format wrong?**
- SQL Server: Must be FQDN (e.g., `server.database.windows.net`)
- ACR: Must be URL (e.g., `registry.azurecr.io`)

---

**Questions?** See the full documentation files listed above.

**Ready?** Start with Step 1 above! ⏱️
