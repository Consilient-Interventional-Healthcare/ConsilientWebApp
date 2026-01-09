#!/bin/bash

# Terraform Outputs Capture Script
# Captures terraform outputs and writes to GITHUB_OUTPUT
# Used by both terraform.yml and terraform-outputs.yml
#
# Prerequisites:
#   - Must be run from infra/terraform directory (or working-directory set)
#   - Terraform must be initialized
#
# Outputs written to GITHUB_OUTPUT:
#   - api_app_name
#   - react_app_name
#   - acr_registry_url
#   - resource_group_name
#   - sql_server_fqdn

set -e

# Capture outputs (with fallback to empty string on error)
API_APP_NAME=$(terraform output -raw api_app_service_name 2>/dev/null || echo "")
REACT_APP_NAME=$(terraform output -raw react_app_service_name 2>/dev/null || echo "")
ACR_REGISTRY_URL=$(terraform output -raw acr_registry_url 2>/dev/null || echo "")
RESOURCE_GROUP_NAME=$(terraform output -raw resource_group_name 2>/dev/null || echo "")
SQL_SERVER_FQDN=$(terraform output -raw sql_server_fqdn 2>/dev/null || echo "")

# Debug output (when ACTIONS_STEP_DEBUG is enabled)
if [[ "${ACTIONS_STEP_DEBUG}" == "true" ]]; then
  echo "Captured Terraform Outputs:"
  echo "  API App Name: ${API_APP_NAME}"
  echo "  React App Name: ${REACT_APP_NAME}"
  echo "  ACR Registry URL: ${ACR_REGISTRY_URL}"
  echo "  Resource Group Name: ${RESOURCE_GROUP_NAME}"
  echo "  SQL Server FQDN: ${SQL_SERVER_FQDN}"
fi

# Write to GITHUB_OUTPUT
echo "api_app_name=${API_APP_NAME}" >> $GITHUB_OUTPUT
echo "react_app_name=${REACT_APP_NAME}" >> $GITHUB_OUTPUT
echo "acr_registry_url=${ACR_REGISTRY_URL}" >> $GITHUB_OUTPUT
echo "resource_group_name=${RESOURCE_GROUP_NAME}" >> $GITHUB_OUTPUT
echo "sql_server_fqdn=${SQL_SERVER_FQDN}" >> $GITHUB_OUTPUT

echo "✅ Terraform outputs captured"
