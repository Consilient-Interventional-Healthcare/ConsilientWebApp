# Local Testing with Act

Complete guide to testing GitHub Actions workflows locally using the `act` tool.

## Quick Start

```powershell
cd infra/act
.\run-act.ps1
```

Follow prompts or use parameters. See [setup guide](#setup-guide) for details.

## What is Act?

`act` is an open-source tool that runs GitHub Actions workflows locally in Docker containers.

**Benefits:**
- Test workflows before pushing to GitHub
- Save GitHub Actions minutes ($)
- Rapid iteration and debugging
- Validate configuration changes

**Limitations:**
- Docker-in-Docker may not work
- Network limited to host only
- Some GitHub features not supported (see [limitations](#limitations))

**Resources:** [nektos/act GitHub](https://github.com/nektos/act)

## Prerequisites

1. **Docker Desktop** (Windows/Mac) or Docker Engine (Linux)
   - Verify: `docker ps`

2. **act CLI**
   - Windows: `choco install act-cli` or `scoop install act`
   - macOS: `brew install act`
   - Verify: `act --version`

3. **Credentials**
   - Azure service principal (for auth)
   - Stored in `infra/act/.env.act`

## Setup Guide

### Step 1: Configure Secrets

**File:** [`infra/act/.env.act`](../../../infra/act/.env.act)

Copy template and fill with Azure credentials:

```bash
# Service principal JSON
AZURE_CREDENTIALS={...}

# Terraform authentication
ARM_CLIENT_ID=...
ARM_CLIENT_SECRET=...
ARM_TENANT_ID=...
AZURE_SUBSCRIPTION_ID=...

# SQL Server
SQL_ADMIN_PASSWORD=...
AZURE_SQL_SERVER=...
```

**How to get values:**
- See [reference/secrets-checklist.md](../reference/secrets-checklist.md)
- Or run: `az account show`

### Step 2: Configure Environment Variables

**File:** [`infra/act/.env`](../../../infra/act/.env)

Non-sensitive configuration (default usually works):
```bash
AZURE_REGION=canadacentral
AZURE_RESOURCE_GROUP_NAME=consilient-resource-group
ACR_REGISTRY=your-registry.azurecr.io
```

### Step 3: Script Configuration

The `run-act.ps1` script handles all act CLI configuration automatically:
- **Custom image** - Uses local `consilientwebapp-runner:latest`
- **Bind mode** - Enables `--bind` for state persistence (5x faster)
- **Pull behavior** - Uses `--pull=false` to use local image only

No additional configuration files needed - the script manages everything.

## Dotfile Dependencies & Configuration

### File Inventory

| File | Purpose | Required | Notes |
|------|---------|----------|-------|
| `.env` | Repository variables (non-sensitive) | ✅ YES | Contains configuration like regions, image names, versions |
| `.env.act` | Secrets & credentials | ✅ YES | Loaded by `run-act.ps1` with `--secret-file` flag; **add to .gitignore** |
| `run-act.ps1` | Main orchestrator script | ✅ YES | Handles Docker image build, act execution, parameter validation |

**Removed Files (No Longer Needed):**
- ~~`.actrc`~~ - Script provides all configuration explicitly via command-line flags
- ~~`.secrets`~~ - Superseded by `.env.act` which contains all credentials

### Configuration Ownership

The **`run-act.ps1` script owns all act configuration**:

```powershell
# Script explicitly sets all act flags:
$ActArgs = @(
    "--pull=false",        # Use local image, don't download
    "--bind",              # Mount workspace for state persistence
    "-P", "ubuntu-latest=consilientwebapp-runner:latest",  # Custom runner image
    "--secret-file", $ActSecretFile  # Load secrets from .env.act
)
```

This means:
- ✅ No separate `.actrc` configuration needed
- ✅ All settings are version-controlled in the script
- ✅ Parameters can be modified without editing multiple files
- ✅ Single source of truth for how act is configured

### Secrets Management

The **`.env.act` file is the single source of truth for all secrets**:

```bash
# .env.act contains:
AZURE_CREDENTIALS={...full JSON...}
ARM_CLIENT_ID=...
ARM_CLIENT_SECRET=...
AZURE_CLIENT_ID=...
SQL_ADMIN_PASSWORD=...
ACR_CICD_CLIENT_ID=...
ACR_CICD_CLIENT_SECRET=...
GITHUB_TOKEN=...
```

**Important:** Add `.env.act` to `.gitignore` to prevent committing credentials.

### How Configuration Works

1. **Script reads `.env.act`:**
   ```powershell
   if (Test-Path $ActSecretFile) {
       $ActArgs += "--secret-file"
       $ActArgs += $ActSecretFile
   }
   ```

2. **Act loads secrets:**
   ```bash
   act ... --secret-file infra/act/.env.act
   ```

3. **Secrets available in workflow:**
   ```yaml
   - name: Login to Azure
     run: az login --service-principal ...
     env:
       AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
   ```

---

## Using run-act.ps1 Script

**Main script:** [`infra/act/run-act.ps1`](../../../infra/act/run-act.ps1)

### Interactive Mode (Recommended)

```powershell
cd infra/act
.\run-act.ps1
```

Prompts for:
- Environment (dev/prod)
- Skip terraform? (dev: yes, test faster)
- Skip databases? (skip if not needed)
- Recreate database? (dev only)
- Local firewall? (yes for SQL access)
- Health checks? (yes to verify)
- Debug mode? (yes for troubleshooting)

### Parameterized Mode

```powershell
.\run-act.ps1 -Environment dev -SkipTerraform -SkipDatabases
```

**Available Parameters:**
- `-Environment` (dev/prod)
- `-SkipTerraform` - Skip infra provisioning
- `-SkipDatabases` - Skip database deployment
- `-RecreateDatabase` - Recreate all DB objects (dev only)
- `-AllowLocalFirewall` - Enable SQL firewall for local testing
- `-EnableDebugMode` - Verbose GitHub Actions logging
- `-SkipHealthChecks` - Skip post-deploy health verification
- `-NonInteractive` - No prompts (for automation)
- `-NoWait` - Don't wait for keypress at end
- `-RebuildImage` - Force rebuild Docker image

### Common Scenarios

**Test database deployment only:**
```powershell
.\run-act.ps1 -Environment dev -SkipTerraform
```

**Full infrastructure test:**
```powershell
.\run-act.ps1 -Environment dev
```

**Debug workflow issues:**
```powershell
.\run-act.ps1 -Environment dev -EnableDebugMode -SkipHealthChecks
```

**Rebuild after Dockerfile changes:**
```powershell
.\run-act.ps1 -RebuildImage
```

## Custom Runner Image

**Location:** [`.github/workflows/runner/Dockerfile`](../../../.github/workflows/runner/Dockerfile)

**Pre-Installed Tools:**
- Azure CLI (latest)
- sqlcmd v1.6.0
- Terraform v1.9.0 (+ pre-cached providers)
- Java 17 JRE
- SchemaSpy v6.2.4
- Node.js 20, Docker, git, jq

**Why Custom Image:**
- All required tools pre-installed
- Faster workflow execution
- Terraform providers cached (no download delay)
- Consistent environment

**Build Time:**
- First time: ~5-10 minutes (Docker build)
- Subsequent: ~1 minute (use cache)

**Build Command:**
```powershell
.\run-act.ps1 -RebuildImage
```

## Performance Optimization

### State Persistence with `--bind`

**Problem (Before Fix):**
- Without `--bind`, Docker copies workspace (ephemeral)
- Each run appears as fresh deployment
- Terraform re-imports all resources every time
- ~5 minutes per run (import overhead)

**Solution:**
- `--bind` flag mounts workspace (persistent)
- State persists between runs
- Subsequent runs skip imports
- ~1 minute per run (5x faster!)

**Verification:**
```powershell
# Check --bind in .actrc
cat infra/act/.actrc | findstr bind
# Should show: --bind
```

**Details:** [infra/act/IMPORT_LOOP_FIX.md](../../../infra/act/IMPORT_LOOP_FIX.md)

### Other Performance Tips

1. **Skip unnecessary steps:**
   ```powershell
   .\run-act.ps1 -SkipTerraform -SkipDatabases  # Fastest
   ```

2. **Use cached image (don't rebuild):**
   ```powershell
   # First run builds image, subsequent runs use cache
   # Only rebuild if Dockerfile changes
   ```

3. **Allocate Docker resources:**
   - Give Docker Desktop 4+ CPUs
   - 8GB+ RAM for faster builds

## Authentication Flow

**When act is detected** (ACT environment variable set):

1. **Composite Action:** `azure-login`
2. **Behavior:** Uses service principal fallback
3. **Implementation:** [`azure-login/action.yml:42-58`](../../../.github/actions/azure-login/action.yml#L42-L58)

**Process:**
1. Read `AZURE_CREDENTIALS` JSON from `.env.act`
2. Parse clientId, clientSecret, tenantId
3. Execute: `az login --service-principal`
4. Set Azure context for subsequent steps

See [components/authentication.md](authentication.md) for auth details.

## Troubleshooting

**Docker not running:**
- Start Docker Desktop
- Wait 1-2 minutes for startup
- Verify: `docker ps`

**act CLI not found:**
- Install: `choco install act-cli`
- Verify: `act --version`
- Reopen PowerShell if just installed

**Secret file missing:**
```powershell
cd infra/act
# Check if .env.act exists
ls -la .env.act
# If missing, copy template and edit
copy .env.act.example .env.act
```

**State persistence issues:**
- Verify `--bind` in `.actrc`
- Check: `ls -la infra/terraform/terraform.tfstate*`
- Should see `.tfstate` files

**Performance slow:**
- Verify `--bind` flag (state persistence)
- Check Docker resource allocation
- Use `-SkipTerraform -SkipDatabases` for faster testing

See [TROUBLESHOOTING.md#local-testing-act](../TROUBLESHOOTING.md#local-testing-act) for more issues.

## Limitations & Differences

**Docker-in-Docker:**
- Nested Docker containers may not work
- Some deployment scenarios may fail locally

**Network Access:**
- Limited to host network
- External APIs may be unreachable
- VPN may be required

**Path Handling:**
- Windows paths converted to Linux (/workspace)
- Some path-dependent tests may fail

**GitHub Features:**
- Some GitHub Actions-specific features not supported
- Secrets/variables loaded from files, not GitHub UI

**Workaround:**
- Always validate in GitHub Actions cloud
- Use act for quick iterations
- Final test in cloud before production

## Files Reference

- [infra/act/run-act.ps1](../../../infra/act/run-act.ps1) - Main orchestrator script
- [infra/act/.env](../../../infra/act/.env) - Repository variables
- [infra/act/.env.act](../../../infra/act/.env.act) - Secrets & credentials (in .gitignore)
- [.github/workflows/runner/Dockerfile](../../../.github/workflows/runner/Dockerfile) - Custom runner image

## Related Documentation

- [components/authentication.md](authentication.md) - Auth details
- [QUICK_START.md#3-test-workflows-locally-with-act](../QUICK_START.md#3-test-workflows-locally-with-act) - Quick start
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System design
- [TROUBLESHOOTING.md#local-testing-act](../TROUBLESHOOTING.md#local-testing-act) - Troubleshooting

---

**Last Updated:** December 2025
**For Navigation:** See [README.md](../README.md)
