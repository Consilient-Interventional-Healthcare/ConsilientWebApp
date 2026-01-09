#!/bin/bash

# Terraform Initialization and Format Check Script
# Consolidates multiple initialization steps into a single script:
# 1. Format Check - Validates Terraform code formatting
# 2. Firewall Configuration - Overrides local firewall setting based on ACT detection
# 3. Cache Cleanup - Removes provider cache to avoid checksum mismatches
# 4. Terraform Init - Initializes Terraform with environment-specific backend
#
# This script is designed to be called from GitHub Actions workflows and supports
# both GitHub Actions (with Azure Storage backend) and ACT (local testing with local backend).
#
# Exit codes:
#   0 - All operations succeeded
#   1 - Terraform init failed (format check warnings do not cause failure)
#
# Dependencies:
#   - terraform CLI
#   - bash 4+

set +e  # Don't exit on error, handle errors explicitly

# ============================================================================
# DEBUG OUTPUT CONTROL
# ============================================================================
# Detect if running in GitHub Actions and force verbose output
IN_GITHUB_ACTIONS=false
if [ -n "$GITHUB_ACTIONS" ] || [ -n "$GITHUB_WORKSPACE" ]; then
  IN_GITHUB_ACTIONS=true
  # In GitHub Actions, always show informational output
  [ -z "${ACTIONS_STEP_DEBUG}" ] && ACTIONS_STEP_DEBUG="true"
fi

# Helper function for debug output
debug_echo() {
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "$1"
}

# ============================================================================
# PHASE 1: TERRAFORM FORMAT CHECK (WARNING ONLY - ALL ENVIRONMENTS)
# ============================================================================
debug_echo ""
debug_echo "=== Phase 1: Terraform Format Check ==="
debug_echo ""

# Store current directory and change to root for format check
INITIAL_DIR=$(pwd)
cd "${GITHUB_WORKSPACE}" || exit 1

# Run format check on all Terraform files
FORMAT_CHECK_OUTPUT=$(terraform fmt -check -recursive 2>&1)
FORMAT_CHECK_EXIT=$?

if [ $FORMAT_CHECK_EXIT -eq 0 ]; then
  echo "✅ Terraform formatting check passed"
  debug_echo "All Terraform files are properly formatted"
else
  echo "⚠️  Terraform formatting issues detected"
  echo "   Recommendation: Run 'terraform fmt -recursive' in your infra/terraform directory locally to clean up the code files."
  debug_echo ""
  debug_echo "Formatting issues found:"
  debug_echo "$FORMAT_CHECK_OUTPUT"
  # NOTE: We don't exit on format errors - warnings only
fi

# Return to initial directory
cd "$INITIAL_DIR" || exit 1

# ============================================================================
# PHASE 2: CONFIGURE LOCAL FIREWALL VARIABLE
# ============================================================================
debug_echo ""
debug_echo "=== Phase 2: Configure Local Firewall Variable ==="
debug_echo ""

# ACT is set by the act tool when running locally
# In GitHub Actions, ACT is empty
if [ -n "$ACT" ]; then
  # Running in ACT (local testing environment)
  if [ "$TF_VAR_enable_local_firewall" = "true" ] || [ "$TF_VAR_enable_local_firewall" = "True" ]; then
    # User explicitly requested local firewall AND we're in ACT
    debug_echo "ℹ️  ACT environment detected"
    echo "✅ Local firewall rule ENABLED (act detected)"
    export TF_VAR_enable_local_firewall=true
  else
    # User didn't request it or we're in GitHub Actions
    debug_echo "ℹ️  Local firewall rule disabled (not requested or not in ACT)"
    export TF_VAR_enable_local_firewall=false
  fi
else
  # Running in GitHub Actions - always disable local firewall
  debug_echo "ℹ️  GitHub Actions environment detected"
  echo "ℹ️  Local firewall rule disabled"
  export TF_VAR_enable_local_firewall=false
fi

# ============================================================================
# PHASE 3: CLEAR TERRAFORM PROVIDER CACHE
# ============================================================================
debug_echo ""
debug_echo "=== Phase 3: Clear Terraform Provider Cache ==="
debug_echo ""

echo "ℹ️  Clearing Terraform provider cache..."
if rm -rf .terraform/providers; then
  echo "✅ Provider cache cleared"
  debug_echo "Removed .terraform/providers directory"
else
  echo "⚠️  Failed to clear provider cache (directory may not exist)"
fi

# ============================================================================
# PHASE 4: TERRAFORM INIT
# ============================================================================
debug_echo ""
debug_echo "=== Phase 4: Terraform Init ==="
debug_echo ""

# Disable Terraform plugin cache to avoid checksum mismatches with pre-cached providers
unset TF_PLUGIN_CACHE_DIR

# Determine backend configuration based on environment
if [ -z "$ACT" ]; then
  # ========================================================================
  # GitHub Actions: Use Azure Storage backend with OIDC
  # ========================================================================
  echo "ℹ️  Initializing with Azure Storage backend (OIDC authentication)"
  debug_echo ""
  debug_echo "Backend configuration:"
  debug_echo "  Resource Group: ${TF_STATE_RESOURCE_GROUP}"
  debug_echo "  Storage Account: ${TF_STATE_STORAGE_ACCOUNT}"
  debug_echo "  Container: ${TF_STATE_CONTAINER}"
  debug_echo "  State File: ${TF_VAR_environment}.terraform.tfstate"
  debug_echo ""

  # Run terraform init with Azure Storage backend configuration
  if terraform init -upgrade=false \
    -backend-config="resource_group_name=${TF_STATE_RESOURCE_GROUP}" \
    -backend-config="storage_account_name=${TF_STATE_STORAGE_ACCOUNT}" \
    -backend-config="container_name=${TF_STATE_CONTAINER}" \
    -backend-config="key=${TF_VAR_environment}.terraform.tfstate" \
    -backend-config="subscription_id=${AZURE_SUBSCRIPTION_ID}" \
    -backend-config="tenant_id=${ARM_TENANT_ID}" \
    -backend-config="use_oidc=true"; then
    echo "✅ Terraform init succeeded"
    debug_echo ""
    debug_echo "✅ Azure Storage backend initialized successfully"
    exit 0
  else
    echo "❌ Terraform init failed"
    exit 1
  fi
else
  # ========================================================================
  # Local (ACT): Use local backend without remote state
  # ========================================================================
  echo "ℹ️  Initializing with local backend (act testing)"
  debug_echo ""
  debug_echo "ACT environment detected - using local backend configuration"
  debug_echo ""

  # Run terraform init with local backend (no remote state)
  if terraform init -reconfigure -backend=false; then
    echo "✅ Terraform init succeeded"
    debug_echo ""
    debug_echo "✅ Local backend initialized successfully"
    exit 0
  else
    echo "❌ Terraform init failed"
    exit 1
  fi
fi
