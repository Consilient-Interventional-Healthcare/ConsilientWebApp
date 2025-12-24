# Azure Container Registry (ACR) Authentication Setup

## Overview

This project uses two different authentication methods for ACR depending on the environment:

- **OIDC (OpenID Connect)** - For GitHub Actions workflows (recommended, no secrets needed)
- **Service Principal** - For local testing with the `act` tool

## GitHub Actions Setup (OIDC)

### Prerequisites

- Azure CLI installed and authenticated to your Azure subscription
- Contributor or Owner role on the Azure subscription
- GitHub repository already created

### Step 1: Create Federated Identity Credential

First, gather the required information:

```bash
# Get your ACR name
ACR_NAME="consilientacrdeveca75c"

# GitHub repository information
REPO_OWNER="your-github-org-or-username"
REPO_NAME="ConsilientWebApp"

# Create Azure AD application for GitHub OIDC
APP_NAME="consilient-acr-github-oidc"
APP_ID=$(az ad app create --display-name "$APP_NAME" --query appId -o tsv)
echo "Application ID: $APP_ID"

# Create service principal from the application
az ad sp create --id $APP_ID

# Get the service principal object ID
SP_OBJECT_ID=$(az ad sp show --id $APP_ID --query id -o tsv)
echo "Service Principal Object ID: $SP_OBJECT_ID"
```

Create federated credentials for GitHub to authenticate as this service principal:

```bash
# Create federated credential for main branch
az ad app federated-credential create \
  --id $APP_ID \
  --parameters '{
    "name": "github-main",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'$REPO_OWNER'/'$REPO_NAME':ref:refs/heads/main",
    "audiences": ["api://AzureADTokenExchange"]
  }'

# Create federated credential for develop branch
az ad app federated-credential create \
  --id $APP_ID \
  --parameters '{
    "name": "github-develop",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'$REPO_OWNER'/'$REPO_NAME':ref:refs/heads/develop",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

### Step 2: Assign ACR Roles

Grant the service principal permission to push and pull images from ACR:

```bash
# Get the ACR resource ID
ACR_ID=$(az acr show --name $ACR_NAME --query id -o tsv)

# Assign AcrPush role (allows pushing images)
az role assignment create \
  --assignee $SP_OBJECT_ID \
  --role "AcrPush" \
  --scope $ACR_ID

# Assign AcrPull role (allows pulling base images during builds)
az role assignment create \
  --assignee $SP_OBJECT_ID \
  --role "AcrPull" \
  --scope $ACR_ID

echo "Role assignments completed successfully"
```

### Step 3: Add GitHub Secrets

Add the following secrets to your GitHub repository (Settings > Secrets and variables > Actions):

1. **AZURE_CLIENT_ID**
   - Value: The `$APP_ID` from Step 1
   - This is the application ID used for OIDC authentication

2. **AZURE_TENANT_ID**
   - Value: Run `az account show --query tenantId -o tsv`
   - This is your Azure AD tenant ID

3. **AZURE_SUBSCRIPTION_ID**
   - Value: Run `az account show --query id -o tsv`
   - This is your Azure subscription ID

4. **ACR_REGISTRY** (keep existing if already set)
   - Value: `$ACR_NAME.azurecr.io` (e.g., `consilientacrdeveca75c.azurecr.io`)
   - This is the ACR login server URL

### Step 4: Verify OIDC Authentication

The GitHub Actions workflows will automatically use OIDC when running in GitHub Actions. To verify:

1. Push a commit to the `main` or `develop` branch
2. Check the workflow run in the Actions tab
3. Verify that "Login to Azure (OIDC)" and "Log in to ACR (via Azure CLI)" steps succeed

## Local Testing Setup (act)

### Prerequisites

- Azure CLI installed
- `act` tool installed ([https://github.com/nektos/act](https://github.com/nektos/act))
- A service principal with credentials for local testing

### Step 1: Create Service Principal for Local Testing

Create a separate service principal specifically for local development (don't reuse the OIDC one):

```bash
# Set variables
LOCAL_SP_NAME="consilient-acr-local-dev"
ACR_NAME="consilientacrdeveca75c"

# Create service principal with password
SP_JSON=$(az ad sp create-for-rbac --name $LOCAL_SP_NAME --query "{clientId:appId,clientSecret:password,objectId:id}")

# Extract the values
CLIENT_ID=$(echo $SP_JSON | jq -r '.clientId')
CLIENT_SECRET=$(echo $SP_JSON | jq -r '.clientSecret')
SP_OBJECT_ID=$(echo $SP_JSON | jq -r '.objectId')

echo "Service Principal created:"
echo "  Client ID: $CLIENT_ID"
echo "  Client Secret: $CLIENT_SECRET (save this somewhere safe)"
echo "  Object ID: $SP_OBJECT_ID"
```

### Step 2: Assign ACR Roles to Service Principal

```bash
# Get ACR resource ID
ACR_ID=$(az acr show --name $ACR_NAME --query id -o tsv)

# Assign roles
az role assignment create \
  --assignee $SP_OBJECT_ID \
  --role "AcrPush" \
  --scope $ACR_ID

az role assignment create \
  --assignee $SP_OBJECT_ID \
  --role "AcrPull" \
  --scope $ACR_ID

echo "ACR role assignments completed"
```

### Step 3: Configure .env.act File

Update the `.env.act` file for the `act` tool with your service principal credentials:

**File:** `infra/github_emulator/.env.act`

Set the ACR credentials (around lines 36-38):

```bash
ACR_REGISTRY=consilientacrdeveca75c.azurecr.io
ACR_CICD_CLIENT_ID=<your-client-id-from-step-1>
ACR_CICD_CLIENT_SECRET=<your-client-secret-from-step-1>
```

Also ensure Azure credentials are set for `act` (these should already be in the file):

```bash
# Lines 7-10 should have valid Azure credentials for act
AZURE_CREDENTIALS={"clientId":"...","clientSecret":"...","subscriptionId":"...","tenantId":"..."}
ARM_CLIENT_ID=...
ARM_CLIENT_SECRET=...
ARM_TENANT_ID=...
```

### Step 4: Run Workflows Locally with act

Test the build workflow locally:

```bash
cd infra/github_emulator

# Run the dotnet_apps workflow
act workflow_dispatch --input environment=dev

# Or trigger on push
act push
```

The workflow will use the service principal credentials from `.env.act` for authentication.

## Security Best Practices

### OIDC Authentication (GitHub Actions)

✅ **Advantages:**

- No long-lived secrets stored in GitHub
- Secrets are never exposed in logs
- Tokens are short-lived and automatically rotated
- Federated credentials are scoped to specific branches
- No need to rotate credentials (keys are automatically refreshed)
- Industry best practice for CI/CD

❌ **Limitations:**

- Only works in GitHub Actions cloud environment
- Federated credentials must be configured for each branch
- Initial setup is more complex than service principal

### Service Principal (Local Testing)

✅ **Advantages:**

- Works for both cloud and local environments
- Simple to set up
- Useful for development and testing

⚠️ **Security Considerations:**

- Credentials are long-lived (must be rotated periodically)
- `.env.act` file should never be committed to git (it's in `.gitignore`)
- Limit service principal permissions to only what's needed (AcrPush, AcrPull)
- Consider using a separate service principal for local testing vs production
- Regenerate credentials if they're exposed

### General Recommendations

1. **Never commit secrets to git** - The `.env.act` file is in `.gitignore` for this reason
2. **Rotate credentials periodically** - Service principal passwords should be rotated every 90 days
3. **Use least privilege** - Only assign the minimum required roles (AcrPush, AcrPull)
4. **Monitor service principal usage** - Check Azure AD sign-in logs periodically
5. **Use OIDC when possible** - It's the most secure method for CI/CD
6. **Keep local credentials separate** - Don't reuse the OIDC service principal credentials locally

## Troubleshooting

### OIDC Authentication Failed in GitHub Actions

**Error:** "Login to Azure (OIDC) - ...failed to authenticate..."

**Solutions:**

1. Verify GitHub secrets are set correctly:
   - `AZURE_CLIENT_ID` matches the app ID
   - `AZURE_TENANT_ID` is correct
   - `AZURE_SUBSCRIPTION_ID` is correct

2. Verify federated credentials are configured:
   ```bash
   az ad app federated-credential list --id $APP_ID
   ```

3. Check that the service principal has ACR roles:
   ```bash
   az role assignment list --assignee $SP_OBJECT_ID --scope $ACR_ID
   ```

4. Ensure the commit is on the correct branch (main or develop)

### ACR Login Failed in GitHub Actions

**Error:** "Log in to ACR (via Azure CLI) - ...authentication failed..."

**Solutions:**

1. Verify `ACR_REGISTRY` secret is set correctly
2. Verify the service principal has AcrPush and AcrPull roles
3. Check that ACR admin user is not required (not enabled in settings)

### Service Principal Credentials Don't Work Locally

**Error:** "error: docker login credentials missing"

**Solutions:**

1. Verify `.env.act` file has correct credentials:
   ```bash
   grep -E "^ACR_" infra/github_emulator/.env.act
   ```

2. Verify the service principal hasn't expired
3. Regenerate credentials if needed:
   ```bash
   az ad sp credential reset --id $SP_OBJECT_ID
   ```

4. Verify the service principal has ACR roles:
   ```bash
   az role assignment list --assignee $SP_OBJECT_ID --scope $ACR_ID
   ```

### "az acr login" Command Not Available

**Solution:** Install Azure CLI:

```bash
# macOS
brew install azure-cli

# Linux
curl -sL https://aka.ms/InstallAzureCLI | bash

# Windows
choco install azure-cli
# or download from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli
```

## Cleanup

### Remove Old Service Principal (After Migration)

If you had an old Terraform-created service principal, you can delete it:

```bash
# Find service principals created by Terraform
az ad sp list --display-name "*acr-cicd*" --query "[].{displayName:displayName, appId:appId, id:id}" -o table

# Delete the old one (replace with actual values)
az ad sp delete --id <SERVICE-PRINCIPAL-OBJECT-ID>
```

## Additional Resources

- [Configure OpenID Connect in Azure - GitHub Docs](https://docs.github.com/en/actions/security-for-github-actions/security-hardening-your-deployments/configuring-openid-connect-in-azure)
- [Authenticate to Azure from GitHub Actions by OpenID Connect](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure-openid-connect)
- [Azure Container Registry Authentication](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-authentication)
- [Azure Service Principal](https://learn.microsoft.com/en-us/azure/active-directory/develop/app-objects-and-service-principals)
