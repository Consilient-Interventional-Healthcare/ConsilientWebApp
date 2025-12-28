#!/bin/bash
set +e  # Don't exit on error, we'll handle errors ourselves

# Import script for Terraform Azure resources
# This script attempts to import existing Azure resources into the Terraform state
# It's designed to be called standalone or from GitHub Actions workflow

# When running in GitHub Actions, always print verbose output
# When ACTIONS_STEP_DEBUG=true: show all echo statements (verbose mode)
# When ACTIONS_STEP_DEBUG=false: suppress informational echo statements (quiet mode)
# In GitHub Actions, always set DEBUG=true for full visibility
IN_GITHUB_ACTIONS=false
if [ -n "$GITHUB_ACTIONS" ] || [ -n "$GITHUB_WORKSPACE" ]; then
  IN_GITHUB_ACTIONS=true
  ACTIONS_STEP_DEBUG=true  # Force verbose output in GitHub Actions
fi

# Check if state file exists - skip imports if this is a fresh deployment
if [ ! -f "terraform.tfstate" ] && [ ! -f ".terraform/terraform.tfstate" ]; then
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "ℹ️  No Terraform state file found - this appears to be a fresh deployment"
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "ℹ️  Skipping resource import step (will create all resources from scratch)"
  exit 0
fi

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "=== Step 1: Reading CAE Configuration from Environment Variables ==="

# CAE configuration from environment variables
USE_SHARED_CAE="${TF_VAR_use_shared_container_environment}"
SHARED_CAE_NAME="${TF_VAR_shared_container_environment_name}"
CREATE_CAE="${TF_VAR_create_container_app_environment}"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "Configuration from environment:"
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  use_shared_container_environment: ${USE_SHARED_CAE}"
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  shared_container_environment_name: ${SHARED_CAE_NAME}"
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  create_container_app_environment: ${CREATE_CAE}"

# Determine expected CAE name based on configuration
if [ "$USE_SHARED_CAE" = "true" ]; then
  # Shared mode: Use fixed name (NO environment substitution!)
  EXPECTED_CAE_NAME="${SHARED_CAE_NAME}"
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "Using shared mode with FIXED name: ${EXPECTED_CAE_NAME}"
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  (This name is used across ALL environments: dev, staging, prod)"
else
  # Template mode: Would resolve from template, but we'll just let Terraform handle it
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "Using template mode - Terraform will resolve the name"
  EXPECTED_CAE_NAME=""  # Don't try to guess
fi

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "✅ Configuration loaded from environment variables"
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "All CAE settings will be managed by Terraform"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "=== Step 2: Importing Existing Azure Resources ==="
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""

# Initialize tracking for failed imports
failed_imports=0
failed_critical_imports=0
failed_import_list=()
failed_critical_import_list=()

# Function to import a resource if it exists
# Second parameter can be "critical" to mark resource as essential
import_resource() {
  local tf_address="$1"
  local resource_id="$2"
  local resource_name="$3"
  local is_critical="${4:-false}"  # Default to non-critical

  # Check if already in state
  if terraform state list | grep -q "^${tf_address}\$"; then
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ✅ Already in state: ${resource_name}"
    return 0
  fi

  # Try to import
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  📥 Importing: ${resource_name}"
  local import_output
  if import_output=$(terraform import "${tf_address}" "${resource_id}" 2>&1); then
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ✅ Imported successfully: ${resource_name}"
    return 0
  else
    # Check if error is because resource doesn't exist
    if echo "$import_output" | grep -q "Cannot import non-existent"; then
      [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  Resource does not exist in Azure: ${resource_name}"
    elif echo "$import_output" | grep -q "Resource already managed"; then
      [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ✅ Already managed: ${resource_name}"
    else
      echo "  ⚠️  Import failed: ${resource_name}"
      echo "$import_output" | grep -i "error" | head -3
      ((failed_imports++))

      if [ "$is_critical" = "critical" ]; then
        ((failed_critical_imports++))
        failed_critical_import_list+=("${resource_name}")
      else
        failed_import_list+=("${resource_name}")
      fi
    fi
    return 1
  fi
}

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "1. Resource Group"
RG_ID="/subscriptions/${TF_VAR_subscription_id}/resourceGroups/${TF_VAR_resource_group_name}"
import_resource "azurerm_resource_group.main" "${RG_ID}" "Resource Group"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "2. Networking Resources"
VNET_ID="${RG_ID}/providers/Microsoft.Network/virtualNetworks/consilient-vnet-${TF_VAR_environment}"
SUBNET_ID="${VNET_ID}/subnets/consilient-subnet-${TF_VAR_environment}"
import_resource "azurerm_virtual_network.main" "${VNET_ID}" "Virtual Network"
import_resource "azurerm_subnet.main" "${SUBNET_ID}" "Subnet"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "3. Container Registry"
ACR_NAME=$(terraform output -raw acr_name 2>/dev/null || echo "consilientacr${TF_VAR_environment}$(echo -n "${TF_VAR_subscription_id}-${TF_VAR_resource_group_name}" | md5sum | cut -c1-6)")
ACR_ID="${RG_ID}/providers/Microsoft.ContainerRegistry/registries/${ACR_NAME}"
import_resource "azurerm_container_registry.main" "${ACR_ID}" "Container Registry" "critical"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "4. SQL Server and Databases"
SQL_SUFFIX=$(echo -n "${TF_VAR_subscription_id}-${TF_VAR_resource_group_name}" | md5sum | cut -c1-6)
SQL_SERVER_NAME="consilient-sqlsrv-${TF_VAR_environment}-${SQL_SUFFIX}"
SQL_SERVER_ID="${RG_ID}/providers/Microsoft.Sql/servers/${SQL_SERVER_NAME}"
MAIN_DB_ID="${SQL_SERVER_ID}/databases/consilient_main_${TF_VAR_environment}"
HANGFIRE_DB_ID="${SQL_SERVER_ID}/databases/consilient_hangfire_${TF_VAR_environment}"
import_resource "azurerm_mssql_server.main" "${SQL_SERVER_ID}" "SQL Server" "critical"
import_resource "module.main_db.azurerm_mssql_database.this" "${MAIN_DB_ID}" "Main Database" "critical"
import_resource "module.hangfire_db.azurerm_mssql_database.this" "${HANGFIRE_DB_ID}" "Hangfire Database" "critical"

# Import SQL firewall rule if local firewall is enabled
if [ "${TF_VAR_enable_local_firewall}" = "true" ]; then
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  Importing SQL Firewall Rule (local firewall enabled)"
  FIREWALL_RULE_ID="${SQL_SERVER_ID}/firewallRules/AllowLocalActTesting"
  import_resource "azurerm_mssql_firewall_rule.local_act[0]" "${FIREWALL_RULE_ID}" "SQL Firewall Rule (Local Act)"
fi

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "5. Storage Account and Container"
STORAGE_NAME="consilientloki${TF_VAR_environment}${SQL_SUFFIX}"
STORAGE_ID="${RG_ID}/providers/Microsoft.Storage/storageAccounts/${STORAGE_NAME}"
STORAGE_CONTAINER_ID="${STORAGE_ID}/blobServices/default/containers/loki-data"
PRIVATE_ENDPOINT_ID="${RG_ID}/providers/Microsoft.Network/privateEndpoints/consilient-pe-loki-storage-${TF_VAR_environment}"
import_resource "azurerm_storage_account.loki" "${STORAGE_ID}" "Loki Storage Account"
import_resource "azurerm_storage_container.loki" "${STORAGE_CONTAINER_ID}" "Loki Storage Container"
import_resource "azurerm_private_endpoint.loki_storage" "${PRIVATE_ENDPOINT_ID}" "Loki Storage Private Endpoint"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "6. Managed Identity"
IDENTITY_ID="${RG_ID}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/consilient-loki-identity-${TF_VAR_environment}"
import_resource "azurerm_user_assigned_identity.loki" "${IDENTITY_ID}" "Loki Managed Identity"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "7. Role Assignment (Loki -> Storage)"
# Get the principal_id of the managed identity
PRINCIPAL_ID=$(az identity show --ids "${IDENTITY_ID}" --query principalId -o tsv 2>/dev/null || echo "")
if [ -n "$PRINCIPAL_ID" ]; then
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  Found managed identity principal ID: ${PRINCIPAL_ID}"
  # List role assignments for the storage account and find the one with this principal
  ROLE_ASSIGNMENT_ID=$(az role assignment list --scope "${STORAGE_ID}" --query "[?principalId=='${PRINCIPAL_ID}'].id | [0]" -o tsv 2>/dev/null || echo "")
  if [ -n "$ROLE_ASSIGNMENT_ID" ]; then
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  Found role assignment: ${ROLE_ASSIGNMENT_ID}"
    import_resource "azurerm_role_assignment.loki_blob" "${ROLE_ASSIGNMENT_ID}" "Loki Storage Blob Role Assignment"
  else
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  No role assignment found for this managed identity on storage account"
  fi
else
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  Managed identity not found, skipping role assignment import"
fi

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "8. Container App Environment"
# Import CAE only if:
# 1. We're NOT creating a new one (create_container_app_environment = false)
# 2. We're in shared mode (use_shared_container_environment = true)
# 3. The CAE name is known (from terraform.tfvars)
# 4. The CAE is in the SAME resource group as other resources

if [ "$CREATE_CAE" = "false" ] && [ "$USE_SHARED_CAE" = "true" ] && [ -n "$EXPECTED_CAE_NAME" ]; then
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  Shared mode detected: Checking if CAE '${EXPECTED_CAE_NAME}' exists in resource group '${TF_VAR_resource_group_name}'"

  # Check if CAE exists in Azure in the SAME resource group
  CAE_EXISTS=$(az containerapp env show \
    --name "${EXPECTED_CAE_NAME}" \
    --resource-group "${TF_VAR_resource_group_name}" \
    --query "id" -o tsv 2>/dev/null || echo "")

  if [ -n "$CAE_EXISTS" ]; then
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  CAE exists in same resource group - attempting import"
    CAE_ID="${RG_ID}/providers/Microsoft.App/managedEnvironments/${EXPECTED_CAE_NAME}"
    import_resource "azurerm_container_app_environment.shared[0]" "${CAE_ID}" "Container App Environment"
  else
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  CAE '${EXPECTED_CAE_NAME}' not found in resource group '${TF_VAR_resource_group_name}'"
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  Terraform will use data source to reference CAE from another resource group"
  fi
elif [ "$CREATE_CAE" = "true" ]; then
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  create_container_app_environment=true - Terraform will create a new CAE"
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  Attempting to import in case it already exists..."

  # Try to determine CAE name (in template mode, Terraform resolves it)
  if [ "$USE_SHARED_CAE" = "true" ]; then
    CAE_NAME="${EXPECTED_CAE_NAME}"
  else
    # Template mode - resolve the template from environment variable
    CAE_TEMPLATE="${TF_VAR_container_app_environment_name_template}"
    CAE_NAME="${CAE_TEMPLATE/\{environment\}/${TF_VAR_environment}}"
  fi

  if [ -n "$CAE_NAME" ]; then
    CAE_ID="${RG_ID}/providers/Microsoft.App/managedEnvironments/${CAE_NAME}"
    import_resource "azurerm_container_app_environment.shared[0]" "${CAE_ID}" "Container App Environment"
  fi
else
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  CAE configuration doesn't require import (using existing CAE via data source or ID)"
fi

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "9. Container App (Loki)"
CONTAINER_APP_ID="${RG_ID}/providers/Microsoft.App/containerApps/consilient-loki-${TF_VAR_environment}"
import_resource "azurerm_container_app.loki" "${CONTAINER_APP_ID}" "Loki Container App"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "10. Grafana"
GRAFANA_ID="${RG_ID}/providers/Microsoft.Dashboard/grafana/consilient-grafana-${TF_VAR_environment}"
import_resource "azurerm_dashboard_grafana.main" "${GRAFANA_ID}" "Grafana Dashboard"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "11. App Service Plans"
ASP_REACT_ID="${RG_ID}/providers/Microsoft.Web/serverFarms/consilient-asp-react-${TF_VAR_environment}"
ASP_API_ID="${RG_ID}/providers/Microsoft.Web/serverFarms/consilient-asp-api-${TF_VAR_environment}"
import_resource "module.react_app.azurerm_service_plan.this" "${ASP_REACT_ID}" "React App Service Plan" "critical"
import_resource "module.api_app.azurerm_service_plan.this" "${ASP_API_ID}" "API App Service Plan" "critical"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "12. App Services"
APP_REACT_ID="${RG_ID}/providers/Microsoft.Web/sites/consilient-react-${TF_VAR_environment}"
APP_API_ID="${RG_ID}/providers/Microsoft.Web/sites/consilient-api-${TF_VAR_environment}"
import_resource "module.react_app.azurerm_linux_web_app.this" "${APP_REACT_ID}" "React App Service" "critical"
import_resource "module.api_app.azurerm_linux_web_app.this" "${APP_API_ID}" "API App Service" "critical"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "13. ACR Pull Role Assignments"
# Get the principal IDs from Terraform root outputs (now exposed in outputs.tf)
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  Retrieving app service principal IDs from Terraform outputs..."
REACT_PRINCIPAL_ID=$(terraform output -raw "react_app_service_principal_id" 2>/dev/null || echo "")
API_PRINCIPAL_ID=$(terraform output -raw "api_app_service_principal_id" 2>/dev/null || echo "")

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && [ -n "$REACT_PRINCIPAL_ID" ] && echo "  ✅ React app principal ID: ${REACT_PRINCIPAL_ID}"
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && [ -n "$API_PRINCIPAL_ID" ] && echo "  ✅ API app principal ID: ${API_PRINCIPAL_ID}"

if [ -n "$REACT_PRINCIPAL_ID" ]; then
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  Found React app service principal ID: ${REACT_PRINCIPAL_ID}"
  # Find the role assignment for AcrPull on ACR
  REACT_ROLE_ASSIGNMENT_ID=$(az role assignment list --scope "${ACR_ID}" --query "[?principalId=='${REACT_PRINCIPAL_ID}' && roleDefinitionName=='AcrPull'].id | [0]" -o tsv 2>/dev/null || echo "")
  if [ -n "$REACT_ROLE_ASSIGNMENT_ID" ]; then
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  Found React ACR pull role assignment: ${REACT_ROLE_ASSIGNMENT_ID}"
    import_resource "azurerm_role_assignment.react_acr_pull" "${REACT_ROLE_ASSIGNMENT_ID}" "React ACR Pull Role Assignment"
  else
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  No AcrPull role assignment found for React app service on ACR"
  fi
else
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  React app service principal ID not found, skipping role assignment import"
fi

if [ -n "$API_PRINCIPAL_ID" ]; then
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  Found API app service principal ID: ${API_PRINCIPAL_ID}"
  # Find the role assignment for AcrPull on ACR
  API_ROLE_ASSIGNMENT_ID=$(az role assignment list --scope "${ACR_ID}" --query "[?principalId=='${API_PRINCIPAL_ID}' && roleDefinitionName=='AcrPull'].id | [0]" -o tsv 2>/dev/null || echo "")
  if [ -n "$API_ROLE_ASSIGNMENT_ID" ]; then
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  Found API ACR pull role assignment: ${API_ROLE_ASSIGNMENT_ID}"
    import_resource "azurerm_role_assignment.api_acr_pull" "${API_ROLE_ASSIGNMENT_ID}" "API ACR Pull Role Assignment"
  else
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  No AcrPull role assignment found for API app service on ACR"
  fi
else
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  API app service principal ID not found, skipping role assignment import"
fi

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "14. Key Vault"
KV_NAME=$(terraform output -raw key_vault_name 2>/dev/null || echo "consilient-kv-${TF_VAR_environment}-$(echo -n "${TF_VAR_subscription_id}-${TF_VAR_resource_group_name}" | md5sum | cut -c1-6)")
KV_ID="${RG_ID}/providers/Microsoft.KeyVault/vaults/${KV_NAME}"
import_resource "azurerm_key_vault.main" "${KV_ID}" "Key Vault" "critical"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "15. Key Vault Secrets"
import_resource "azurerm_key_vault_secret.sql_connection_main" "${KV_ID}/secrets/sql-connection-string-main" "SQL Main Connection Secret"
import_resource "azurerm_key_vault_secret.sql_connection_hangfire" "${KV_ID}/secrets/sql-connection-string-hangfire" "SQL Hangfire Connection Secret"
import_resource "azurerm_key_vault_secret.jwt_signing_secret" "${KV_ID}/secrets/jwt-signing-secret" "JWT Signing Secret"
import_resource "azurerm_key_vault_secret.grafana_loki_url" "${KV_ID}/secrets/grafana-loki-url" "Grafana Loki URL Secret"

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "16. Key Vault Role Assignments"
if [ -n "$API_PRINCIPAL_ID" ]; then
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  Found API app service principal ID: ${API_PRINCIPAL_ID}"
  # Find the role assignment for Key Vault Secrets User
  API_KV_ROLE_ASSIGNMENT_ID=$(az role assignment list --scope "${KV_ID}" --query "[?principalId=='${API_PRINCIPAL_ID}' && roleDefinitionName=='Key Vault Secrets User'].id | [0]" -o tsv 2>/dev/null || echo "")
  if [ -n "$API_KV_ROLE_ASSIGNMENT_ID" ]; then
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  Found API Key Vault role assignment: ${API_KV_ROLE_ASSIGNMENT_ID}"
    import_resource "azurerm_role_assignment.api_keyvault_secrets_user" "${API_KV_ROLE_ASSIGNMENT_ID}" "API Key Vault Secrets User Role Assignment"
  else
    [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  No Key Vault Secrets User role assignment found for API app service"
  fi
else
  [[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "  ℹ️  API app service principal ID not found, skipping Key Vault role assignment import"
fi

[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "=== Import Process Complete ==="
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo ""
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "Configuration will be managed by terraform.tfvars"
[[ "${ACTIONS_STEP_DEBUG}" == "true" ]] && echo "No workflow overrides applied"

# Generate GitHub step summary for critical failures (if running in GitHub Actions)
if [ -n "$GITHUB_STEP_SUMMARY" ]; then
  if [ $failed_critical_imports -gt 0 ]; then
    echo "### ❌ Critical Import Failures" >> $GITHUB_STEP_SUMMARY
    echo "" >> $GITHUB_STEP_SUMMARY
    echo "The following **critical** resources failed to import:" >> $GITHUB_STEP_SUMMARY
    echo "" >> $GITHUB_STEP_SUMMARY
    for resource in "${failed_critical_import_list[@]}"; do
      echo "- \`${resource}\`" >> $GITHUB_STEP_SUMMARY
    done
    echo "" >> $GITHUB_STEP_SUMMARY
    echo "**Action Required**: These resources are essential to the infrastructure and must be resolvable by Terraform." >> $GITHUB_STEP_SUMMARY
    echo "- Check if the resources actually exist in Azure under the correct resource group" >> $GITHUB_STEP_SUMMARY
    echo "- Verify the resource naming conventions match the Terraform configuration" >> $GITHUB_STEP_SUMMARY
    echo "- Ensure proper Azure credentials and permissions to list resources" >> $GITHUB_STEP_SUMMARY
    echo "- If resources don't exist yet, remove them from tfstate or use \`terraform apply\` to create them" >> $GITHUB_STEP_SUMMARY
    echo "" >> $GITHUB_STEP_SUMMARY
  fi

  if [ $failed_imports -gt 0 ] && [ $failed_critical_imports -eq 0 ]; then
    echo "### ⚠️ Non-Critical Import Warnings" >> $GITHUB_STEP_SUMMARY
    echo "" >> $GITHUB_STEP_SUMMARY
    echo "The following non-critical resources failed to import:" >> $GITHUB_STEP_SUMMARY
    echo "" >> $GITHUB_STEP_SUMMARY
    for resource in "${failed_import_list[@]}"; do
      echo "- \`${resource}\`" >> $GITHUB_STEP_SUMMARY
    done
    echo "" >> $GITHUB_STEP_SUMMARY
    echo "**Note**: Import failures are expected if resources don't exist in Azure yet." >> $GITHUB_STEP_SUMMARY
    echo "Terraform will create them during the apply step." >> $GITHUB_STEP_SUMMARY
    echo "" >> $GITHUB_STEP_SUMMARY
  fi

  echo "Total import attempts: $(terraform state list | wc -l) resources in state" >> $GITHUB_STEP_SUMMARY
  echo "Failed critical imports: $failed_critical_imports" >> $GITHUB_STEP_SUMMARY
  echo "Failed non-critical imports: ${#failed_import_list[@]}" >> $GITHUB_STEP_SUMMARY
fi

# Fail if critical resources failed to import
if [ $failed_critical_imports -gt 0 ]; then
  echo ""
  echo "❌ FAILURE: Critical resources failed to import. Terraform state is out of sync with Azure resources."
  exit 1
fi

exit 0  # Succeed if only non-critical imports failed
