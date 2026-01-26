#!/bin/bash
set +e  # Don't exit on error, we'll handle errors ourselves

# Import script for Terraform Azure resources using aztfexport
# This script uses Azure's official aztfexport tool to automatically discover
# and import existing Azure resources into Terraform state.
#
# aztfexport: https://github.com/Azure/aztfexport
# Documentation: https://learn.microsoft.com/en-us/azure/developer/terraform/azure-export-for-terraform/export-terraform-overview
#
# Advantages over manual import.sh:
# - Automatically discovers all resources in the resource group
# - No need to maintain hardcoded resource IDs or terraform addresses
# - Handles resource dependencies automatically
# - Maps Azure resource IDs to correct Terraform resource types via aztft
# - Generates valid HCL configuration that matches actual infrastructure
# - Maintained by Azure team
#
# Usage:
#   ./import-aztfexport.sh
#
# Required environment variables:
#   TF_VAR_subscription_id      - Azure subscription ID
#   TF_VAR_resource_group_name  - Resource group to import from
#
# Optional environment variables:
#   AZTFEXPORT_APPEND           - Set to "true" to append to existing state (default: true)
#   AZTFEXPORT_NON_INTERACTIVE  - Set to "true" for non-interactive mode (default: true in CI)
#   AZTFEXPORT_HCL_ONLY         - Set to "true" to generate HCL only, no state import (default: false)
#   AZTFEXPORT_PROVIDER         - Provider to use: "azurerm" or "azapi" (default: azurerm)
#
# Key aztfexport flags used:
#   --append              Import into existing Terraform state without overwriting
#   --non-interactive     Run without user prompts (required for CI/CD)
#   --continue            Continue on errors (some resources may not be importable)
#   --plain-ui            Use plain text output (for environments without /dev/tty)
#   --generate-mapping-file  Save resource mapping for debugging/review

# GitHub Actions detection
IN_GITHUB_ACTIONS=false
if [ -n "$GITHUB_ACTIONS" ] || [ -n "$GITHUB_WORKSPACE" ]; then
  IN_GITHUB_ACTIONS=true
  ACTIONS_STEP_DEBUG=true
fi

log() {
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "$@"
}

log_always() {
  echo "$@"
}

# Verify required environment variables
if [ -z "$TF_VAR_subscription_id" ]; then
  log_always "ERROR: TF_VAR_subscription_id is required"
  exit 1
fi

if [ -z "$TF_VAR_resource_group_name" ]; then
  log_always "ERROR: TF_VAR_resource_group_name is required"
  exit 1
fi

# Check if aztfexport is installed
if ! command -v aztfexport &> /dev/null; then
  log_always "aztfexport not found. Installing..."

  if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    # Linux installation
    if command -v apt-get &> /dev/null; then
      # Debian/Ubuntu
      curl -sSL https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
      sudo apt-add-repository "https://packages.microsoft.com/repos/azure-cli/"
      sudo apt-get update && sudo apt-get install -y aztfexport
    elif command -v dnf &> /dev/null; then
      # RHEL/Fedora
      sudo dnf install -y aztfexport
    else
      # Fallback: install via Go
      if command -v go &> /dev/null; then
        go install github.com/Azure/aztfexport@latest
      else
        log_always "ERROR: Cannot install aztfexport. Please install manually:"
        log_always "  https://github.com/Azure/aztfexport#installation"
        exit 1
      fi
    fi
  elif [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    if command -v brew &> /dev/null; then
      brew install aztfexport
    else
      log_always "ERROR: Homebrew not found. Install aztfexport manually:"
      log_always "  brew install aztfexport"
      exit 1
    fi
  elif [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "win32" ]]; then
    # Windows (Git Bash or similar)
    if command -v winget &> /dev/null; then
      winget install aztfexport
    else
      log_always "ERROR: Please install aztfexport manually using winget or MSI:"
      log_always "  winget install aztfexport"
      log_always "  Or download from: https://github.com/Azure/aztfexport/releases"
      exit 1
    fi
  else
    log_always "ERROR: Unsupported OS. Please install aztfexport manually."
    exit 1
  fi
fi

# Verify aztfexport is now available
if ! command -v aztfexport &> /dev/null; then
  log_always "ERROR: aztfexport installation failed"
  exit 1
fi

log "aztfexport version: $(aztfexport --version 2>/dev/null || echo 'unknown')"

# Check if Terraform state exists
STATE_RESOURCES=$(terraform state list 2>/dev/null || echo "")

if [ -z "$STATE_RESOURCES" ]; then
  log "No Terraform state found - this appears to be a fresh deployment"
  log "Skipping resource import step (will create all resources from scratch)"
  exit 0
fi

log "Found existing state with $(echo "$STATE_RESOURCES" | wc -l) resources"

# Check if resource group exists in Azure
RG_EXISTS=$(az group exists --name "${TF_VAR_resource_group_name}" 2>/dev/null || echo "false")
if [ "$RG_EXISTS" != "true" ]; then
  log "Resource group '${TF_VAR_resource_group_name}' does not exist in Azure"
  log "Skipping import - Terraform will create resources"
  exit 0
fi

log "=== Importing Azure Resources using aztfexport ==="
log ""

# Set defaults for aztfexport options
APPEND_MODE="${AZTFEXPORT_APPEND:-true}"
NON_INTERACTIVE="${AZTFEXPORT_NON_INTERACTIVE:-$IN_GITHUB_ACTIONS}"
HCL_ONLY="${AZTFEXPORT_HCL_ONLY:-false}"
PROVIDER="${AZTFEXPORT_PROVIDER:-azurerm}"

# Build aztfexport command arguments
# Command format: aztfexport resource-group [options] <resource-group-name>
AZTFEXPORT_ARGS=(
  "resource-group"
)

# Subscription selection
AZTFEXPORT_ARGS+=("--subscription=${TF_VAR_subscription_id}")

# Append mode: import into existing state without overwriting
# This verifies existing provider/terraform blocks and imports to existing state
if [ "$APPEND_MODE" = "true" ]; then
  AZTFEXPORT_ARGS+=("--append")
  log "Using append mode - will import into existing Terraform state"
  log "  Generated files will have .aztfexport suffix to avoid conflicts"
fi

# Non-interactive mode for CI/CD
if [ "$NON_INTERACTIVE" = "true" ]; then
  AZTFEXPORT_ARGS+=("--non-interactive")
  # Use plain-ui for environments without /dev/tty (like GitHub Actions runners)
  AZTFEXPORT_ARGS+=("--plain-ui")
  log "Running in non-interactive mode with plain UI"
fi

# Provider selection (azurerm or azapi)
if [ "$PROVIDER" = "azapi" ]; then
  AZTFEXPORT_ARGS+=("--provider-name=azapi")
  log "Using AzAPI provider"
else
  log "Using AzureRM provider (default)"
fi

# Generate mapping file for review (useful for debugging)
MAPPING_FILE="aztfexport-mapping.json"
AZTFEXPORT_ARGS+=("--generate-mapping-file=${MAPPING_FILE}")

# HCL only mode: generate config files without importing to state
if [ "$HCL_ONLY" = "true" ]; then
  AZTFEXPORT_ARGS+=("--hcl-only")
  log "HCL-only mode - generating configuration without state import"
fi

# Continue on errors (some resources may not be importable)
# This is important for partial imports where some resource types aren't supported
AZTFEXPORT_ARGS+=("--continue")

# Resource group name must be the last argument
AZTFEXPORT_ARGS+=("${TF_VAR_resource_group_name}")

log ""
log "Running: aztfexport ${AZTFEXPORT_ARGS[*]}"
log ""

# Execute aztfexport
EXPORT_OUTPUT=$(aztfexport "${AZTFEXPORT_ARGS[@]}" 2>&1)
EXPORT_EXIT_CODE=$?

log "$EXPORT_OUTPUT"

# Check for mapping file
if [ -f "$MAPPING_FILE" ]; then
  log ""
  log "Resource mapping saved to: ${MAPPING_FILE}"

  # Count resources in mapping
  if command -v jq &> /dev/null; then
    MAPPED_COUNT=$(jq 'length' "$MAPPING_FILE" 2>/dev/null || echo "0")
    log "Discovered ${MAPPED_COUNT} resources for import"
  fi
fi

# Analyze results
if [ $EXPORT_EXIT_CODE -eq 0 ]; then
  log ""
  log "=== aztfexport completed successfully ==="

  # Show final state count
  FINAL_STATE_COUNT=$(terraform state list 2>/dev/null | wc -l)
  log "Terraform state now contains ${FINAL_STATE_COUNT} resources"

  # Generate GitHub step summary
  if [ -n "$GITHUB_STEP_SUMMARY" ]; then
    echo "### aztfexport Import Results" >> $GITHUB_STEP_SUMMARY
    echo "" >> $GITHUB_STEP_SUMMARY
    echo "- **Resource Group**: \`${TF_VAR_resource_group_name}\`" >> $GITHUB_STEP_SUMMARY
    echo "- **Resources in state**: ${FINAL_STATE_COUNT}" >> $GITHUB_STEP_SUMMARY
    echo "- **Status**: Success" >> $GITHUB_STEP_SUMMARY
    echo "" >> $GITHUB_STEP_SUMMARY
  fi

  exit 0
else
  log_always ""
  log_always "=== aztfexport completed with warnings/errors ==="
  log_always "Exit code: ${EXPORT_EXIT_CODE}"

  # aztfexport may return non-zero for partial imports
  # Check if we actually imported anything
  FINAL_STATE_COUNT=$(terraform state list 2>/dev/null | wc -l)
  INITIAL_STATE_COUNT=$(echo "$STATE_RESOURCES" | wc -l)

  if [ "$FINAL_STATE_COUNT" -gt "$INITIAL_STATE_COUNT" ]; then
    IMPORTED_COUNT=$((FINAL_STATE_COUNT - INITIAL_STATE_COUNT))
    log_always "Partial import successful: ${IMPORTED_COUNT} new resources imported"

    if [ -n "$GITHUB_STEP_SUMMARY" ]; then
      echo "### aztfexport Import Results" >> $GITHUB_STEP_SUMMARY
      echo "" >> $GITHUB_STEP_SUMMARY
      echo "- **Resource Group**: \`${TF_VAR_resource_group_name}\`" >> $GITHUB_STEP_SUMMARY
      echo "- **New resources imported**: ${IMPORTED_COUNT}" >> $GITHUB_STEP_SUMMARY
      echo "- **Total resources in state**: ${FINAL_STATE_COUNT}" >> $GITHUB_STEP_SUMMARY
      echo "- **Status**: Partial success (some resources may not be supported)" >> $GITHUB_STEP_SUMMARY
      echo "" >> $GITHUB_STEP_SUMMARY
    fi

    # Partial import is acceptable - proceed with terraform plan/apply
    exit 0
  else
    log_always "No new resources were imported"

    if [ -n "$GITHUB_STEP_SUMMARY" ]; then
      echo "### aztfexport Import Results" >> $GITHUB_STEP_SUMMARY
      echo "" >> $GITHUB_STEP_SUMMARY
      echo "- **Resource Group**: \`${TF_VAR_resource_group_name}\`" >> $GITHUB_STEP_SUMMARY
      echo "- **Status**: No changes (resources may already be in state)" >> $GITHUB_STEP_SUMMARY
      echo "" >> $GITHUB_STEP_SUMMARY
    fi

    # This might be okay if everything is already imported
    exit 0
  fi
fi

# =============================================================================
# Advanced Usage: Two-Step Import with Mapping File
# =============================================================================
#
# For more control over the import process, you can use a two-step workflow:
#
# Step 1: Generate mapping file only (for review)
#   AZTFEXPORT_HCL_ONLY=true ./import-aztfexport.sh
#   # Review aztfexport-mapping.json to see what will be imported
#
# Step 2: Import using the mapping file
#   aztfexport map --non-interactive --append ./aztfexport-mapping.json
#
# This allows you to:
# - Review resources before importing
# - Edit the mapping file to exclude certain resources
# - Customize terraform resource names in the mapping
#
# =============================================================================
# Comparison with manual import.sh
# =============================================================================
#
# Manual import.sh:
#   - Requires maintaining ~500 lines of bash with hardcoded resource IDs
#   - Must be updated when adding/removing infrastructure
#   - Resource naming logic must match terraform locals.tf
#   - Explicit control over each resource import
#
# aztfexport (this script):
#   - Automatic discovery of all resources in resource group
#   - No maintenance required when infrastructure changes
#   - Uses Azure's aztft library for accurate resource mapping
#   - May import resources not in your Terraform config (review mapping)
#
# Recommendation: Use aztfexport for initial imports and when infrastructure
# changes significantly. Use manual import.sh when you need precise control
# over exactly which resources are imported.
# =============================================================================
