#!/bin/bash

# Hostname Pre-Check Script
# Proactively checks hostname availability via DNS and determines optimal naming tier
# before Terraform runs, avoiding apply failures due to global namespace conflicts.
#
# Output: Prints "naming_tier=N" where N is 0 (standard), 1 (region), or 2 (random)
#
# Dependencies:
#   - Azure CLI (az command)
#   - curl or nslookup/host for DNS checks
#   - Bash 4+

set +e  # Don't exit on error, we'll handle errors ourselves

# Naming tier constants
TIER_STANDARD=0
TIER_REGION=1
TIER_RANDOM=2

# Verbose output control
IN_GITHUB_ACTIONS=false
if [ -n "$GITHUB_ACTIONS" ] || [ -n "$GITHUB_WORKSPACE" ]; then
  IN_GITHUB_ACTIONS=true
  ACTIONS_STEP_DEBUG=true  # Force verbose output in GitHub Actions
fi

# Helper: Print debug messages
debug_echo() {
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  $1" >&2
}

# Helper: Check if Azure resource exists in our subscription
# Returns 0 (success) if exists, 1 if not found
check_azure_resource_exists() {
  local resource_name="$1"
  local resource_group="$2"

  if az webapp show --name "${resource_name}" --resource-group "${resource_group}" &>/dev/null 2>&1; then
    return 0
  else
    return 1
  fi
}

# Helper: Check if hostname is available via DNS
# Returns 0 if hostname resolves (taken), 1 if doesn't resolve (available)
check_hostname_via_dns() {
  local app_name="$1"
  local hostname="${app_name}.azurewebsites.net"

  # Try nslookup first, then host command as fallback
  if nslookup "${hostname}" &>/dev/null 2>&1; then
    return 0  # Hostname resolves = taken
  elif host "${hostname}" &>/dev/null 2>&1; then
    return 0  # Hostname resolves = taken
  else
    return 1  # Hostname doesn't resolve = available
  fi
}

# Generate app name for a given tier
# Args: tier, project_name, app_type, environment, region, subscription_id, resource_group_name
generate_app_name() {
  local tier="$1"
  local project_name="$2"
  local app_type="$3"
  local environment="$4"
  local region="$5"
  local subscription_id="$6"
  local resource_group="$7"

  case "$tier" in
    "$TIER_STANDARD")
      echo "${project_name}-${app_type}-${environment}"
      ;;
    "$TIER_REGION")
      # Normalize region: lowercase, remove spaces
      local region_suffix=$(echo "${region}" | tr '[:upper:]' '[:lower:]' | tr -d ' ')
      echo "${project_name}-${app_type}-${environment}-${region_suffix}"
      ;;
    "$TIER_RANDOM")
      # Deterministic 4-character suffix using MD5 (different offset than unique_suffix)
      local hash=$(echo -n "${subscription_id}-${resource_group}-${app_type}-${environment}" | md5sum | cut -c7-10)
      echo "${project_name}-${app_type}-${environment}-${hash}"
      ;;
    *)
      echo "ERROR: Invalid tier $tier" >&2
      return 1
      ;;
  esac
}

# Check if a naming tier is available
# Both API and React app names must be available for the tier to pass
# Args: tier, project_name, environment, region, subscription_id, resource_group_name
check_tier_availability() {
  local tier="$1"
  local project_name="$2"
  local environment="$3"
  local region="$4"
  local subscription_id="$5"
  local resource_group="$6"

  debug_echo "=== Checking Tier $tier Availability ==="

  # Generate app names for this tier
  local api_name=$(generate_app_name "$tier" "$project_name" "api" "$environment" "$region" "$subscription_id" "$resource_group")
  local react_name=$(generate_app_name "$tier" "$project_name" "react" "$environment" "$region" "$subscription_id" "$resource_group")

  debug_echo "API name: $api_name"
  debug_echo "React name: $react_name"

  # Check if apps already exist in our subscription (preserve existing names)
  local api_exists=1
  local react_exists=1

  if check_azure_resource_exists "$api_name" "$resource_group"; then
    api_exists=0
    debug_echo "API app exists in subscription (will preserve)"
  fi

  if check_azure_resource_exists "$react_name" "$resource_group"; then
    react_exists=0
    debug_echo "React app exists in subscription (will preserve)"
  fi

  # If either app exists in our subscription, we should use this tier (preserve existing)
  if [ "$api_exists" -eq 0 ] || [ "$react_exists" -eq 0 ]; then
    debug_echo "✅ Resources exist in subscription - preserving existing names"
    return 0  # Available (preserve existing)
  fi

  # Check DNS availability (only if resources don't exist in our subscription)
  local api_available=1
  local react_available=1

  if ! check_hostname_via_dns "$api_name"; then
    api_available=0
    debug_echo "API hostname available"
  else
    debug_echo "API hostname taken (DNS conflict)"
  fi

  if ! check_hostname_via_dns "$react_name"; then
    react_available=0
    debug_echo "React hostname available"
  else
    debug_echo "React hostname taken (DNS conflict)"
  fi

  # Both must be available for tier to pass
  if [ "$api_available" -eq 0 ] && [ "$react_available" -eq 0 ]; then
    debug_echo "✅ Both hostnames available"
    return 0  # Available
  else
    debug_echo "⚠️  Hostname conflict detected"
    return 1  # Conflict
  fi
}

# Main logic: Try tiers sequentially until one is available
determine_naming_tier() {
  local project_name="${TF_VAR_project_name:-consilient}"
  local environment="${TF_VAR_environment}"
  local region="${TF_VAR_region}"
  local subscription_id="${TF_VAR_subscription_id}"
  local resource_group="${TF_VAR_resource_group_name}"

  echo ""
  echo "🔍 Hostname Availability Pre-Check" >&2
  echo "Project: $project_name" >&2
  echo "Environment: $environment" >&2
  echo "Region: $region" >&2
  echo "" >&2

  # Validate required variables
  if [ -z "$environment" ] || [ -z "$region" ] || [ -z "$subscription_id" ] || [ -z "$resource_group" ]; then
    echo "❌ ERROR: Missing required environment variables" >&2
    echo "   Required: TF_VAR_environment, TF_VAR_region, TF_VAR_subscription_id, TF_VAR_resource_group_name" >&2
    exit 1
  fi

  # Try Tier 0: Standard names
  debug_echo ""
  if check_tier_availability "$TIER_STANDARD" "$project_name" "$environment" "$region" "$subscription_id" "$resource_group"; then
    echo "✅ Tier 0 (Standard) - Available" >&2
    echo "naming_tier=$TIER_STANDARD"
    return 0
  fi

  echo "⚠️  Tier 0 (Standard) - Conflict detected" >&2

  # Try Tier 1: Region suffix
  debug_echo ""
  if check_tier_availability "$TIER_REGION" "$project_name" "$environment" "$region" "$subscription_id" "$resource_group"; then
    echo "✅ Tier 1 (Region) - Available" >&2
    echo "naming_tier=$TIER_REGION"
    return 0
  fi

  echo "⚠️  Tier 1 (Region) - Conflict detected" >&2

  # Try Tier 2: Random suffix
  debug_echo ""
  if check_tier_availability "$TIER_RANDOM" "$project_name" "$environment" "$region" "$subscription_id" "$resource_group"; then
    echo "✅ Tier 2 (Random) - Available" >&2
    echo "naming_tier=$TIER_RANDOM"
    return 0
  fi

  # All tiers exhausted - fail with clear message
  echo "" >&2
  echo "❌ ERROR: All hostname tiers exhausted - no available names" >&2
  echo "" >&2
  echo "Attempted configurations:" >&2
  local api_std="${project_name}-api-${environment}"
  local react_std="${project_name}-react-${environment}"
  local region_lower=$(echo "${region}" | tr '[:upper:]' '[:lower:]' | tr -d ' ')
  local api_region="${project_name}-api-${environment}-${region_lower}"
  local react_region="${project_name}-react-${environment}-${region_lower}"
  echo "  1. Standard: $api_std, $react_std" >&2
  echo "  2. Region: $api_region, $react_region" >&2
  echo "  3. Random: ${api_std}-XXXX, ${react_std}-XXXX" >&2
  echo "" >&2
  echo "Resolution options:" >&2
  echo "  - Change TF_VAR_project_name to a unique value" >&2
  echo "  - Check for orphaned resources: az webapp list --query \"[?contains(name, '$project_name')]\"" >&2
  echo "  - Wait 24-48 hours if resources were recently deleted (DNS propagation)" >&2
  echo "" >&2
  exit 1
}

# Execute pre-check
determine_naming_tier
