# Azure Container Registry (ACR) Setup with Service Principal

This document explains how to set up GitHub secrets for ACR authentication using a service principal.

## Overview

The infrastructure uses a service principal to authenticate GitHub Actions workflows with Azure Container Registry (ACR). This is more secure than using ACR admin credentials.

The service principal must be created before Terraform can assign roles to it.

## Terraform Configuration

The service principal role assignments are configured in `infra/terraform/acr_service_principal.tf` and include:

- **AcrPush Role**: Allows the service principal to push images to the ACR
- **AcrPull Role**: Allows the service principal to pull images from the ACR

## Prerequisites

Make sure you're logged in with Azure CLI:

```bash
az login
```

Your account must have "Application Administrator" role in Azure AD to create applications.

## Deployment Steps

### Step 1: Deploy Terraform (Creates Service Principal Automatically)

Terraform will automatically create the Azure AD application, service principal, and client secret:

```bash
cd infra/terraform
terraform init
terraform plan
terraform apply
```

Terraform will:
1. Create an Azure AD application named `consilient{env}acr-cicd`
2. Create a service principal for that application
3. Create a client secret for authentication
4. Assign AcrPush and AcrPull roles to the service principal
5. (Optional) Update `.env.act` file with credentials if you provide the path

### Step 2: Optional - Update .env.act for Local Testing

If you're running tests locally with `act`, provide the path to the `.env.act` file:

```bash
terraform apply -var "env_act_file_path=../github_emulator/.env.act"
```

This will automatically update your `.env.act` file with:
- `ACR_REGISTRY`: The ACR URL
- `ACR_CICD_CLIENT_ID`: Service principal client ID
- `ACR_CICD_CLIENT_SECRET`: Service principal client secret

### Step 3: GitHub Secrets Setup

Set up the following GitHub secrets in your repository settings (`Settings > Secrets and variables > Actions`):

### Required Secrets

After Terraform applies successfully, get the values from the output files created in the terraform directory:

1. **ACR_REGISTRY**
   - Value: `terraform output acr_registry_url`
   - Or read from: `infra/terraform/acr_registry.txt`

2. **ACR_CICD_CLIENT_ID**
   - Value: `cat infra/terraform/acr_client_id.txt`

3. **ACR_CICD_CLIENT_SECRET**
   - Value: `cat infra/terraform/acr_client_secret.txt`
   - **Important**: This is sensitive; mark as secret and never expose in logs

Then set these in GitHub:
1. Go to your repository `Settings > Secrets and variables > Actions`
2. Add the three secrets with the values above

## Docker Login Flow

The GitHub Actions workflow uses Docker's built-in service principal authentication:

```yaml
- name: Log in to ACR
  uses: docker/login-action@v3
  with:
    registry: ${{ secrets.ACR_REGISTRY }}
    username: ${{ secrets.ACR_CICD_CLIENT_ID }}
    password: ${{ secrets.ACR_CICD_CLIENT_SECRET }}
```

Docker automatically handles service principal authentication when:
- Registry: ACR login server
- Username: Service principal client ID
- Password: Service principal client secret

## Security Considerations

1. **Least Privilege**: The service principal has only AcrPush and AcrPull permissions
2. **Rotation**: Service principal credentials should be rotated periodically
3. **Auditing**: All ACR operations are logged in Azure Activity Log
4. **Expiration**: By default, the client secret expires in 2 years

## Troubleshooting

### "Username and password required" error

This error occurs when GitHub secrets are not set or are empty:
1. Verify all three secrets are set in GitHub: `ACR_REGISTRY`, `ACR_CICD_CLIENT_ID`, `ACR_CICD_CLIENT_SECRET`
2. Check secret values match the `create_acr_service_principal.sh` output exactly
3. Verify service principal exists in Azure AD

### "Unauthorized: authentication required" error

This indicates the service principal credentials are incorrect:
1. Create new credentials using: `az ad app credential create --id <app-id> --display-name "github-actions"`
2. Update the `ACR_CICD_CLIENT_SECRET` secret in GitHub with the new password
3. Verify the service principal has AcrPush role assignment on the ACR

### Script fails with permission error

This occurs when your Azure AD account doesn't have sufficient permissions:
1. Ask your Azure AD administrator to grant you "Application Administrator" role
2. Then re-run the script

### "docker: command not found" error

This error occurs when running `act` locally without Docker installed:
1. Install Docker Desktop
2. Or use `act` with the Docker image that includes docker-in-docker

## Rotating Service Principal Credentials

To rotate the service principal credentials:

1. **Create a new secret**:
   ```bash
   az ad app credential create --id <app-id> --display-name "github-actions-rotated"
   ```

2. **Update GitHub secrets** with new credentials

3. **Delete old secret** (optional):
   ```bash
   # List credentials to find the old one
   az ad app credential list --id <app-id>
   ```

## Related Files

- Service Principal Creation & Role Assignments: `infra/terraform/acr_service_principal.tf`
- ACR Configuration: `infra/terraform/acr.tf`
- Terraform Variables: `infra/terraform/variables.tf`
- Workflow Configuration: `.github/workflows/dotnet_apps.yml`
- Local Testing Config: `infra/github_emulator/.env.act`
