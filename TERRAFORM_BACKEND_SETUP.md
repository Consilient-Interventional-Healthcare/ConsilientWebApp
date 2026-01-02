# Terraform Remote Backend Setup for GitHub Actions

## Overview

This document guides you through setting up a remote Terraform backend for GitHub Actions using Azure Storage with OIDC authentication. Local testing with `act` will continue to use the local backend.

## What Was Changed

### Code Changes (Already Committed)

1. **`infra/terraform/backend.tf`**
   - Enabled Azure Storage backend with OIDC authentication
   - Backend details provided via CLI flags in workflow (not hardcoded)

2. **`.github/workflows/terraform.yml`**
   - Added `TF_STATE_STORAGE_ACCOUNT` and `TF_STATE_CONTAINER` environment variables
   - Updated Terraform Init step with conditional logic:
     - GitHub Actions: Uses Azure Storage backend with OIDC
     - Local (act): Uses local backend via `terraform init -reconfigure -backend=false`
   - Fixed state detection in "Detect and Import Existing Resources" step
     - Changed from file-based checks to `terraform state list` command

3. **`infra/terraform/scripts/import.sh`**
   - Fixed state detection to use `terraform state list` instead of file checks
   - Works reliably with both local and remote backends

### Manual Setup Required

You must manually create the Azure Storage Account for Terraform state. This is a bootstrap problem - Terraform can't create the state storage before state storage exists.

## Manual Setup Steps

### Prerequisites
- Azure CLI installed and authenticated
- Access to the Azure subscription
- Your OIDC federated identity client ID (GitHub secret `AZURE_CLIENT_ID`)

### Step 1: Set Environment Variables

Replace the values with your actual configuration:

```bash
SUBSCRIPTION_ID="<your-subscription-id>"
RESOURCE_GROUP="<your-resource-group-name>"  # Same as TF_VAR_resource_group_name
ENVIRONMENT="dev"  # dev, staging, prod
LOCATION="eastus"  # Same as TF_VAR_region

# Generate unique suffix (matches Terraform's method)
UNIQUE_SUFFIX=$(echo -n "${SUBSCRIPTION_ID}-${RESOURCE_GROUP}" | md5sum | cut -c1-6)

# Storage account name (must be globally unique, 3-24 lowercase letters/numbers)
STORAGE_ACCOUNT_NAME="consilienttfstate${ENVIRONMENT}${UNIQUE_SUFFIX}"
CONTAINER_NAME="tfstate"

echo "Storage Account Name: ${STORAGE_ACCOUNT_NAME}"
```

### Step 2: Create Storage Account (Private)

```bash
# Create the storage account with private access
az storage account create \
  --name "${STORAGE_ACCOUNT_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --location "${LOCATION}" \
  --sku Standard_LRS \
  --kind StorageV2 \
  --min-tls-version TLS1_2 \
  --allow-blob-public-access false \
  --public-network-access Enabled \
  --https-only true \
  --encryption-services blob \
  --tags purpose=terraform-state environment="${ENVIRONMENT}"

echo "✅ Storage account created: ${STORAGE_ACCOUNT_NAME}"
```

**Privacy note:** The storage account is "private" in that:
- No public blob access (--allow-blob-public-access false)
- Access controlled via RBAC, not storage account keys
- TLS 1.2 minimum encryption
- HTTPS only
- Public network access is needed for GitHub Actions runners to connect

### Step 3: Create Blob Container

```bash
az storage container create \
  --name "${CONTAINER_NAME}" \
  --account-name "${STORAGE_ACCOUNT_NAME}" \
  --public-access off \
  --auth-mode login

echo "✅ Container created: ${CONTAINER_NAME}"
```

### Step 4: Enable Versioning and Soft Delete

```bash
# Enable versioning for state file history
az storage account blob-service-properties update \
  --account-name "${STORAGE_ACCOUNT_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --enable-versioning true

# Enable soft delete (7 day retention for recovery)
az storage account blob-service-properties update \
  --account-name "${STORAGE_ACCOUNT_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --enable-delete-retention true \
  --delete-retention-days 7

echo "✅ Versioning and soft delete enabled"
```

### Step 5: Grant RBAC Permissions to OIDC Identity

```bash
# Get the OIDC client ID from your GitHub secret
OIDC_CLIENT_ID="<your-oidc-client-id>"  # From GitHub secret AZURE_CLIENT_ID

# Get the storage account resource ID
STORAGE_ACCOUNT_ID=$(az storage account show \
  --name "${STORAGE_ACCOUNT_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --query id -o tsv)

# Assign "Storage Blob Data Contributor" role to OIDC identity
az role assignment create \
  --assignee "${OIDC_CLIENT_ID}" \
  --role "Storage Blob Data Contributor" \
  --scope "${STORAGE_ACCOUNT_ID}"

echo "✅ RBAC permissions granted to OIDC identity"
```

### Step 6: Add GitHub Repository Variables

Go to your GitHub repository:
1. Settings → Secrets and variables → Actions → Variables
2. Create two new variables:

```
TF_STATE_STORAGE_ACCOUNT = <value of ${STORAGE_ACCOUNT_NAME}>
TF_STATE_CONTAINER = tfstate
```

Example:
```
TF_STATE_STORAGE_ACCOUNT = consilienttfstatdev123abc
TF_STATE_CONTAINER = tfstate
```

### Step 7: Verify Setup

```bash
# Test that you can list blobs in the container
az storage blob list \
  --account-name "${STORAGE_ACCOUNT_NAME}" \
  --container-name "${CONTAINER_NAME}" \
  --auth-mode login

echo "✅ Setup complete!"
echo ""
echo "Summary:"
echo "  Storage Account: ${STORAGE_ACCOUNT_NAME}"
echo "  Container: ${CONTAINER_NAME}"
echo "  Resource Group: ${RESOURCE_GROUP}"
echo ""
echo "Added to GitHub Variables:"
echo "  TF_STATE_STORAGE_ACCOUNT=${STORAGE_ACCOUNT_NAME}"
echo "  TF_STATE_CONTAINER=${CONTAINER_NAME}"
```

## How It Works

### GitHub Actions Workflow

When the workflow runs in GitHub Actions:

1. **Terraform Init Step**
   - Detects GitHub Actions environment (ACT variable not set)
   - Uses `terraform init` with backend-config flags
   - Configures Azure Storage backend with OIDC authentication
   - State file stored at: `${environment}.terraform.tfstate` (e.g., `dev.terraform.tfstate`)

2. **State Detection**
   - Uses `terraform state list` to check if state exists
   - Works with remote backend (queries Azure Storage)
   - If state exists, import only runs if Terraform operations fail

3. **State Persistence**
   - State persists in Azure Storage between workflow runs
   - No need to recreate resources on each run
   - Import script only runs if state drift is detected

### Local Testing (act)

When running locally with `act`:

1. **Terraform Init Step**
   - Detects local environment (ACT variable is set)
   - Uses `terraform init -reconfigure -backend=false`
   - Disables remote backend, uses local state

2. **State Persistence**
   - State file at `infra/terraform/terraform.tfstate` (via `--bind` mount)
   - Persists between local test runs
   - No changes to existing behavior

## Architecture

```
GitHub Actions (Ephemeral Container)
├── terraform init
│   ├─ Detect: ACT variable not set → GitHub Actions
│   ├─ Backend Config via CLI flags
│   └─ Use Azure Storage backend with OIDC
├── terraform plan/apply
│   └─ State queried from Azure Storage
└── Result: State persisted in Azure Storage

Local Development (act)
├── terraform init
│   ├─ Detect: ACT variable set → Local
│   ├─ -reconfigure -backend=false
│   └─ Use local backend
├── terraform plan/apply
│   └─ State in infra/terraform/terraform.tfstate
└── Result: State persisted via --bind mount
```

## State Detection Logic

### Old Logic (File-Based)
```bash
if [ ! -f "terraform.tfstate" ] && [ ! -f ".terraform/terraform.tfstate" ]; then
  # Assumed fresh deployment
fi
```

**Problem:** In GitHub Actions, even when state exists remotely, these local files don't exist, causing false "fresh deployment" detection.

### New Logic (Backend-Aware)
```bash
STATE_RESOURCES=$(terraform state list 2>/dev/null || echo "")

if [ -z "$STATE_RESOURCES" ]; then
  # Truly fresh deployment
fi
```

**Benefits:**
- Works with both local and remote backends
- Queries the actual backend (local file or Azure Storage)
- Accurately detects if state exists regardless of environment
- No false positives for fresh deployments

## Testing

### Test 1: Fresh Deployment (No Existing State)
```bash
# Ensure storage container is empty
az storage blob list \
  --account-name "${STORAGE_ACCOUNT_NAME}" \
  --container-name "${CONTAINER_NAME}"
# Should be empty

# Run workflow
# Expected: Skip import, create all resources, save state to Azure
```

### Test 2: Existing Deployment (State Exists)
```bash
# Verify state file exists
az storage blob list \
  --account-name "${STORAGE_ACCOUNT_NAME}" \
  --container-name "${CONTAINER_NAME}" \
  --query "[].name"
# Should show: dev.terraform.tfstate (or appropriate environment)

# Run workflow
# Expected: Detect state, import if needed, apply changes
```

### Test 3: Local Testing with act
```bash
# Run act locally
act workflow_dispatch -i ghcr.io/consilient-interventional-healthcare/consilient-web-app-runner:latest \
  --pull=false \
  --bind \
  --action-offline-mode

# Expected: Use local backend, state persists via --bind
# Same behavior as before
```

## Troubleshooting

### Backend Configuration Error
**Error:** `Error: Invalid backend configuration`

**Solution:** Ensure GitHub variables are set:
```bash
# Check variables in GitHub repository settings
# Settings → Secrets and variables → Actions → Variables
# Should have TF_STATE_STORAGE_ACCOUNT and TF_STATE_CONTAINER
```

### OIDC Authentication Failed
**Error:** `AADSTS65001: The user or admin has not consented...`

**Solution:** Verify RBAC role assignment:
```bash
# Check role assignment
az role assignment list \
  --assignee "${OIDC_CLIENT_ID}" \
  --scope "${STORAGE_ACCOUNT_ID}"

# Should show: Storage Blob Data Contributor role
```

### State File Not Found in First Run
**Error:** Import script shows "No Terraform state found" but workflow continues

**This is expected behavior:**
- First run: No existing state, so import is skipped
- Resources are created from scratch
- State saved to Azure Storage
- Subsequent runs: Import only runs if needed

### Local Testing Fails
**Error:** `terraform state list` fails locally

**Solution:** Verify local state file exists:
```bash
ls -la infra/terraform/terraform.tfstate
# Should show the state file

# Or re-initialize with local backend
cd infra/terraform
terraform init -reconfigure -backend=false
```

## Security Considerations

### OIDC Authentication
- Uses federated credentials instead of service principal secrets
- No secrets stored in environment variables for backend access
- Tokens are short-lived and specific to the workflow

### Storage Account Security
- No public blob access (--allow-blob-public-access false)
- Access controlled via RBAC
- TLS 1.2 minimum
- HTTPS only

### State File Contents
- Contains sensitive data (passwords, API keys, connection strings)
- Versioning and soft delete enabled for recovery
- Access limited to OIDC identity (GitHub Actions) and authenticated users

## Migration from Local to Remote State

The first time the workflow runs with these changes:

1. Terraform Init will initialize remote backend
2. State detection will find no existing remote state
3. Resources will be created from scratch
4. State saved to Azure Storage
5. Subsequent runs use the remote state

**Note:** If you have existing resources in Azure from previous deployments with local state, run the import script manually to bring them under Terraform management:

```bash
cd infra/terraform
terraform init -backend-config="..." # With backend-config flags
bash scripts/import.sh
```

## References

- [Terraform Azure Provider Documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/guide-oidc)
- [Azure OIDC Federated Credentials](https://learn.microsoft.com/en-us/azure/active-directory/workload-identities/workload-identity-federation)
- [Terraform Remote State](https://www.terraform.io/language/state/remote)

## Support

For issues or questions:
1. Check the plan file at: `.claude/plans/virtual-bubbling-pond.md`
2. Review GitHub Actions workflow logs for detailed error messages
3. Verify GitHub variables are correctly configured
4. Ensure Azure CLI has proper permissions
