# GitHub Actions Composite Actions

Reusable composite actions for common workflow patterns across this repository.

## Overview

Composite actions encapsulate frequently-used step sequences into reusable units, reducing code duplication and improving maintainability.

### Benefits

- **DRY Principle**: Write once, use everywhere
- **Consistency**: Same logic applied across all workflows
- **Maintainability**: Update logic in one place
- **Readability**: Clearer workflow structure
- **Testability**: Easier to test common patterns

## Available Actions

### 1. azure-login

**Purpose**: Login to Azure with dual authentication support (OIDC for cloud, service principal for local testing)

**Location**: `.github/actions/azure-login/action.yml`

**Usage**:
```yaml
- name: Login to Azure (OIDC + act fallback)
  uses: ./.github/actions/azure-login
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
    azure-credentials: ${{ secrets.AZURE_CREDENTIALS }}
```

**Inputs**:
- `client-id` (required): Azure Client ID for OIDC
- `tenant-id` (required): Azure Tenant ID for OIDC
- `subscription-id` (required): Azure Subscription ID for OIDC
- `azure-credentials` (required): JSON credentials for service principal (used only with act)

**Behavior**:
- **In GitHub Actions Cloud** (`!env.ACT`): Uses OIDC for authentication (secure, no long-lived secrets)
- **When Running Locally with act** (`env.ACT`): Uses service principal credentials from JSON

**Why Needed**:
- OIDC cannot be used with the `act` tool (local testing simulator)
- Service principal works with `act` but requires long-lived secrets in GitHub Actions cloud
- This pattern provides both security in cloud and functionality for local testing

**Used In**:
- `terraform.yml`
- `databases.yml`
- `dotnet_apps.yml`

---

### 2. validate-inputs

**Purpose**: Validate workflow inputs and configuration before execution

**Location**: `.github/actions/validate-inputs/action.yml`

**Usage**:
```yaml
- name: Validate inputs
  uses: ./.github/actions/validate-inputs
  with:
    environment: ${{ inputs.environment }}
    action: ${{ inputs.action }}
    scripts-path: ${{ inputs.scripts_path }}
    required-fields: '{"API_APP_NAME": "${{ env.API_APP_NAME }}", "ACR_REGISTRY": "${{ env.ACR_REGISTRY }}"}'
```

**Inputs**:
- `environment` (optional): Environment value (dev, prod, staging)
- `action` (optional): Action value (plan, apply, destroy)
- `scripts-path` (optional): Directory path that must exist
- `required-fields` (optional): JSON object of field:value pairs that must be set

**Outputs**:
- `validation-passed`: Boolean indicating whether validation succeeded

**Validations Performed**:
- Environment is valid (dev, prod, staging)
- Action is valid (plan, apply, destroy)
- Scripts path exists as a directory
- All required fields are set and non-empty

**Used In**:
- Recommended for all infrastructure and deployment workflows

---

### 3. debug-variables

**Purpose**: Display environment variables for debugging, with automatic masking of sensitive values

**Location**: `.github/actions/debug-variables/action.yml`

**Usage**:
```yaml
- name: Display configuration
  uses: ./.github/actions/debug-variables
  with:
    section-title: 'Terraform Configuration'
    variables: |
      {
        "TF_VAR_environment": "${{ env.TF_VAR_environment }}",
        "TF_VAR_region": "${{ env.TF_VAR_region }}",
        "TF_VAR_sql_admin_password": "${{ secrets.SQL_ADMIN_PASSWORD }}"
      }
```

**Inputs**:
- `variables` (required): JSON object of variable names and values
- `section-title` (optional): Title for debug output section (default: "Environment Variables")

**Outputs**: None (outputs to logs only)

**Features**:
- Automatically masks sensitive values (passwords, secrets, tokens, keys, credentials)
- Truncates long values (>30 chars) to last 20 characters for readability
- Formats output with clear section headers
- Non-sensitive values displayed fully for debugging

**Used In**:
- Any workflow step that needs to log configuration

---

### 4. sqlcmd-execute

**Purpose**: Execute SQL scripts against Azure SQL Database with proper error handling, timeouts, and Azure AD authentication

**Location**: `.github/actions/sqlcmd-execute/action.yml`

**Usage**:
```yaml
- name: Apply SQL script
  uses: ./.github/actions/sqlcmd-execute
  with:
    sql-server: ${{ env.SQL_SERVER }}
    database-name: ${{ env.DATABASE_NAME }}
    script-file: './infra/db/schema.sql'
    timeout: '600'
    fail-on-error: 'true'
```

**Inputs**:
- `sql-server` (required): SQL Server FQDN (e.g., server.database.windows.net)
- `database-name` (required): Database name to target
- `script-file` (required): Path to SQL script file
- `timeout` (optional): Command timeout in seconds (default: 600)
- `fail-on-error` (optional): Whether to fail workflow on error (default: true)

**Outputs**:
- `exit-code`: Exit code from sqlcmd execution
- `error-message`: Error message if execution failed

**Features**:
- Validates script file exists before execution
- Uses Azure AD authentication (-G flag)
- Enforces timeout to prevent hanging
- Distinguishes between timeout (124) and other errors
- Optional error handling with `fail-on-error` parameter
- Automatically logs error output from sqlcmd
- Cleans up temporary error log files

**Used In**:
- `databases.yml` (database deployment workflow)

---

## Code Reduction

### Before Composite Actions

Example from `terraform.yml` (23 lines):
```yaml
- name: Login to Azure (OIDC)
  if: ${{ !env.ACT }}
  uses: azure/login@v2.3.0
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

- name: Login to Azure (Service Principal for act)
  if: ${{ env.ACT }}
  shell: bash
  run: |
    CREDS='${{ secrets.AZURE_CREDENTIALS }}'
    CLIENT_ID=$(echo "$CREDS" | jq -r '.clientId')
    CLIENT_SECRET=$(echo "$CREDS" | jq -r '.clientSecret')
    TENANT_ID=$(echo "$CREDS" | jq -r '.tenantId')
    az login --service-principal -u "$CLIENT_ID" -p "$CLIENT_SECRET" --tenant "$TENANT_ID"
    az account set --subscription "${{ secrets.AZURE_SUBSCRIPTION_ID }}"
    echo "✅ Successfully logged in to Azure via service principal"
```

### After Composite Actions

```yaml
- name: Login to Azure (OIDC + act fallback)
  uses: ./.github/actions/azure-login
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
    azure-credentials: ${{ secrets.AZURE_CREDENTIALS }}
```

**Reduction**: 23 lines → 8 lines (65% reduction)

**Applied to**: 3 workflows × 23 lines = **69 lines eliminated**

---

## Best Practices

### When to Create a Composite Action

- **Reused 2+ times** across workflows
- **5+ lines of code** in a single step
- **Complex logic** that should be tested independently
- **Best practices** that should be standardized

### When NOT to Create a Composite Action

- **One-time use** only
- **Very simple** steps (single command, no conditional logic)
- **Highly specific** to a single workflow (unlikely to be reused)

### Composite Action Guidelines

1. **Keep it focused**: One responsibility per action
2. **Document inputs/outputs**: Clear descriptions for each parameter
3. **Use meaningful defaults**: Fallback values when appropriate
4. **Handle errors gracefully**: Validate inputs, provide helpful messages
5. **Log clearly**: Use consistent output formatting
6. **Test locally**: Use `act` tool to verify behavior
7. **Version dependencies**: Pin action versions (e.g., `azure/login@v2.3.0`)

---

## Examples

### Example 1: Using azure-login and debug-variables

```yaml
- name: Checkout
  uses: actions/checkout@v4

- name: Login to Azure (OIDC + act fallback)
  uses: ./.github/actions/azure-login
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
    azure-credentials: ${{ secrets.AZURE_CREDENTIALS }}

- name: Display configuration
  uses: ./.github/actions/debug-variables
  with:
    section-title: 'Deployment Configuration'
    variables: |
      {
        "ENVIRONMENT": "${{ inputs.environment }}",
        "APP_NAME": "${{ env.APP_NAME }}",
        "REGION": "${{ vars.AZURE_REGION }}"
      }

- name: Validate inputs
  uses: ./.github/actions/validate-inputs
  with:
    environment: ${{ inputs.environment }}
    required-fields: '{"APP_NAME": "${{ env.APP_NAME }}"}'
```

### Example 2: Database deployment with SQL execution

```yaml
- name: Login to Azure
  uses: ./.github/actions/azure-login
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
    azure-credentials: ${{ secrets.AZURE_CREDENTIALS }}

- name: Execute schema script
  uses: ./.github/actions/sqlcmd-execute
  with:
    sql-server: ${{ env.SQL_SERVER }}
    database-name: ${{ env.DATABASE_NAME }}
    script-file: './infra/db/schema.sql'
    timeout: '300'

- name: Execute seed data
  uses: ./.github/actions/sqlcmd-execute
  with:
    sql-server: ${{ env.SQL_SERVER }}
    database-name: ${{ env.DATABASE_NAME }}
    script-file: './infra/db/seed.sql'
    fail-on-error: 'false'  # Seed failures are non-critical
```

---

## Maintenance

### Updating Composite Actions

1. Edit the action file in `.github/actions/{action-name}/action.yml`
2. All workflows using the action will automatically use the updated version
3. No need to update individual workflow files
4. Test changes with `act` tool before pushing

### Versioning

Currently, composite actions are referenced from the main branch:
```yaml
uses: ./.github/actions/azure-login
```

In the future, consider:
- Creating releases for actions (GitHub Actions Marketplace)
- Pinning versions: `uses: my-org/azure-login@v1.0.0`
- Semantic versioning (major.minor.patch)

---

## Related Documentation

- [GitHub Actions - Composite Actions](https://docs.github.com/en/actions/creating-actions/creating-a-composite-action)
- [Terraform Workflow](./workflows/terraform.yml)
- [Database Deployment Workflow](./workflows/databases.yml)
- [OIDC Authentication Pattern](../docs/OIDC_IMPLEMENTATION.md)
