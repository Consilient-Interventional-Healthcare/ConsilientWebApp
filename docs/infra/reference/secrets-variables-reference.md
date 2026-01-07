# Secrets & Variables Complete Reference

Comprehensive reference for all secrets, variables, and configuration used across GitHub Actions workflows and Terraform infrastructure.

## Table of Contents

1. [Overview](#overview)
2. [Quick Reference Tables](#quick-reference-tables)
3. [GitHub Actions Configuration](#github-actions-configuration)
   - [GitHub Secrets](#github-secrets)
   - [GitHub Variables](#github-variables)
4. [Terraform Configuration](#terraform-configuration)
   - [Root Module Variables](#root-module-variables)
   - [Environment-Based Configuration](#environment-based-configuration)
   - [Module Variables Overview](#module-variables-overview)
5. [Azure App Configuration](#azure-app-configuration)
   - [Configuration Management Philosophy: "Source of Creation" Rule](#configuration-management-philosophy-source-of-creation-rule)
   - [App Configuration Structure](#app-configuration-structure)
   - [App Configuration Integration](#app-configuration-integration)
   - [SKU and Pricing](#sku-and-pricing)
   - [Configuration Refresh Strategy](#configuration-refresh-strategy)
6. [Azure Key Vault Secrets](#azure-key-vault-secrets)
7. [Environment-Specific Configuration](#environment-specific-configuration)
8. [Setup & Configuration Guide](#setup--configuration-guide)
9. [Security Best Practices](#security-best-practices)
10. [Troubleshooting](#troubleshooting)
11. [Related Documentation](#related-documentation)

---

## Overview

This reference documents the complete configuration landscape for the ConsilientWebApp infrastructure, consolidating secrets, variables, and configuration from three layers:

1. **GitHub Actions Layer** - Secrets and variables used by CI/CD workflows
2. **Terraform Layer** - Infrastructure-as-code variables and configuration
3. **Azure Layer** - Azure Key Vault secrets for runtime configuration

### Configuration Flow

```
GitHub Actions Workflows
  ├─ GitHub Secrets (OIDC, Service Principal, Database, Application)
  ├─ GitHub Variables (Container Registry, Infrastructure, Database, CAE)
  └─ Environment Variables (TF_VAR_* for Terraform injection)
         ↓
Terraform Execution
  ├─ Root Module Variables (22 variables)
  ├─ Module Variables (app_service, sql_database, storage_account)
  ├─ Locals Configuration (environment-specific SKUs, costs)
  └─ Azure Resources Creation
         ↓
Azure Key Vault
  ├─ sql-connection-string-main
  ├─ sql-connection-string-hangfire
  ├─ jwt-signing-secret
  ├─ grafana-loki-url
  └─ oauth-client-secret (conditional)
         ↓
App Service
  └─ Key Vault References in Application Settings
```

### How to Use This Reference

- **Quick Lookup**: Use the [Quick Reference Tables](#quick-reference-tables) section for a fast overview
- **Setup**: Follow [Setup & Configuration Guide](#setup--configuration-guide) for initial configuration
- **Security**: Review [Security Best Practices](#security-best-practices) for secure configuration
- **Troubleshooting**: Check [Troubleshooting](#troubleshooting) for common issues and solutions
- **Details**: Browse individual sections for comprehensive information about each configuration

---

## Quick Reference Tables

### Table 1: All GitHub Secrets (14 Total)

| Secret Name | Type | Required | Category | Security | Used By |
|-------------|------|----------|----------|----------|---------|
| AZURE_CLIENT_ID | UUID | Yes | OIDC | High | azure-login |
| AZURE_TENANT_ID | UUID | Yes | OIDC | High | azure-login |
| AZURE_SUBSCRIPTION_ID | UUID | Yes | OIDC | High | terraform, azure-cli |
| ARM_CLIENT_ID | UUID | Yes | Terraform | High | terraform-provider |
| ARM_CLIENT_SECRET | String | Yes | Terraform | Critical | terraform-provider |
| ARM_TENANT_ID | UUID | Yes | Terraform | High | terraform-provider |
| SQL_ADMIN_USERNAME | String | Yes | Database | High | databases.yml |
| SQL_ADMIN_PASSWORD | String | Yes | Database | Critical | databases.yml |
| JWT_SIGNING_SECRET | String | Yes | Application | Critical | terraform, keyvault |
| OAUTH_CLIENT_SECRET | String | No | Application | Critical | terraform, keyvault |
| AZURE_CREDENTIALS | JSON | No | Legacy | High | act (local testing) |
| AZURE_API_WEBAPP_NAME | String | No | Legacy | Low | dotnet_apps.yml |
| AZURE_WEBAPP_NAME | String | No | Legacy | Low | react_apps.yml |
| GITHUB_TOKEN | String | Auto | GitHub | Medium | container-registry |

### Table 2: All GitHub Variables (13 Total)

| Variable Name | Scope | Default | Used By | Category |
|---------------|-------|---------|---------|----------|
| CONTAINER_REGISTRY | Repo | ghcr.io | workflows | Registry |
| ACR_REGISTRY_URL | Repo | - | dotnet_apps, react_apps | Registry |
| API_IMAGE_NAME | Repo | consilient-api | dotnet_apps | Registry |
| REACT_IMAGE_NAME | Repo | consilient-react | react_apps | Registry |
| ACTIONS_RUNNER_IMAGE | Repo | consilient-runner:latest | build-runner-image | Registry |
| AZURE_SQL_SERVER_FQDN | Repo | - | databases | Database |
| SCHEMASPY_VERSION | Repo | 6.2.4 | docs_db | Database |
| JDBC_DRIVER_VERSION | Repo | 12.4.2.jre11 | docs_db | Database |
| AZURE_REGION | Repo | canadacentral | terraform | Infrastructure |
| AZURE_RESOURCE_GROUP_NAME | Repo | consilient-resource-group | terraform | Infrastructure |
| TERRAFORM_VERSION | Repo | 1.6.0 | terraform | Infrastructure |

### Table 3: Terraform Root Variables by Category (22 Total)

| Variable | Type | Default | Required | Sensitive |
|----------|------|---------|----------|-----------|
| **Global Configuration** |
| project_name | string | consilient | No | No |
| environment | string | - | Yes | No |
| region | string | - | Yes | No |
| resource_group_name | string | - | Yes | No |
| subscription_id | string | - | Yes | No |
| **Database** |
| sql_admin_username | string | - | Yes | No |
| sql_admin_password | string | - | Yes | **Yes** |
| **Application Secrets** |
| jwt_signing_secret | string | - | Yes | **Yes** |
| oauth_client_secret | string | "" | No | **Yes** |
| **Monitoring** |
| loki_retention | string | 30d | No | No |
| loki_cpu_request | number | 0.5 | No | No |
| loki_cpu_limit | number | 1.0 | No | No |
| loki_memory_request | string | 1.0Gi | No | No |
| loki_memory_limit | string | 2.0Gi | No | No |
| grafana_major_version | number | 11 | No | No |
| **Container App Environment** |
| create_container_app_environment | bool | true | No | No |
| container_app_environment_name_template | string | consilient-cae-{environment} | No | No |
| existing_container_app_environment_id | string | "" | No | No |
| **Security & Networking** |
| enable_local_firewall | bool | false | No | No |
| **Custom Domains** |
| api_custom_domain | string | "" | No | No |
| react_custom_domain | string | "" | No | No |

### Configuration Verification Checklist

Before deploying, verify:

**GitHub Secrets (9 required):**
- [ ] AZURE_CLIENT_ID configured
- [ ] AZURE_TENANT_ID configured
- [ ] AZURE_SUBSCRIPTION_ID configured
- [ ] ARM_CLIENT_ID configured
- [ ] ARM_CLIENT_SECRET configured
- [ ] ARM_TENANT_ID configured
- [ ] SQL_ADMIN_USERNAME configured
- [ ] SQL_ADMIN_PASSWORD configured
- [ ] JWT_SIGNING_SECRET configured

**GitHub Variables (minimum required):**
- [ ] AZURE_REGION configured
- [ ] AZURE_RESOURCE_GROUP_NAME configured
- [ ] TERRAFORM_VERSION configured

**Terraform Variables:**
- [ ] terraform.tfvars exists in infra/terraform/
- [ ] environment set (dev or prod)
- [ ] sql_admin_username set
- [ ] Sensitive variables injected via TF_VAR_* or CI/CD

**Security:**
- [ ] No secrets committed to Git
- [ ] No hardcoded passwords in terraform.tfvars
- [ ] enable_local_firewall = false in production
- [ ] All secrets masked in CI/CD logs

---

## GitHub Actions Configuration

### GitHub Secrets

GitHub Secrets are sensitive credentials stored at the repository level. They are automatically masked in workflow logs and can only be accessed by workflows.

> **Security**: Secrets are critical to infrastructure security. Never commit them to source control. Use GitHub's secret management exclusively.

#### OIDC Authentication Secrets

These secrets enable OpenID Connect (OIDC) authentication to Azure without storing long-lived credentials.

##### AZURE_CLIENT_ID

| Property | Value |
|----------|-------|
| Required | Yes |
| Type | UUID (format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx) |
| Used By | `azure/login@v1` action in workflows |
| Security Level | High |
| Rotation | Auto (federated credential) |

**Description**: The Application ID (Client ID) of the Entra ID application configured for OIDC authentication. This identifies your GitHub repository to Azure.

**How to Obtain**:

```powershell
# List OIDC applications
az ad app list --display-name "consilient-app" --query '[0].appId' -o tsv

# Or search by display name
az ad app list --query "[?displayName=='consilient-app'].appId" -o tsv
```

**Example Format**:
```
12345678-1234-1234-1234-123456789012
```

**Security Notes**:
- This is a public identifier, safe to share
- Federated credentials control access (not a secret itself)
- Rotation happens when federated credential is updated

**Related Documentation**: [components/authentication.md](../components/authentication.md#oidc-authentication)

---

##### AZURE_TENANT_ID

| Property | Value |
|----------|-------|
| Required | Yes |
| Type | UUID (Azure AD Tenant ID) |
| Used By | `azure/login@v1` action in workflows |
| Security Level | High |
| Rotation | Never (unless organization changes) |

**Description**: Your Azure Entra ID tenant ID. This identifies the Azure directory where your resources and service principals exist.

**How to Obtain**:

```powershell
# Get your current tenant ID
az account show --query tenantId -o tsv

# Or list all tenant IDs your account can access
az account list --query '[].tenantId' -o tsv
```

**Example Format**:
```
87654321-4321-4321-4321-210987654321
```

**Security Notes**:
- This is an organizational identifier, not secret but should not be widely publicized
- Same for all resources in your Azure organization

**Related Secrets**: AZURE_CLIENT_ID, AZURE_SUBSCRIPTION_ID

---

##### AZURE_SUBSCRIPTION_ID

| Property | Value |
|----------|-------|
| Required | Yes |
| Type | UUID (Azure Subscription ID) |
| Used By | Terraform provider, Azure CLI commands |
| Security Level | High |
| Rotation | Never (unless subscription changes) |

**Description**: Your Azure Subscription ID where resources are deployed. This identifies which Azure subscription to bill resources to.

**How to Obtain**:

```powershell
# Get current subscription ID
az account show --query id -o tsv

# Or list all subscriptions
az account list --query '[].id' -o tsv

# Set as current subscription
az account set --subscription "<subscription-id>"
```

**Example Format**:
```
aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee
```

**Security Notes**:
- Subscription ID identifies your Azure account financially
- Should be treated as sensitive; not shared widely
- Required for all infrastructure operations

**Related Secrets**: AZURE_CLIENT_ID, AZURE_TENANT_ID

**Workflow Usage**:
```yaml
env:
  AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
  AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
  AZURE_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

---

#### Terraform Service Principal Secrets

These secrets authenticate Terraform to Azure using a Service Principal. The Service Principal needs Contributor role on your subscription.

##### ARM_CLIENT_ID

| Property | Value |
|----------|-------|
| Required | Yes |
| Type | UUID (Service Principal App ID) |
| Used By | Azure Terraform provider (azurerm) |
| Security Level | High |
| Rotation | Quarterly (recommended) |

**Description**: The Application ID of the Service Principal used by Terraform. This is distinct from the OIDC Client ID and authenticates Terraform specifically.

**How to Obtain**:

```powershell
# Create new service principal
az ad sp create-for-rbac --name "consilient-terraform" \
  --role Contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>

# Output includes appId (this value)

# Or list existing service principals
az ad sp list --display-name "consilient-terraform" --query '[0].appId' -o tsv
```

**Example Format**:
```
bbbbbbbb-cccc-dddd-eeee-ffffffffffff
```

**Security Notes**:
- Different from AZURE_CLIENT_ID (this is for Terraform, not GitHub login)
- Service Principal should have Contributor role
- Should be rotated quarterly

**Related Secrets**: ARM_CLIENT_SECRET, ARM_TENANT_ID

---

##### ARM_CLIENT_SECRET

| Property | Value |
|----------|-------|
| Required | Yes |
| Type | String (password) |
| Used By | Azure Terraform provider (azurerm) |
| Security Level | Critical |
| Rotation | Quarterly (recommended) |

**Description**: The password for the Terraform Service Principal. This is a long-lived credential that authenticates Terraform to Azure.

**How to Obtain**:

```powershell
# When creating service principal, the output shows password:
az ad sp create-for-rbac --name "consilient-terraform" \
  --role Contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>

# Output:
# {
#   "appId": "...",
#   "password": "YOUR_SECRET_HERE",
#   "tenant": "..."
# }

# If you lost the password, create a new credential:
az ad sp credential reset --id <appId> --cert

# Or as password:
az ad sp credential reset --id <appId>
```

**Security Notes**:
- **Critical**: This authenticates Terraform with full infrastructure permissions
- Never share or expose in logs
- Must be rotated quarterly minimum
- If compromised, immediately rotate: `az ad sp credential reset`
- Use strong password (20+ characters, mixed case, numbers, symbols)

**Workflow Usage**:
```yaml
env:
  ARM_CLIENT_ID: ${{ secrets.ARM_CLIENT_ID }}
  ARM_CLIENT_SECRET: ${{ secrets.ARM_CLIENT_SECRET }}
  ARM_TENANT_ID: ${{ secrets.ARM_TENANT_ID }}
```

---

##### ARM_TENANT_ID

| Property | Value |
|----------|-------|
| Required | Yes |
| Type | UUID (Tenant ID) |
| Used By | Azure Terraform provider (azurerm) |
| Security Level | High |
| Rotation | Never (unless org changes) |

**Description**: The Azure Entra ID Tenant ID for the Service Principal. Same as AZURE_TENANT_ID.

**How to Obtain**:

```powershell
# When creating service principal:
az ad sp create-for-rbac --name "consilient-terraform" \
  --role Contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>

# Or query existing:
az account show --query tenantId -o tsv
```

**Security Notes**:
- Same as AZURE_TENANT_ID
- Not a secret itself, but required for authentication
- Identifies your Azure organization

**Related Secrets**: ARM_CLIENT_ID, ARM_CLIENT_SECRET

---

#### Database Secrets

These secrets authenticate to SQL Server for database deployment and management.

##### SQL_ADMIN_USERNAME

| Property | Value |
|----------|-------|
| Required | Yes |
| Type | String (SQL login) |
| Used By | databases.yml workflow, Terraform |
| Security Level | High |
| Rotation | Annual (recommended) |

**Description**: The SQL Server administrator username. Used to create and manage databases.

**How to Set**:

```powershell
# Default convention is "sqladmin" or "dbadmin"
# Examples: sqladmin, dba, dbadmin
# Requirements: 1-128 characters, alphanumeric plus underscore
```

**Example Format**:
```
sqladmin
dbadmin
consilient_admin
```

**Security Notes**:
- Should follow your organization's naming conventions
- Separate from Azure AD authentication
- Username alone is not sensitive, but access should be restricted
- Consider rotating annually

**Related Secrets**: SQL_ADMIN_PASSWORD

**Terraform Usage**:
```hcl
# Set via environment variable
export TF_VAR_sql_admin_username="sqladmin"

# Or in terraform.tfvars (not recommended)
sql_admin_username = "sqladmin"
```

---

##### SQL_ADMIN_PASSWORD

| Property | Value |
|----------|-------|
| Required | Yes |
| Type | String (strong password) |
| Used By | databases.yml workflow, Terraform, Key Vault |
| Security Level | Critical |
| Rotation | Quarterly (recommended) |

**Description**: The SQL Server administrator password. Enables database management and is stored in Azure Key Vault.

**How to Set**:

```powershell
# GitHub Secret configuration
# Settings → Secrets and Variables → Actions → New repository secret
# Name: SQL_ADMIN_PASSWORD
# Value: [Generate strong password]

# Strong password requirements:
# - Minimum 8 characters (SQL Server minimum)
# - Recommended 12+ characters for security
# - Must contain: uppercase, lowercase, numbers, special characters
# - Examples:
#   - P@ssw0rd!SecureABC
#   - MyDb$Admin2024!Secure
#   - C0nsi1ent!SqlAdm1n
```

**Security Notes**:
- **Critical**: Controls access to all databases
- Never use simple passwords or dictionary words
- Never log or expose in CI/CD output
- Automatically masked in GitHub Actions logs
- Stored in Azure Key Vault for runtime access
- Rotate quarterly minimum
- If compromised, change immediately and update in Key Vault

**Terraform Usage** (insecure - for dev only):
```hcl
# DO NOT commit this to source control
sql_admin_password = "P@ssw0rd!SecureABC"
```

**Secure Terraform Usage** (recommended):
```bash
# Set as environment variable
export TF_VAR_sql_admin_password="P@ssw0rd!SecureABC"

# Then run terraform
terraform apply
```

**In GitHub Actions** (secure):
```yaml
env:
  TF_VAR_sql_admin_password: ${{ secrets.SQL_ADMIN_PASSWORD }}
```

---

#### Application Secrets

These secrets are used by the application for authentication and external integrations.

##### JWT_SIGNING_SECRET

| Property | Value |
|----------|-------|
| Required | Yes |
| Type | String (base64 encoded, 64+ bytes) |
| Used By | Terraform, Azure Key Vault, Application (API) |
| Security Level | Critical |
| Rotation | Annual or when access revoked |

**Description**: The secret key used to sign and verify JWT authentication tokens. Used by the API to generate secure session tokens.

**How to Set**:

```powershell
# Generate using OpenSSL
openssl rand -base64 64

# Example output:
# aGVsbG8gd29ybGQgZm9vIGJhciEgdGhpcyBpcyBhIHRlc3QhISEh

# Or using .NET
[Convert]::ToBase64String((1..64 | ForEach-Object {[byte]$_}))

# Store in GitHub secret:
# Settings → Secrets and Variables → Actions → New repository secret
# Name: JWT_SIGNING_SECRET
# Value: [paste generated secret]
```

**Security Notes**:
- **Critical**: Secures user sessions and identity tokens
- Must be cryptographically random (use openssl or equivalent)
- Must be at least 64 bytes (base64 encoded)
- Never hardcode or reuse across environments
- Stored in Azure Key Vault (secure)
- Rotation breaks existing tokens (forces re-authentication)
- If compromised, rotate immediately and invalidate all existing tokens

**Terraform Usage**:
```bash
export TF_VAR_jwt_signing_secret="aGVsbG8gd29ybGQgZm9vIGJhciEgdGhpcyBpcyBhIHRlc3QhISEh"
```

**Application Usage** (via Key Vault):
```csharp
// Retrieved from Azure Key Vault
var secret = await keyVaultClient.GetSecretAsync(vaultBaseUrl, "jwt-signing-secret");
var jwtSecret = secret.Value;
```

---

##### OAUTH_CLIENT_SECRET

| Property | Value |
|----------|-------|
| Required | No (conditional) |
| Type | String |
| Used By | Terraform, Azure Key Vault (if OAuth enabled) |
| Security Level | Critical |
| Rotation | Quarterly or per OAuth provider |

**Description**: OAuth provider client secret for external identity providers (optional). Only required if OAuth authentication is enabled.

**How to Obtain** (from your OAuth provider):

```powershell
# From Azure Entra ID:
az ad app show --id <app-id> --query "appRoles" -o json

# From third-party OAuth provider:
# 1. Go to provider's developer console
# 2. Navigate to application settings
# 3. Copy "Client Secret" or "App Secret"
```

**Setting in GitHub**:

```powershell
# If OAuth is NOT enabled (default):
# Create secret with empty value or don't create at all
# Name: OAUTH_CLIENT_SECRET
# Value: (empty string)

# If OAuth IS enabled:
# Paste the client secret from your OAuth provider
```

**Security Notes**:
- Optional: only needed if OAuth is configured
- **If used: Critical** - secures external identity integration
- Keep separate from JWT signing secret
- Rotate according to OAuth provider recommendations
- If compromised, regenerate in OAuth provider dashboard

**Terraform Usage**:
```bash
# If OAuth not enabled:
export TF_VAR_oauth_client_secret=""

# If OAuth enabled:
export TF_VAR_oauth_client_secret="your-oauth-secret-here"
```

---

#### Legacy/Optional Secrets

These secrets are legacy or optional and may not be needed for current deployments.

##### AZURE_CREDENTIALS

| Property | Value |
|----------|-------|
| Required | No |
| Type | JSON (Service Principal credentials) |
| Used By | act (local GitHub Actions testing) |
| Security Level | High |
| Rotation | Quarterly |

**Description**: Service Principal credentials in JSON format. Used only for local testing with the `act` tool, not for cloud deployment.

**How to Generate**:

```powershell
# Create a service principal for local testing
az ad sp create-for-rbac --name "consilient-act-local" \
  --role Contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>

# Format the output as JSON:
{
  "clientId": "appId-from-above",
  "clientSecret": "password-from-above",
  "subscriptionId": "your-subscription-id",
  "tenantId": "your-tenant-id"
}
```

**Storage** (for local use):

```bash
# Store in infra/act/.env.act file
# Never commit to Git
AZURE_CREDENTIALS={
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "...",
  "tenantId": "..."
}
```

**Security Notes**:
- **Important**: Only for local testing with `act`
- Should not be committed to Git
- Use a separate service principal (not production)
- Rotate quarterly
- Never use in cloud CI/CD (use OIDC instead)

**Related Documentation**: [components/local-testing.md](../components/local-testing.md)

---

##### AZURE_API_WEBAPP_NAME

| Property | Value |
|----------|-------|
| Required | No |
| Type | String (App Service name) |
| Used By | dotnet_apps.yml (fallback) |
| Security Level | Low |
| Rotation | Never |

**Description**: Azure App Service name for the .NET API. Legacy fallback secret; not needed if using workflow outputs.

**Format**: `consilient-api-dev`, `consilient-api-prod`

**Notes**: Prefer using Terraform outputs instead.

---

##### AZURE_WEBAPP_NAME

| Property | Value |
|----------|-------|
| Required | No |
| Type | String (App Service name) |
| Used By | react_apps.yml (fallback) |
| Security Level | Low |
| Rotation | Never |

**Description**: Azure App Service name for React frontend. Legacy fallback; prefer Terraform outputs.

**Format**: `consilient-react-dev`, `consilient-react-prod`

---

##### GITHUB_TOKEN

| Property | Value |
|----------|-------|
| Required | Auto-provided |
| Type | String (JWT token) |
| Used By | Container registry authentication |
| Security Level | Medium |
| Rotation | Auto (per job) |

**Description**: GitHub automatically provides a token for each workflow run. No manual configuration needed.

**Auto-provided By**: GitHub Actions runtime

**Used For**:
- Authenticating to GitHub Container Registry (ghcr.io)
- Publishing Docker images
- Creating releases and artifacts

**Security Notes**:
- Automatically scoped to the current repository
- Expires at end of job execution
- Can be accessed via `${{ secrets.GITHUB_TOKEN }}`
- No action needed; GitHub handles automatically

---

### GitHub Variables

GitHub Variables are non-sensitive configuration stored at the repository or environment level. Unlike secrets, they are visible in logs.

> **Security**: Use variables only for non-sensitive values (defaults, URLs, settings). Never use for secrets, passwords, or API keys.

#### Container Registry Variables

##### CONTAINER_REGISTRY

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | ghcr.io |
| Used By | build-runner-image.yml, workflows |
| Required | No |

**Description**: Container registry URL for published images. Defaults to GitHub Container Registry.

**Usage**: `ghcr.io/<owner>/<repo>:tag`

**Values**:
```
ghcr.io                              # GitHub Container Registry (default)
<registry>.azurecr.io                # Azure Container Registry
docker.io                            # Docker Hub
```

**When to Customize**: If using Azure Container Registry or other private registry.

---

##### ACR_REGISTRY_URL

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | Not set |
| Used By | dotnet_apps.yml, react_apps.yml |
| Required | If using ACR |

**Description**: Azure Container Registry URL for pulling images.

**Format**: `<registry-name>.azurecr.io`

**Example**:
```
consilientacr.azurecr.io
```

**When to Set**: Only if using Azure Container Registry for image storage.

---

##### API_IMAGE_NAME

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | consilient-api |
| Used By | dotnet_apps.yml |
| Required | No |

**Description**: Docker image name for the .NET API service.

**Format**: `<registry>/<owner>/<image-name>:tag`

**Example**:
```
ghcr.io/consilient/consilient-api:latest
```

**When to Customize**: To use different naming conventions or registries.

---

##### REACT_IMAGE_NAME

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | consilient-react |
| Used By | react_apps.yml |
| Required | No |

**Description**: Docker image name for the React frontend service.

**Format**: `<registry>/<owner>/<image-name>:tag`

**Example**:
```
ghcr.io/consilient/consilient-react:latest
```

---

##### ACTIONS_RUNNER_IMAGE

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | consilient-runner:latest |
| Used By | build-runner-image.yml |
| Required | No |

**Description**: Custom GitHub Actions runner image name. Used for building the runner image locally.

**When to Customize**: If building custom actions runner image for advanced requirements.

---

#### Database Variables

##### AZURE_SQL_SERVER_FQDN

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | Not set |
| Used By | databases.yml |
| Required | Yes |

**Description**: Fully Qualified Domain Name of Azure SQL Server.

**Format**: `<server-name>.database.windows.net`

**Example**:
```
consilient-sql-dev.database.windows.net
consilient-sql-prod.database.windows.net
```

**How to Find**:
```powershell
# Get SQL Server FQDN
az sql server show --resource-group <rg> --name <server-name> \
  --query fullyQualifiedDomainName -o tsv
```

---

##### SCHEMASPY_VERSION

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | 6.2.4 |
| Used By | docs_db.yml |
| Required | No |

**Description**: SchemaSpy version for database documentation generation.

**Current Default**: `6.2.4`

**When to Update**: To use a newer version of SchemaSpy for database docs.

---

##### JDBC_DRIVER_VERSION

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | 12.4.2.jre11 |
| Used By | docs_db.yml |
| Required | No |

**Description**: JDBC driver version for SQL Server database documentation.

**Current Default**: `12.4.2.jre11` (SQL Server JDBC driver for Java 11)

**When to Update**: To use a newer driver version or Java version.

---

#### Infrastructure Variables

##### AZURE_REGION

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | canadacentral |
| Used By | terraform.yml |
| Required | No |

**Description**: Azure region for resource deployment.

**Valid Values** (common):
```
canadacentral      # Canada (Central)
eastus             # US (East)
westus             # US (West)
northeurope        # Europe (North)
westeurope         # Europe (West)
eastasia           # Asia (East)
southeastasia      # Asia (Southeast)
```

**Cost Impact**: Varies by region; eastus is typically cheaper.

**When to Customize**: Based on data residency requirements or latency.

---

##### AZURE_RESOURCE_GROUP_NAME

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | consilient-resource-group |
| Used By | terraform.yml |
| Required | No |

**Description**: Azure Resource Group name. Containers all resources for the project.

**Naming Convention**: `consilient-<environment>-rg`

**Examples**:
```
consilient-dev-rg
consilient-prod-rg
consilient-resource-group
```

**When to Customize**: To organize resources by project or environment.

---

##### TERRAFORM_VERSION

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | 1.6.0 |
| Used By | terraform.yml |
| Required | No |

**Description**: Terraform version to use in CI/CD workflows.

**Current Default**: `1.6.0`

**When to Update**:
- For new Terraform features
- For security patches
- For provider compatibility

**How to Check Latest**:
```powershell
terraform version
```

---

#### Container App Environment Variables

##### CAE_CREATE_NEW

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | false |
| Used By | terraform.yml |
| Required | No |

**Description**: Whether to create a new Container App Environment or use existing.

**Set to `true`**: To create new CAE for this deployment

**Set to `false`**: To use existing shared or external CAE

---

##### CAE_NAME_TEMPLATE

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | Not typically set |
| Used By | terraform.yml |
| Required | No |

**Description**: Template for Container App Environment naming with {environment} placeholder.

**Format**: `consilient-cae-{environment}`

**Expands to**:
```
consilient-cae-dev    # For dev environment
consilient-cae-prod   # For prod environment
```

---

#### Terraform State Backend Variables

##### TF_STATE_STORAGE_ACCOUNT

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | Not set |
| Used By | terraform.yml |
| Required | Yes |

**Description**: Azure Storage account name for storing Terraform state files.

**Format**: `<project>tfstate` or `<project>-tfstate`

**Example**:
```
consilienttfstate
consilient-tfstate
```

**When to Set**: Required for remote state backend configuration.

---

##### TF_STATE_CONTAINER

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | tfstate |
| Used By | terraform.yml |
| Required | No |

**Description**: Blob container name within the storage account for state files.

**Format**: Lowercase alphanumeric and hyphens

**Common Values**:
```
tfstate           # Standard container for all state files
tfstate-dev       # Separate containers per environment
tfstate-prod
```

**When to Customize**: To organize state files in separate containers by environment (optional).

---

##### TF_STATE_RESOURCE_GROUP

| Property | Value |
|----------|-------|
| Scope | Repository |
| Default | consilient-terraform |
| Used By | terraform.yml |
| Required | Yes |

**Description**: Azure Resource Group name containing the Terraform state storage account (separate from application resources).

**Format**: `<project>-terraform` or `<project>-tfstate-rg`

**Examples**:
```
consilient-terraform
consilient-tfstate-rg
terraform-state
```

**Important**: This is the resource group where the state storage account exists, NOT the application resource group.

**When to Customize**: If using a different resource group name for state management infrastructure.

---

## Terraform Configuration

### Root Module Variables

Terraform variables are defined in `c:\Work\ConsilientWebApp\infra\terraform\variables.tf` and configured via `terraform.tfvars` or environment variables.

#### Global Configuration Variables

##### project_name

```hcl
variable "project_name" {
  description = "Project name used as prefix for all resources"
  type        = string
  default     = "consilient"
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | consilient |
| Type | string |
| Sensitive | No |

**Usage**: Prefix for all Azure resource names

**Examples**:
```hcl
project_name = "consilient"
project_name = "my-project"
```

**Setting**:
```hcl
# In terraform.tfvars
project_name = "consilient"

# Or via environment variable
export TF_VAR_project_name="consilient"
```

---

##### environment

```hcl
variable "environment" {
  description = "Deployment environment (e.g., dev, staging, prod)."
  type        = string

  validation {
    condition     = contains(["dev", "prod"], var.environment)
    error_message = "Environment must be either 'dev' or 'prod'."
  }
}
```

| Property | Value |
|----------|-------|
| Required | Yes |
| Default | None |
| Type | string |
| Sensitive | No |
| Validation | Must be "dev" or "prod" |

**Description**: Deployment environment. Controls resource SKUs, security features, and cost profiles.

**Valid Values**: `dev` or `prod`

**Impact by Environment**:
- **dev**: Lower-cost SKUs (B1, Basic), limited redundancy, development features
- **prod**: Higher-performance SKUs (P2v3, GP_Gen5), zone redundancy, audit logging

**Setting**:
```hcl
# In terraform.tfvars
environment = "dev"

# Or via environment variable
export TF_VAR_environment="prod"
```

**Cost Impact**:
```
dev:  ~$45/month
prod: ~$2,800/month
```

---

##### region

```hcl
variable "region" {
  description = "Azure region to deploy resources."
  type        = string
}
```

| Property | Value |
|----------|-------|
| Required | Yes |
| Default | None |
| Type | string |
| Sensitive | No |

**Description**: Azure region for all resource deployment.

**Valid Regions** (common):
```
canadacentral    # Canada (Central) - Default
eastus           # US (East) - Often cheaper
westus           # US (West)
northeurope      # Europe (North)
westeurope       # Europe (West)
eastasia         # Asia (East)
```

**Cost Implications**: Varies by region; eastus typically 10-20% cheaper than canadacentral

**Data Residency**: Choose region based on:
- Where your data must reside
- Latency requirements
- Compliance requirements (HIPAA requires specific regions)

**Setting**:
```hcl
# In terraform.tfvars
region = "canadacentral"

# Or via environment variable
export TF_VAR_region="eastus"
```

---

##### resource_group_name

```hcl
variable "resource_group_name" {
  description = "Name of the resource group."
  type        = string
}
```

| Property | Value |
|----------|-------|
| Required | Yes |
| Default | None |
| Type | string |
| Sensitive | No |

**Description**: Azure Resource Group name. Containers all resources for the project.

**Naming Convention**:
```
consilient-<environment>-rg
consilient-resource-group
<project>-<environment>-rg
```

**Examples**:
```hcl
resource_group_name = "consilient-dev-rg"
resource_group_name = "consilient-prod-rg"
resource_group_name = "consilient-resource-group"
```

**Setting**:
```hcl
# In terraform.tfvars
resource_group_name = "consilient-dev-rg"

# Or via environment variable
export TF_VAR_resource_group_name="consilient-prod-rg"
```

**Note**: Resource group must not already exist; Terraform will create it.

---

##### subscription_id

```hcl
variable "subscription_id" {
  description = "Azure Subscription ID"
  type        = string
}
```

| Property | Value |
|----------|-------|
| Required | Yes |
| Default | None |
| Type | string |
| Sensitive | No |

**Description**: Your Azure Subscription ID where resources will be billed.

**Format**: UUID `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`

**Example**:
```
0e49a8db-e270-49d4-afb2-bab7d5b8357f
```

**How to Find**:
```powershell
az account show --query id -o tsv
```

**Security Warning** ⚠️:
- Subscription ID identifies your Azure account financially
- Should not be committed to source control (but currently is in terraform.tfvars)
- Recommend using environment variable instead

**Setting** (Secure):
```bash
# Use environment variable
export TF_VAR_subscription_id="0e49a8db-e270-49d4-afb2-bab7d5b8357f"
terraform apply
```

**Setting** (Not Recommended):
```hcl
# terraform.tfvars (SECURITY RISK)
subscription_id = "0e49a8db-e270-49d4-afb2-bab7d5b8357f"
```

---

#### Database Variables

##### sql_admin_username

```hcl
variable "sql_admin_username" {
  description = "SQL Server administrator username."
  type        = string
}
```

| Property | Value |
|----------|-------|
| Required | Yes |
| Default | None |
| Type | string |
| Sensitive | No |

**Description**: SQL Server administrator login username.

**Requirements**:
- 1-128 characters
- Alphanumeric plus underscore
- Cannot be reserved words (admin, root, sa, etc.)

**Common Values**:
```
sqladmin
dbadmin
dba
consilient_admin
```

**Setting**:
```hcl
# In terraform.tfvars
sql_admin_username = "sqladmin"

# Or via environment variable
export TF_VAR_sql_admin_username="dbadmin"
```

**Related**: sql_admin_password

---

##### sql_admin_password

```hcl
variable "sql_admin_password" {
  description = <<EOT
SQL Server administrator password.
Never set a default or commit this value to source control.
Inject securely using environment variables (TF_VAR_sql_admin_password), a secret manager, or your CI/CD pipeline.
EOT
  type        = string
  sensitive   = true
}
```

| Property | Value |
|----------|-------|
| Required | Yes |
| Default | None (intentionally) |
| Type | string |
| Sensitive | **Yes** |

**Description**: SQL Server administrator password. Marked as sensitive to hide from Terraform output.

**Requirements**:
- Minimum 8 characters (SQL Server minimum)
- Minimum 12 characters (recommended for security)
- Must contain: uppercase, lowercase, numbers, special characters
- Cannot contain username

**Strong Password Examples**:
```
P@ssw0rd!SecureABC
MyDb$Admin2024!Secure
C0nsi1ent!SqlAdm1n
```

**Security Practices** ⚠️:
- Never hardcode in terraform.tfvars
- Never log or expose in CI/CD
- Use environment variables or secrets manager
- Rotate quarterly minimum
- If compromised, rotate immediately

**Setting** (Secure - Recommended):
```bash
# Set environment variable
export TF_VAR_sql_admin_password="P@ssw0rd!SecureABC"

# Then run terraform
terraform apply
```

**In GitHub Actions** (Secure):
```yaml
env:
  TF_VAR_sql_admin_password: ${{ secrets.SQL_ADMIN_PASSWORD }}
```

**Insecure** (Development Only):
```hcl
# terraform.tfvars - NEVER in production
sql_admin_password = "P@ssw0rd!SecureABC"
```

**Current Issue** 🔴:
The file `terraform.tfvars` currently contains:
```hcl
sql_admin_password = "YourSecureP@ssw0rd!"
```
This is a security risk. See [Security Best Practices](#security-best-practices).

---

#### Application Secret Variables

##### jwt_signing_secret

```hcl
variable "jwt_signing_secret" {
  description = <<EOT
JWT signing secret for authentication tokens.
SECURITY: Never set a default or commit this value to source control.
Inject securely using environment variables (TF_VAR_jwt_signing_secret), a secret manager, or your CI/CD pipeline.
Generate using: openssl rand -base64 64
EOT
  type        = string
  sensitive   = true
}
```

| Property | Value |
|----------|-------|
| Required | Yes |
| Default | None (intentionally) |
| Type | string |
| Sensitive | **Yes** |

**Description**: Secret key for signing JWT authentication tokens. Critical for application security.

**How to Generate**:

```powershell
# Using OpenSSL
openssl rand -base64 64

# Output example:
# aGVsbG8gd29ybGQgZm9vIGJhciEgdGhpcyBpcyBhIHRlc3QhISEh

# Using .NET
[Convert]::ToBase64String((1..64 | ForEach-Object {[byte](Get-Random -Maximum 256)}))
```

**Format**: Base64-encoded cryptographic random string (64+ bytes)

**Security Practices** ⚠️:
- **Critical**: Controls user authentication
- Must be cryptographically random
- Never hardcode or reuse
- Different secret per environment (dev vs prod)
- Stored in Azure Key Vault (secure)
- Rotation invalidates existing sessions
- If compromised, rotate immediately and invalidate sessions

**Setting** (Secure):
```bash
# Generate secret
SECRET=$(openssl rand -base64 64)
echo "Generated: $SECRET"

# Store in GitHub secret
# Settings → Secrets and Variables → Actions → JWT_SIGNING_SECRET

# Set environment variable
export TF_VAR_jwt_signing_secret="$SECRET"

# Deploy
terraform apply
```

**In GitHub Actions**:
```yaml
env:
  TF_VAR_jwt_signing_secret: ${{ secrets.JWT_SIGNING_SECRET }}
```

**Application Usage** (via Key Vault):
```csharp
// Configuration in appsettings.json references Key Vault
"Authentication": {
  "UserService": {
    "Jwt": {
      "Secret": "@Microsoft.KeyVault(SecretUri=https://vault-name.vault.azure.net/secrets/jwt-signing-secret/)"
    }
  }
}
```

---

##### oauth_client_secret

```hcl
variable "oauth_client_secret" {
  description = <<EOT
OAuth provider client secret (if OAuth is enabled).
SECURITY: Never set a default or commit this value to source control.
Set to empty string "" if OAuth is not used.
EOT
  type        = string
  sensitive   = true
  default     = ""
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | "" (empty string) |
| Type | string |
| Sensitive | **Yes** |

**Description**: OAuth provider client secret for external identity integration. Optional; only needed if OAuth is enabled.

**When Required**:
- Using Azure Entra ID for OAuth
- Using third-party OAuth provider (Google, GitHub, etc.)
- Multi-tenant authentication

**When Not Required**:
- Using only local authentication
- Using managed identity only

**How to Obtain**:

```powershell
# From Azure Entra ID
az ad app credential list --id <app-id>

# From GitHub OAuth Apps
# Settings → Developer settings → OAuth Apps → Client Secret

# From Google Cloud Console
# APIs & Services → Credentials → OAuth 2.0 Client IDs
```

**Setting** (When Disabled):
```bash
# If OAuth not used, set empty string
export TF_VAR_oauth_client_secret=""
```

**Setting** (When Enabled):
```bash
# Store OAuth secret in GitHub
# Settings → Secrets and Variables → OAUTH_CLIENT_SECRET

# Set environment variable
export TF_VAR_oauth_client_secret="your-oauth-secret"

# Deploy
terraform apply
```

**Security Notes** ⚠️:
- If used: **Critical** - controls external identity access
- Keep separate from JWT signing secret
- Rotate according to OAuth provider recommendations
- If compromised, regenerate in provider dashboard immediately

---

#### Monitoring Variables

##### loki_retention

```hcl
variable "loki_retention" {
  description = "Loki log retention period (e.g., 7d, 30d)."
  type        = string
  default     = "30d"
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | 30d |
| Type | string |
| Sensitive | No |

**Description**: How long logs are retained in Loki (logging system).

**Valid Values**:
```
7d      # 7 days (minimal retention)
14d     # 14 days
30d     # 30 days (default)
90d     # 90 days
365d    # 1 year (expensive)
```

**Cost Impact**: Storage increases with retention time

**Common Usage**:
- **Dev**: 7d (to reduce costs)
- **Prod**: 30d (compliance and debugging)

**Setting**:
```hcl
# Development (cheaper)
loki_retention = "7d"

# Production (compliance)
loki_retention = "30d"
```

---

##### loki_cpu_request

```hcl
variable "loki_cpu_request" {
  description = "Loki container CPU request - must match Azure Container Apps valid combinations"
  type        = number
  default     = 0.5
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | 0.5 |
| Type | number |
| Sensitive | No |

**Description**: CPU request for Loki container in Container Apps.

**Valid Values** (must match Azure Container Apps):
```
0.25
0.5 (default)
1.0
```

**Paired Requirements** (Azure Container Apps combinations):
- 0.25 CPU → 0.5 Gi memory
- 0.5 CPU → 1.0 Gi memory (default)
- 1.0 CPU → 1.5-2.0 Gi memory

**Setting**:
```hcl
loki_cpu_request = 0.5
```

**Related Variables**: loki_cpu_limit, loki_memory_request

---

##### loki_cpu_limit

```hcl
variable "loki_cpu_limit" {
  description = "Loki container CPU limit (not used in Azure Container Apps)"
  type        = number
  default     = 1.0
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | 1.0 |
| Type | number |
| Sensitive | No |

**Note**: This variable is informational only; Azure Container Apps doesn't enforce separate limits.

---

##### loki_memory_request

```hcl
variable "loki_memory_request" {
  description = "Loki container memory request - must be 1.0Gi for 0.5 CPU (Azure Container Apps requirement)"
  type        = string
  default     = "1.0Gi"
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | 1.0Gi |
| Type | string |
| Sensitive | No |

**Description**: Memory request for Loki container.

**Valid Values** (per CPU):
```
0.25 CPU → 0.5Gi
0.5 CPU  → 1.0Gi (default)
1.0 CPU  → 1.5Gi or 2.0Gi
```

**Setting**:
```hcl
loki_memory_request = "1.0Gi"
```

---

##### loki_memory_limit

```hcl
variable "loki_memory_limit" {
  description = "Loki container memory limit (not used in Azure Container Apps)"
  type        = string
  default     = "2.0Gi"
}
```

**Note**: Informational only for Azure Container Apps.

---

##### grafana_major_version

```hcl
variable "grafana_major_version" {
  description = "Grafana major version."
  type        = number
  default     = 11
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | 11 |
| Type | number |
| Sensitive | No |

**Description**: Major version of Grafana for monitoring dashboards.

**Valid Values**:
```
9      # Grafana 9.x
10     # Grafana 10.x
11     # Grafana 11.x (current default)
```

**When to Update**: For major Grafana upgrades (usually yearly)

**Setting**:
```hcl
grafana_major_version = 11
```

---

#### Container App Environment Variables

##### create_container_app_environment

```hcl
variable "create_container_app_environment" {
  description = "Whether to create a new Container App Environment or use an existing one"
  type        = bool
  default     = true
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | true |
| Type | bool |
| Sensitive | No |

**Description**: Whether to create a new Container App Environment (CAE) or use an existing one.

**Set to `true`**: Create new CAE for this deployment

**Set to `false`**: Use existing CAE (specify ID in existing_container_app_environment_id)

**Setting**:
```hcl
# Create new
create_container_app_environment = true

# Use existing
create_container_app_environment = false
existing_container_app_environment_id = "/subscriptions/.../resourceGroups/.../providers/Microsoft.App/managedEnvironments/existing-cae"
```

---

##### container_app_environment_name_template

```hcl
variable "container_app_environment_name_template" {
  description = <<EOT
Template for Container App Environment name with placeholder substitution.
Supports {environment} placeholder which will be replaced with the actual environment value.
Examples:
  - "consilient-cae-{environment}" → "consilient-cae-dev" for dev environment
  - "my-cae-{environment}" → "my-cae-prod" for prod environment
EOT
  type        = string
  default     = "consilient-cae-{environment}"

  validation {
    condition     = can(regex("^[a-z0-9]([a-z0-9-]{0,58}[a-z0-9])?$", replace(var.container_app_environment_name_template, "{environment}", "dev")))
    error_message = "Container App Environment name template must result in a valid Azure resource name (lowercase alphanumeric and hyphens, 1-60 chars, start/end with alphanumeric)."
  }
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | consilient-cae-{environment} |
| Type | string |
| Sensitive | No |
| Validation | Must result in valid Azure resource name |

**Description**: Template for Container App Environment names with {environment} placeholder substitution.

**Placeholder**: `{environment}` - replaced with actual environment value (dev or prod)

**Expansion**:
```
Template: consilient-cae-{environment}
Dev:      consilient-cae-dev
Prod:     consilient-cae-prod
```

**Valid Examples**:
```
consilient-cae-{environment}
my-cae-{environment}
env-{environment}
```

**Setting**:
```hcl
container_app_environment_name_template = "consilient-cae-{environment}"
```

---

##### existing_container_app_environment_id

```hcl
variable "existing_container_app_environment_id" {
  description = "ID of existing Container App Environment (only used if create_container_app_environment is false)"
  type        = string
  default     = ""
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | "" (empty) |
| Type | string |
| Sensitive | No |

**Description**: Resource ID of an existing Container App Environment to use instead of creating new.

**Format**: Azure resource ID
```
/subscriptions/<subscription-id>/resourceGroups/<resource-group>/providers/Microsoft.App/managedEnvironments/<cae-name>
```

**How to Find**:

```powershell
# List existing CAEs
az containerapp env list --query "[].id" -o tsv

# Get specific CAE ID
az containerapp env show \
  --resource-group <rg> \
  --name <cae-name> \
  --query id -o tsv
```

**Setting** (Use Existing):
```hcl
create_container_app_environment = false
existing_container_app_environment_id = "/subscriptions/xxx/resourceGroups/my-rg/providers/Microsoft.App/managedEnvironments/existing-cae"
```

---

#### Security & Networking Variables

##### enable_local_firewall

```hcl
variable "enable_local_firewall" {
  description = <<EOT
Enable SQL Server firewall rule for local act testing.
WARNING: This opens SQL Server to all IPs (0.0.0.0/0) and enables public network access.
Only use for local development testing via act. Never enable in production.
EOT
  type        = bool
  default     = false
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | false |
| Type | bool |
| Sensitive | No |

**Description**: Whether to enable firewall rule allowing all IPs to SQL Server. WARNING: Security risk.

**Set to `true`**: Only for local testing with `act` tool

**Set to `false`**: Always in production (default and recommended)

**Security Impact** ⚠️:
- When true: Opens SQL Server to `0.0.0.0/0` (all IPs on internet)
- Enables public network access on SQL Server
- Allows anyone with credentials to connect
- **Never enable in production**

**Setting**:
```hcl
# Development (local testing only)
enable_local_firewall = true

# Production (must be)
enable_local_firewall = false
```

**Why It Exists**: The `act` tool for local GitHub Actions testing needs to reach SQL Server; this enables that for development.

---

#### Custom Domain Variables

##### api_custom_domain

```hcl
variable "api_custom_domain" {
  description = <<EOT
Custom domain name for the API App Service.
If provided, Azure will automatically issue and manage an SSL certificate for this domain.
The domain must be registered and its DNS must be configured to point to the App Service.
Example: "api.example.com"
EOT
  type        = string
  default     = ""
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | "" (empty) |
| Type | string |
| Sensitive | No |

**Description**: Custom domain for API App Service. Optional; if set, Azure manages SSL certificate.

**Prerequisites**:
- Domain registered and owned
- DNS configured to point to App Service

**How to Set Up**:

1. Purchase domain (GoDaddy, Azure, Route 53, etc.)
2. Point DNS to App Service IP:

```powershell
# Get App Service IP
az webapp show --name <api-app-name> --resource-group <rg> \
  --query defaultHostName -o tsv
# Example: consilient-api-dev.azurewebsites.net

# In your DNS provider: Create CNAME record
# CNAME: api → consilient-api-dev.azurewebsites.net
```

3. Set Terraform variable:

```hcl
api_custom_domain = "api.example.com"
```

4. Azure automatically issues SSL certificate (takes 1-2 hours)

**Setting**:
```hcl
# Without custom domain
api_custom_domain = ""

# With custom domain
api_custom_domain = "api.example.com"
api_custom_domain = "api.mycompany.com"
```

---

##### react_custom_domain

```hcl
variable "react_custom_domain" {
  description = <<EOT
Custom domain name for the React App Service.
If provided, Azure will automatically issue and manage an SSL certificate for this domain.
The domain must be registered and its DNS must be configured to point to the App Service.
Example: "app.example.com"
EOT
  type        = string
  default     = ""
}
```

| Property | Value |
|----------|-------|
| Required | No |
| Default | "" (empty) |
| Type | string |
| Sensitive | No |

**Description**: Custom domain for React App Service. Same setup as api_custom_domain.

**Setting**:
```hcl
react_custom_domain = "app.example.com"
```

---

### Environment-Based Configuration

Environment-specific configurations are defined in `c:\Work\ConsilientWebApp\infra\terraform\locals.tf`. These include SKU selections, costs, and naming patterns for dev vs prod environments.

#### Development Environment Profile (dev)

**App Service**: B1 tier (~$13/month)
- 1 vCPU, 1.75 GB RAM
- Suitable for light traffic
- No auto-scaling

**SQL Database**: Basic DTU (~$5/month)
- Limited DTU capacity
- No zone redundancy
- Basic security features

**Container Registry**: Basic tier (~$5/month)
- Single storage account
- No geo-replication

**Estimated Monthly Cost**: ~$45 USD

**Use Case**: Development, testing, staging environments

#### Production Environment Profile (prod)

**App Service**: P2v3 tier (~$204/month)
- 4 vCPU, 8 GB RAM
- Auto-scaling enabled
- High availability

**SQL Database**: GP_Gen5_4 tier (~$650/month)
- 4 vCores, 24 GB RAM
- Zone redundancy enabled
- Automatic backups
- Advanced security (threat detection, audit logging)

**Container Registry**: Premium tier (~$40/month)
- Geo-replication
- Advanced networking
- Advanced security

**Additional Features**:
- Backup retention: 365 days
- Audit logging: Enabled
- Threat protection: Enabled

**Estimated Monthly Cost**: ~$2,800 USD

**Use Case**: Production, customer-facing, compliance-required environments

#### Environment Comparison Table

| Feature | Dev | Prod |
|---------|-----|------|
| App Service SKU | B1 | P2v3 |
| App Service Cost | $13/mo | $204/mo |
| SQL SKU | Basic | GP_Gen5_4 |
| SQL Cost | $5/mo | $650/mo |
| Zone Redundancy | No | Yes |
| Backup Retention | 7 days | 365 days |
| Audit Logging | No | Yes |
| Threat Protection | No | Yes |
| Auto-scaling | No | Yes |
| **Total Est. Monthly** | **~$45** | **~$2,800** |

---

### Module Variables Overview

Terraform configurations are modularized for reusability. Detailed information about module variables can be found in the respective module directories.

#### App Service Module

**Location**: `c:\Work\ConsilientWebApp\infra\terraform\modules\app_service\`

**Variables**: 24 total

**Key Variables**:
- `plan_name`, `plan_tier`, `plan_size`, `sku_name` - Service plan configuration
- `app_name`, `app_settings`, `connection_strings` - Application configuration
- `container_registry_managed_identity_client_id` - ACR authentication
- `custom_domain_name`, `enable_https_only` - HTTPS and domain settings
- `health_check_path`, `health_check_eviction_time_in_min` - Availability monitoring
- `vnet_route_all_enabled` - Virtual network routing

**Used By**: Both API and React App Service deployments

**For Complete Details**: See [modules/app_service/variables.tf](../../../../../../infra/terraform/modules/app_service/variables.tf)

---

#### SQL Database Module

**Location**: `c:\Work\ConsilientWebApp\infra\terraform\modules\sql_database\`

**Variables**: 11 total

**Key Variables**:
- `name`, `sku_name` - Database name and tier
- `zone_redundant` - High availability
- `min_capacity`, `auto_pause_delay_in_minutes` - Serverless configuration

**Used By**: Main and Hangfire database deployments

**For Complete Details**: See [modules/sql_database/variables.tf](../../../../../../infra/terraform/modules/sql_database/variables.tf)

---

#### Storage Account Module

**Location**: `c:\Work\ConsilientWebApp\infra\terraform\modules\storage_account\`

**Variables**: 11 total

**Key Variables**:
- `name`, `account_tier`, `account_replication_type` - Storage configuration
- `public_network_access_enabled` - Network access
- `min_tls_version` - Security (TLS 1.2 minimum)
- `container_name`, `container_access_type` - Blob container configuration

**For Complete Details**: See [modules/storage_account/variables.tf](../../../../../../infra/terraform/modules/storage_account/variables.tf)

---

## Azure App Configuration

Azure App Configuration (AAC) is the single source of truth for all application runtime configuration. It acts as a centralized configuration store that bridges infrastructure (Terraform) and runtime (application) configuration needs.

### Configuration Management Philosophy: "Source of Creation" Rule

Configuration should be managed by whoever/whatever created it. This ensures clean separation of concerns and prevents conflicts.

#### Configuration Ownership Breakdown

| Config Type | Examples | Source of Creation | Who Writes It | Where It's Stored | Mechanism |
|---|---|---|---|---|---|
| **Infrastructure Outputs** | DB Connection String, Storage Key, Loki URL | Terraform | Terraform | Key Vault | Terraform creates resource → gets output → writes to KV → AAC references it |
| **3rd Party / Business Secrets** | Stripe API Key, SendGrid Token, OAuth secrets | Manual/Admin | Admin or CI Pipeline | Key Vault | Manually injected via GitHub Secrets → Terraform → Key Vault |
| **Application Runtime Settings** | Logging Level, Debug Mode, Feature Flags, UI Theme | Admin/CI | Admin or CI Pipeline | App Configuration | Managed in AAC Portal, CLI, or CI/CD pipeline |

#### Practical Application in ConsilientWebApp

**Terraform Writes (Infrastructure Outputs)**:
- ✅ SQL connection strings (Terraform creates DB → gets connection string → writes to Key Vault → referenced in AAC)
- ✅ Grafana Loki URL (Terraform creates Container App → gets FQDN → writes to Key Vault → referenced in AAC)
- ✅ Any dynamically generated resource outputs

**Admin/Manual Writes (Business/3rd Party)**:
- ✅ JWT Signing Secret (GitHub Secret → Terraform variable → Key Vault → referenced in AAC)
- ✅ OAuth Client Secret (GitHub Secret → Terraform variable → Key Vault → referenced in AAC)
- ✅ Any 3rd party API keys

**Admin/CI Writes (Runtime Configuration)**:
- ✅ Feature flags (logging level, debug mode, mock services) → stored directly in App Configuration
- ✅ UI themes and runtime behavior settings → stored in App Configuration
- ✅ Environment-specific values (dev debug=true, prod debug=false) → environment labels in AAC

#### Why This Matters

1. **Single Responsibility**: Each system manages what it creates
2. **Conflict Prevention**: Terraform doesn't fight with admins over runtime config values
3. **Lifecycle Alignment**: Infrastructure outputs stay in sync with infrastructure
4. **Flexibility**: Runtime config can change without triggering infrastructure redeploys
5. **Audit Trail**: Clear ownership of who changed what and when

### App Configuration Structure

#### Organization Pattern

App Configuration uses hierarchical keys with colons as delimiters (ASP.NET Core convention):

```
Api:Authentication:Jwt:Issuer = "https://consilient-api-dev.azurewebsites.net"
Api:Authentication:Jwt:Audience = "https://consilient-api-dev.azurewebsites.net"
Api:Authentication:Jwt:ExpiryMinutes = "60"
Api:Authentication:Jwt:Secret = "@Microsoft.KeyVault(...)"  # KV reference

Api:Logging:LogLevel:Default = "Debug"
Api:Logging:LogLevel:Microsoft.AspNetCore = "Warning"
Api:Logging:GrafanaLoki:Url = "@Microsoft.KeyVault(...)"   # KV reference

React:ApiBaseUrl = "https://consilient-api-dev.azurewebsites.net"
React:Environment = "development"
React:EnableDebugMode = "true"
React:UseMockServices = "false"
React:EnableExternalLoginMock = "true"

ConnectionStrings:DefaultConnection = "@Microsoft.KeyVault(...)"    # KV reference
ConnectionStrings:HangfireConnection = "@Microsoft.KeyVault(...)"   # KV reference
```

#### Environment Labels

App Configuration uses labels to support multiple environments from a single store:

| Label | Environment | Use Case |
|---|---|---|
| `dev` | Development | Feature flags enabled, debug mode on, lower security requirements |
| `prod` | Production | Conservative settings, debug mode off, enhanced security |
| (no label) | Shared | Values that apply across all environments |

**Example**: Same key with different values per environment
```
Key: React:EnableDebugMode
  dev  label: "true"
  prod label: "false"
```

### App Configuration Integration

#### In Terraform

Terraform reads React configuration from App Configuration and populates App Service environment variables:

```hcl
data "azurerm_app_configuration_keys" "react_config" {
  configuration_store_id = azurerm_app_configuration.main.id
  label                  = var.environment
  key                    = "React:*"
}

locals {
  # Transform React:ApiBaseUrl → APP_API_BASE_URL
  react_app_settings = {
    for item in data.azurerm_app_configuration_keys.react_config.items :
    replace(upper(replace(item.key, "React:", "APP_")), ":", "_") => item.value
  }
}
```

#### In API (C#/.NET)

API loads configuration from App Configuration using managed identity:

```csharp
var appConfigEndpoint = builder.Configuration["AppConfiguration:Endpoint"];
if (!string.IsNullOrEmpty(appConfigEndpoint))
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options
            .Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential())
            .Select(KeyFilter.Any, LabelFilter.Null)      // Load shared keys
            .Select(KeyFilter.Any, builder.Environment.EnvironmentName.ToLower())  // Load env-specific
            .ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()));
    });
}
```

**Result**: API automatically loads all Api:* keys and Key Vault references

#### In React Frontend

React continues using proven docker-entrypoint.sh pattern:
1. Terraform reads React:* keys from AAC
2. Terraform populates App Service APP_* environment variables
3. Container startup runs docker-entrypoint.sh (unchanged)
4. Script generates env.js with window.__ENV object
5. React loads configuration from env.js

**No frontend code changes required** - leverages existing reliable pattern.

### SKU and Pricing

| SKU | Free | Standard |
|---|---|---|
| **Cost** | $0/month | $36/month (~$1.20/day) |
| **Storage** | 10 MB | 1 GB |
| **Requests/day** | 1,000 | 30,000 |
| **SLA** | None | 99.9% |
| **Recommended Use** | Dev/test | Production |

#### SKU Selection

- **Dev environment**: Free tier (sufficient for development)
- **Prod environment**: Standard tier (required for SLA and request capacity)
- **Single AAC for multiple envs**: Use Standard tier with environment labels

### Key Vault References in App Configuration

App Configuration can reference secrets in Key Vault:

**Format**:
```
@Microsoft.KeyVault(VaultName=<vault-name>;SecretName=<secret-name>)
```

**Example**:
```
Api:Authentication:Jwt:Secret = "@Microsoft.KeyVault(VaultName=consilient-kv-prod;SecretName=jwt-signing-secret)"
```

**How it works**:
1. App Configuration stores the Key Vault reference (not the secret itself)
2. When App reads the configuration, App Configuration resolves the reference
3. App Configuration's managed identity authenticates to Key Vault
4. Secret value is returned transparently to application
5. Application never sees Key Vault reference - just the secret value

**Benefits**:
- Secrets never stored in App Configuration
- Centralized audit trail (both AAC and Key Vault)
- Managed identity handles authentication
- If Key Vault secret rotated, immediately reflected in App Configuration

### RBAC Permissions for App Configuration

| Principal | Role | Scope | Purpose |
|---|---|---|---|
| Terraform Service Principal | App Configuration Data Owner | AAC | Terraform deploys configuration |
| API App Service (Managed Identity) | App Configuration Data Reader | AAC | API reads runtime configuration at startup |
| App Configuration (Managed Identity) | Key Vault Secrets User | Key Vault | AAC resolves Key Vault references transparently |

### Configuration Refresh Strategy

#### Option 1: Startup Only (Default - Recommended)

Configuration loaded once at application startup. Changes require restart.

**Advantages**:
- Simple, reliable
- Works with Free tier
- Minimal AAC requests

**Disadvantages**:
- Configuration changes require app restart
- No dynamic updates

#### Option 2: Periodic Refresh with Sentinel (Optional)

Configuration checked periodically for changes via sentinel key pattern.

**How it works**:
```csharp
.ConfigureRefresh(refresh =>
{
    refresh.Register("Api:RefreshSentinel", refreshAll: true)
           .SetCacheExpiration(TimeSpan.FromMinutes(5));
})
```

1. API caches configuration for 5 minutes
2. Every 5 minutes, checks if `Api:RefreshSentinel` changed
3. If changed, refreshes ALL configuration
4. No restart needed

**Advantages**:
- Dynamic configuration updates without restart
- Sentinel key prevents unnecessary full refreshes
- Cached to minimize AAC requests

**Disadvantages**:
- Up to 5-minute delay for config changes
- Requires Standard tier (1000 req/day not enough)
- Slightly more complex

**Recommendation**: Start with Option 1 (startup only). Add Option 2 later if dynamic updates needed.

### Local Development

Developers can work locally without Azure App Configuration access:

```json
// appsettings.Development.json
{
  "AppConfiguration": {
    "Endpoint": ""  // Empty = skip AAC in local dev
  },
  "Api": {
    "Authentication": {
      "Enabled": false,
      "Jwt": {
        "Secret": "local-dev-secret-key-minimum-32-characters"
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Consilient_Dev;..."
  }
}
```

Configuration falls back to appsettings.Development.json when AAC endpoint is empty.

### Migration Path from Legacy Configuration

**Current State (Before AAC)**:
- App settings directly in Terraform
- Key Vault references in App Service settings
- GitHub variables for infrastructure config

**Transition Period**:
- App Service settings retain legacy values (backward compatibility)
- API code updated to try AAC first, fall back to Key Vault if unavailable
- No breaking changes to existing deployments

**Final State (After AAC)**:
- App Configuration: Single source of truth for runtime config
- Key Vault: Secrets only (referenced by AAC)
- App Service settings: Minimal (just AAC endpoint + Azure config)
- Application: Reads from AAC, uses managed identity

### Security Considerations

1. **Secrets Protection**: Secrets never stored in AAC itself; stored in Key Vault with references from AAC
2. **Managed Identity**: All access via managed identity; no credentials stored
3. **RBAC**: Principle of least privilege - readers can only read, not write
4. **Audit Trail**: All configuration changes tracked in AAC revision history
5. **Environment Isolation**: Labels ensure dev and prod configurations separate

---

## Azure Key Vault Secrets

Azure Key Vault stores sensitive runtime secrets that are injected into applications via managed identity.

### Key Vault Integration Overview

```
Terraform Execution
  ├─ Creates Key Vault
  ├─ Sets RBAC: Terraform → Secrets Officer
  └─ Populates Secrets (using Terraform variables)
         ↓
Application Deployment
  ├─ App Service created
  ├─ Managed Identity assigned
  ├─ RBAC: Managed Identity → Secrets User (read-only)
  └─ Key Vault references in App Settings
         ↓
Runtime
  ├─ App Service reads app setting with Key Vault reference
  ├─ Managed Identity authenticates to Key Vault
  └─ Secret value resolved and available to application
```

### Secrets Stored in Key Vault

#### sql-connection-string-main

| Property | Value |
|----------|-------|
| Name | sql-connection-string-main |
| Source | Terraform generates from sql_admin_password |
| Format | SQL Server connection string |
| Access Method | Key Vault reference in App Service |

**Format**:
```
Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Persist Security Info=False;User ID=<username>;Password=<password>;Encrypt=True;Connection Timeout=30;
```

**Example**:
```
Server=tcp:consilient-sql-dev.database.windows.net,1433;Initial Catalog=consilient_main_dev;Persist Security Info=False;User ID=sqladmin;Password=P@ssw0rd!;Encrypt=True;Connection Timeout=30;
```

**Terraform Source** (api_app.tf):
```hcl
"DefaultConnection" = "@Microsoft.KeyVault(VaultName=${vault_name};SecretName=sql-connection-string-main)"
```

---

#### sql-connection-string-hangfire

| Property | Value |
|----------|-------|
| Name | sql-connection-string-hangfire |
| Source | Terraform generates from sql_admin_password |
| Format | SQL Server connection string |
| Access Method | Key Vault reference in App Service |

**Purpose**: Hangfire background job queue database connection

**Format**: Same as main connection string, different database

---

#### jwt-signing-secret

| Property | Value |
|----------|-------|
| Name | jwt-signing-secret |
| Source | Terraform variable jwt_signing_secret |
| Format | Base64-encoded key |
| Access Method | Key Vault reference in App Service |

**Terraform Source** (keyvault.tf):
```hcl
resource "azurerm_key_vault_secret" "jwt_signing_secret" {
  name         = "jwt-signing-secret"
  value        = var.jwt_signing_secret
  key_vault_id = azurerm_key_vault.main.id
}
```

**Application Reference** (api_app.tf):
```hcl
"ApplicationSettings__Authentication__UserService__Jwt__Secret" =
  "@Microsoft.KeyVault(VaultName=${vault_name};SecretName=jwt-signing-secret)"
```

---

#### grafana-loki-url

| Property | Value |
|----------|-------|
| Name | grafana-loki-url |
| Source | Terraform generates Container App environment URL |
| Format | HTTPS URL |
| Access Method | Key Vault reference in App Service |

**Format**:
```
https://<loki-container-app-url>/loki/api/v1/push
```

**Purpose**: Logging endpoint for Grafana Loki in Container Apps

---

#### oauth-client-secret

| Property | Value |
|----------|-------|
| Name | oauth-client-secret |
| Source | Terraform variable oauth_client_secret (conditional) |
| Format | String (varies by provider) |
| Access Method | Key Vault reference in App Service |
| Conditional | Only created if oauth_client_secret is not empty |

**Purpose**: External OAuth provider authentication (if enabled)

**Terraform Source** (keyvault.tf):
```hcl
resource "azurerm_key_vault_secret" "oauth_client_secret" {
  count        = var.oauth_client_secret != "" ? 1 : 0
  name         = "oauth-client-secret"
  value        = var.oauth_client_secret
  key_vault_id = azurerm_key_vault.main.id
}
```

---

### Accessing Key Vault Secrets

#### From App Service (Runtime)

App Service uses managed identity to read secrets without storing credentials:

**App Settings with Key Vault Reference**:
```json
{
  "@Microsoft.KeyVault(VaultName=consilient-kv-dev;SecretName=jwt-signing-secret)": "secret-value"
}
```

**In Application Code** (no explicit authentication needed):
```csharp
var secret = Environment.GetEnvironmentVariable("ApplicationSettings__Authentication__UserService__Jwt__Secret");
// Or through configuration binding
services.Configure<JwtOptions>(configuration.GetSection("Authentication:UserService:Jwt"));
```

#### From Azure CLI

```powershell
# Get secret value
az keyvault secret show --vault-name <kv-name> --name <secret-name> --query value -o tsv

# Example:
az keyvault secret show --vault-name consilient-kv-dev --name jwt-signing-secret --query value
```

#### RBAC Permissions

**Terraform Service Principal** (for deployment):
- Role: `Key Vault Secrets Officer`
- Permissions: Create, read, update, delete secrets

**App Service Managed Identity** (at runtime):
- Role: `Key Vault Secrets User`
- Permissions: Read secrets only (no write)

---

## Environment-Specific Configuration

### Development Environment Setup

**Terraform Configuration**:
```hcl
# terraform.tfvars or environment variables
environment           = "dev"
region                = "canadacentral"
resource_group_name   = "consilient-dev-rg"
subscription_id       = "0e49a8db-e270-49d4-afb2-bab7d5b8357f"
sql_admin_username    = "sqladmin"
create_container_app_environment  = true
use_shared_container_environment  = false
```

**GitHub Secrets** (minimum required):
```
AZURE_CLIENT_ID
AZURE_TENANT_ID
AZURE_SUBSCRIPTION_ID
ARM_CLIENT_ID
ARM_CLIENT_SECRET
ARM_TENANT_ID
SQL_ADMIN_USERNAME
SQL_ADMIN_PASSWORD
JWT_SIGNING_SECRET
```

**GitHub Variables**:
```
AZURE_REGION = canadacentral
AZURE_RESOURCE_GROUP_NAME = consilient-dev-rg
TERRAFORM_VERSION = 1.6.0
```

**Key Vault**: `consilient-kv-dev-<suffix>`

**Cost**: ~$45/month

---

### Production Environment Setup

**Terraform Configuration**:
```hcl
# terraform.tfvars or environment variables
environment           = "prod"
region                = "canadacentral"
resource_group_name   = "consilient-prod-rg"
subscription_id       = "0e49a8db-e270-49d4-afb2-bab7d5b8357f"
sql_admin_username    = "sqladmin"
create_container_app_environment  = true
use_shared_container_environment  = false
enable_local_firewall = false  # Critical: never true
```

**GitHub Secrets** (same as dev, different values):
```
AZURE_CLIENT_ID
AZURE_TENANT_ID
AZURE_SUBSCRIPTION_ID
ARM_CLIENT_ID
ARM_CLIENT_SECRET
ARM_TENANT_ID
SQL_ADMIN_USERNAME
SQL_ADMIN_PASSWORD (different from dev)
JWT_SIGNING_SECRET (different from dev)
```

**Key Vault**: `consilient-kv-prod-<suffix>`

**Cost**: ~$2,800/month

**Additional Controls**:
- Zone redundancy: Enabled
- Backup retention: 365 days
- Audit logging: Enabled
- Threat detection: Enabled
- Network: Restricted access

---

## Setup & Configuration Guide

### Initial Setup Steps

#### Step 1: Azure Preparation

```powershell
# Log into Azure
az login

# Set subscription
az account set --subscription "<subscription-id>"

# Verify
az account show
```

#### Step 2: Create Service Principals

**OIDC Service Principal** (for GitHub login):
```powershell
# Create OIDC app registration
$appName = "consilient-app"
$appId = az ad app create --display-name $appName --query appId -o tsv

# Create service principal
az ad sp create --id $appId

# Set up federated credential for GitHub
az ad app federated-credential create \
  --id $appId \
  --parameters @- <<EOF
{
  "name": "github-actions",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:your-org/consilient-webapp:ref:refs/heads/main",
  "audiences": ["api://AzureADTokenExchange"],
  "description": "GitHub Actions OIDC credential"
}
EOF

# Get values for secrets
Write-Host "Client ID: $appId"
az account show --query tenantId
az account show --query id
```

**Terraform Service Principal**:
```powershell
# Create service principal for Terraform
az ad sp create-for-rbac --name "consilient-terraform" \
  --role Contributor \
  --scopes /subscriptions/<subscription-id>

# Output shows:
# appId (use as ARM_CLIENT_ID)
# password (use as ARM_CLIENT_SECRET)
# tenant (use as ARM_TENANT_ID)
```

#### Step 3: Configure GitHub Secrets

1. Go to: GitHub Repository → Settings → Secrets and Variables → Actions
2. Click "New repository secret" and add:

```
AZURE_CLIENT_ID = <from OIDC app>
AZURE_TENANT_ID = <from Azure>
AZURE_SUBSCRIPTION_ID = <from Azure>
ARM_CLIENT_ID = <from Terraform SP>
ARM_CLIENT_SECRET = <from Terraform SP>
ARM_TENANT_ID = <from Terraform SP>
SQL_ADMIN_USERNAME = sqladmin
SQL_ADMIN_PASSWORD = <generate strong password>
JWT_SIGNING_SECRET = <generate with openssl>
```

#### Step 4: Configure GitHub Variables

1. Go to: GitHub Repository → Settings → Secrets and Variables → Variables
2. Click "New repository variable" and add:

```
AZURE_REGION = canadacentral
AZURE_RESOURCE_GROUP_NAME = consilient-dev-rg
TERRAFORM_VERSION = 1.6.0
```

#### Step 5: Create terraform.tfvars

```powershell
cd infra/terraform
cp terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars`:
```hcl
region              = "canadacentral"
environment         = "dev"
resource_group_name = "consilient-dev-rg"
sql_admin_username  = "sqladmin"
sql_admin_password  = "P@ssw0rd!SecureABC"  # Use TF_VAR_* instead
subscription_id     = "your-subscription-id"
```

**Better Approach** (secure):
```bash
# Don't store password in terraform.tfvars
# Set as environment variable instead
export TF_VAR_sql_admin_password="P@ssw0rd!SecureABC"
export TF_VAR_jwt_signing_secret="$(openssl rand -base64 64)"

# Then deploy
terraform apply
```

#### Step 6: Initialize Terraform

```powershell
terraform init
```

#### Step 7: Plan Infrastructure

```powershell
terraform plan -out=tfplan
```

Review the output to verify what will be created.

#### Step 8: Deploy Infrastructure

```powershell
terraform apply tfplan
```

First run typically takes 10-20 minutes.

---

## Security Best Practices

### Secret Management

#### Never Commit Secrets

**Anti-Pattern** ❌:
```hcl
# terraform.tfvars - NEVER DO THIS
sql_admin_password = "MyPassword123!"
jwt_signing_secret = "base64secret"
```

**Correct Pattern** ✅:
```bash
# Use environment variables
export TF_VAR_sql_admin_password="MyPassword123!"
export TF_VAR_jwt_signing_secret="$(openssl rand -base64 64)"

# Then run terraform
terraform apply
```

#### Use Azure Key Vault for Runtime Secrets

Application should never have secrets in configuration files:

**Anti-Pattern** ❌:
```json
{
  "Authentication": {
    "Jwt": {
      "Secret": "base64-secret-here"
    }
  }
}
```

**Correct Pattern** ✅:
```json
{
  "Authentication": {
    "Jwt": {
      "Secret": "@Microsoft.KeyVault(VaultName=consilient-kv;SecretName=jwt-signing-secret)"
    }
  }
}
```

Application loads from Key Vault at runtime using managed identity (no credential storage).

#### Secret Rotation Schedule

| Secret | Rotation | Method |
|--------|----------|--------|
| ARM_CLIENT_SECRET | Quarterly | `az ad sp credential reset` |
| SQL_ADMIN_PASSWORD | Quarterly | Change in Azure, update GitHub secret, update Key Vault |
| JWT_SIGNING_SECRET | Annual or on compromise | Generate new, update GitHub secret, invalidate sessions |
| OAUTH_CLIENT_SECRET | Per provider | Per OAuth provider recommendations |

**Rotation Procedure**:

```powershell
# 1. Generate new secret
$newSecret = openssl rand -base64 64

# 2. Update in Azure/GitHub
az keyvault secret set --vault-name <kv-name> --name <secret-name> --value $newSecret

# Update in GitHub
# Settings → Secrets and Variables → Actions → Edit secret

# 3. If authentication-related (JWT), invalidate existing sessions
# This forces re-authentication with new secret

# 4. Update Key Vault
az keyvault secret set --vault-name <kv-name> --name <secret-name> --value $newSecret

# 5. Notify team if manual action needed
```

---

### Access Control

#### Azure RBAC Best Practices

**Principle of Least Privilege**:
- Terraform Service Principal: Only Contributor (needed for resource creation)
- App Service Managed Identity: Only "Secrets User" on Key Vault (read-only)
- SQL Admin: Only for deployment, not for application use

**RBAC Roles**:
```powershell
# Verify Terraform SP has Contributor
az role assignment list --assignee <app-id>

# Verify App Service has Secrets User on Key Vault
az role assignment list --assignee <managed-identity-id> \
  --scope <key-vault-id>
```

#### GitHub RBAC

**Secret Access Control**:
1. Go to: Repository → Settings → Collaborators → Manage access
2. Assign minimal required permissions:
   - **Read**: Can view secrets (in UI, not in logs)
   - **Write**: Can modify secrets
3. Use branch protection rules to control who can deploy to prod

---

### Subscription ID Security

**Current Issue** 🔴:
`terraform.tfvars` contains subscription ID in plain text. While less critical than passwords, it identifies your Azure account financially.

**Recommendation**:
```bash
# Move to environment variable
export TF_VAR_subscription_id="0e49a8db-e270-49d4-afb2-bab7d5b8357f"

# Remove from terraform.tfvars
# (leave blank or with placeholder)
```

---

### SQL Server Firewall

**enable_local_firewall Security** ⚠️:

**When Set to true** (current): Opens SQL to 0.0.0.0/0
```
- Anyone on internet with credentials can connect
- Only use for local act testing
- Must be false in production
```

**When Set to false** (production):
```
- SQL only accessible from App Service
- Network isolation via service endpoints
- High security posture
```

**Verification**:
```powershell
# Check current firewall rules
az sql server firewall-rule list --resource-group <rg> --server <server-name>

# Should NOT have 0.0.0.0 rule in production
```

---

### Compromised Secret Response

**If a secret is compromised:**

1. **Immediately revoke**:
   ```powershell
   # For Service Principal
   az ad sp credential reset --id <app-id>

   # For SQL password
   az sql server admin-user update --name <server> \
     --admin-user <username> --resource-group <rg>
   ```

2. **Rotate in all systems**:
   - GitHub secrets
   - Azure Key Vault
   - Terraform state (may need cleanup)
   - Any cached credentials

3. **Audit access**:
   ```powershell
   # Check Azure Activity Log for unauthorized access
   az monitor activity-log list --resource-group <rg> \
     --offset 24h --query "[].properties.statusMessage"
   ```

4. **Update Key Vault**:
   ```powershell
   az keyvault secret set --vault-name <name> --name <secret> --value <new-secret>
   ```

5. **Redeploy if needed**:
   - Run terraform apply with new secrets
   - Update GitHub Actions to use new secrets

---

## Troubleshooting

### GitHub Secrets and Variables

#### "Secret not found" Error in Workflows

**Error**: `Error: The secret 'AZURE_CLIENT_ID' is not found`

**Cause**: Secret not configured in GitHub

**Solution**:
1. Go to: Settings → Secrets and Variables → Actions
2. Verify all required secrets exist
3. Check exact spelling (case-sensitive)
4. Verify using `${{ secrets.SECRET_NAME }}` syntax in workflow

#### Secrets showing as null in logs

**Cause**: Secret not found at runtime

**Solution**:
1. Check secret exists in GitHub UI
2. Verify secret is at repository level (not organization level)
3. Push a new commit to trigger fresh workflow run

---

### Terraform

#### "Variable required but not supplied"

**Error**: `Error: Missing required variable`

**Cause**: Required variable not set

**Solution**:
```bash
# Set via environment variable
export TF_VAR_<variable_name>="value"

# Or add to terraform.tfvars
<variable_name> = "value"

# Then apply
terraform apply
```

#### Terraform validation error

**Error**: `Error: Invalid value for variable`

**Cause**: Variable doesn't match validation rules

**Examples**:
```
# environment = "staging" (invalid - must be dev or prod)
# Fix:
environment = "dev"

# region = "" (empty, but required)
# Fix:
region = "canadacentral"
```

---

### Azure Key Vault

#### "Access denied" when accessing Key Vault

**Error**: Application cannot read Key Vault secrets

**Cause**: RBAC not configured correctly

**Solution**:
```powershell
# Verify App Service managed identity has Secrets User role
az role assignment create \
  --assignee <managed-identity-object-id> \
  --role "Key Vault Secrets User" \
  --scope <key-vault-id>

# Or via Azure CLI
az keyvault set-policy --name <kv-name> \
  --object-id <managed-identity-object-id> \
  --secret-permissions get list
```

#### "Secret not found in Key Vault"

**Error**: Key Vault secret doesn't exist

**Solution**:
```powershell
# List all secrets in vault
az keyvault secret list --vault-name <kv-name> --query "[].name"

# If missing, recreate:
terraform apply
```

---

### Azure SQL

#### "Cannot connect to SQL Server"

**Error**: Connection timeout when deploying databases

**Causes**:
1. Firewall rule not configured
2. SQL Server not fully deployed yet
3. Credentials incorrect

**Solution**:
```powershell
# Check firewall rules
az sql server firewall-rule list --resource-group <rg> --server <server-name>

# If local testing, enable firewall
terraform apply -var="enable_local_firewall=true"

# Verify credentials
az sql server show --name <server-name> --resource-group <rg>
```

#### "Login failed for user"

**Error**: `Sql.Data.SqlClient.SqlException: Login failed for user 'sqladmin'`

**Cause**: Wrong password

**Solution**:
1. Verify SQL_ADMIN_PASSWORD secret in GitHub
2. Verify TF_VAR_sql_admin_password environment variable
3. If wrong password, rotate:

```powershell
# Reset SQL admin password in Key Vault
az keyvault secret set --vault-name <kv-name> \
  --name sql-connection-string-main \
  --value "Server=...;Password=<new-password>;..."
```

---

### GitHub Actions Workflows

#### Workflow fails at Terraform step

**Check**:
1. All required secrets configured
2. Service principal has permissions
3. Terraform state not locked

**Debug**:
```powershell
# Check for state lock
terraform show

# If locked, force unlock (careful!)
terraform force-unlock <lock-id>
```

#### Container image not found in workflow

**Error**: `Docker image not found`

**Cause**: Image not pushed to container registry

**Solution**:
1. Check CONTAINER_REGISTRY variable
2. Verify Docker image built successfully
3. Check container registry authentication

---

## Related Documentation

For more information, see:

- [Secrets Checklist](./secrets-checklist.md) - Quick reference for GitHub secrets setup
- [Authentication Guide](../components/authentication.md) - Detailed OIDC and Service Principal explanation
- [Quick Start Guide](../QUICK_START.md) - Step-by-step infrastructure deployment
- [Troubleshooting Guide](../TROUBLESHOOTING.md) - Extended troubleshooting procedures
- [GitHub Workflows](../../../../.github/workflows/) - All workflow YAML files
- [Terraform Configuration](../../../../infra/terraform/) - Complete Terraform code

---

**Last Updated**: December 2025
**For Navigation**: See [README.md](../README.md)

