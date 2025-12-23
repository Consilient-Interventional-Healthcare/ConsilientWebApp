#!/bin/bash
# Script to create ACR CI/CD service principal using Azure CLI
# This script must be run before 'terraform apply' with proper Azure AD permissions
# Run as: bash create_acr_service_principal.sh

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}=== ACR CI/CD Service Principal Creation ===${NC}"
echo ""

# Get the ACR name from Terraform locals
ACR_NAME_PREFIX="consilient"
ENVIRONMENT="${1:-dev}"

# Generate a unique suffix (same as Terraform locals)
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
RESOURCE_GROUP="${2:-consilient-resource-group}"

echo "Subscription ID: $SUBSCRIPTION_ID"
echo "Environment: $ENVIRONMENT"
echo "Resource Group: $RESOURCE_GROUP"
echo ""

# Create Azure AD Application
echo -e "${YELLOW}Creating Azure AD Application...${NC}"
APP_NAME="${ACR_NAME_PREFIX}acr${ENVIRONMENT}-cicd"
APP=$(az ad app create --display-name "$APP_NAME" --query id -o tsv)
echo -e "${GREEN}✓ Application created: $APP_NAME (ID: $APP)${NC}"
echo ""

# Create Service Principal
echo -e "${YELLOW}Creating Service Principal...${NC}"
SP=$(az ad sp create --id "$APP" --query id -o tsv)
echo -e "${GREEN}✓ Service Principal created (Object ID: $SP)${NC}"
echo ""

# Create Client Secret
echo -e "${YELLOW}Creating Client Secret...${NC}"
SECRET=$(az ad app credential create --id "$APP" --display-name "github-actions" --query password -o tsv)
CLIENT_ID=$(az ad app show --id "$APP" --query appId -o tsv)
echo -e "${GREEN}✓ Client Secret created${NC}"
echo ""

# Output values needed for GitHub
echo -e "${YELLOW}=== Values for GitHub Actions Secrets ===${NC}"
echo ""
echo -e "${GREEN}ACR_REGISTRY:${NC}"
echo "consilient${ENVIRONMENT}.azurecr.io"
echo ""
echo -e "${GREEN}ACR_CICD_CLIENT_ID:${NC}"
echo "$CLIENT_ID"
echo ""
echo -e "${GREEN}ACR_CICD_CLIENT_SECRET:${NC}"
echo "$SECRET"
echo ""

# Output values for Terraform
echo -e "${YELLOW}=== Value for Terraform ===${NC}"
echo ""
echo "Add this to your terraform.tfvars or pass via -var flag:"
echo ""
echo "acr_cicd_service_principal_object_id = \"$SP\""
echo ""

# Save to a temporary file for reference
TEMP_FILE=".acr_sp_values.txt"
cat > "$TEMP_FILE" << EOF
# ACR Service Principal Values
# Created: $(date)
# Save these values securely!

ACR_REGISTRY=consilient${ENVIRONMENT}.azurecr.io
ACR_CICD_CLIENT_ID=${CLIENT_ID}
ACR_CICD_CLIENT_SECRET=${SECRET}

# For Terraform:
acr_cicd_service_principal_object_id = "${SP}"
EOF

echo -e "${GREEN}Values also saved to: $TEMP_FILE${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Set GitHub Actions secrets with the values above"
echo "2. Run: terraform apply -var acr_cicd_service_principal_object_id=$SP"
echo ""
