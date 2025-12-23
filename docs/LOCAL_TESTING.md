# Local Testing with Act

This guide explains how to test GitHub Actions workflows locally using `act`.

## Prerequisites

1. **Docker Desktop** - Must be running
2. **Act** - GitHub Actions local runner ([installation guide](https://github.com/nektos/act#installation))
3. **Azure CLI** - For retrieving Azure resource information ([installation guide](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli))

## Setup

### 1. Build the Custom Docker Image

The first time you run `run-act-main.bat`, it will automatically build the custom Docker image if it doesn't exist. You can also build it manually:

```batch
docker build -t githubactions:latest -f GITHUBACTIONS.dockerfile .
```

### 2. Configure Azure Credentials

The `.env.act` file contains secrets and credentials for local testing. It's already configured with your Azure credentials from `.secrets`, but you need to update the SQL Server FQDN.

#### Option A: Automatic (Recommended)

Run the helper script to automatically retrieve and update the SQL Server FQDN:

```batch
get-sql-server.bat
```

This script will:
- Prompt you for the environment (dev/prod)
- Log in to Azure
- Retrieve the SQL Server FQDN for that environment
- Optionally update `.env.act` file automatically

#### Option B: Manual

1. Log in to Azure CLI:
   ```batch
   az login
   ```

2. Get the SQL Server FQDN:
   ```batch
   az sql server list --resource-group consilient-resource-group --query "[?contains(name, 'dev')].fullyQualifiedDomainName" -o tsv
   ```

3. Open `.env.act` and update the `AZURE_SQL_SERVER` value:
   ```
   AZURE_SQL_SERVER=consilient-sqlsrv-dev-XXXXXX.database.windows.net
   ```

### 3. Enable SQL Server Access for Local Testing

The SQL Server created by Terraform has `public_network_access_enabled = false` by default for security. To test database deployment locally, you need to temporarily enable public access and add a firewall rule.

#### Step 1: Enable Public Network Access

Run the toggle script to enable public access:

```batch
toggle-sql-public-access.bat
```

When prompted:
- Environment: `dev`
- Action: `enable`

#### Step 2: Add Firewall Rule for Your IP

Run the firewall rule script:

```batch
add-firewall-rule.bat
```

When prompted:
- Environment: `dev`

This will automatically detect your public IP and add it to the SQL Server firewall.

#### Step 3: After Testing - Disable Public Access

**IMPORTANT**: After you're done testing, disable public access for security:

```batch
toggle-sql-public-access.bat
```

When prompted:
- Environment: `dev`
- Action: `disable`

## Running Workflows Locally

### Run the Main Workflow

```batch
run-act-main.bat
```

You'll be prompted for:
1. **Environment** (dev/prod) - default: dev
2. **Skip Terraform?** (y/n) - default: y (recommended for faster local testing)
3. **Skip Databases?** (y/n) - default: n

### Workflow Execution Flow

When you run the main workflow:

1. **validate-environment** - Validates the environment input
2. **setup-prerequisites** - Sets up prerequisites (always runs)
3. **terraform** - Runs Terraform (skipped if you choose 'y')
4. **deploy-databases** - Deploys database scripts (runs if you choose 'n')

### Understanding the Database Deployment

The database deployment workflow:

1. **Discovers databases** - Scans `src/Databases` for directories
2. **Validates** - Checks that the directory exists and environment is valid
3. **Deploys** - For each database directory found:
   - Creates/updates the database `<dirname>_<environment>` (e.g., `consilient_main_dev`)
   - Runs all `.sql` files in alphabetical order
   - Uses Azure AD authentication

## Configuration Variables

Global configuration can be customized by setting GitHub repository variables:

- `AZURE_REGION` - Default: `canadacentral`
- `AZURE_RESOURCE_GROUP` - Default: `consilient-resource-group`
- `DB_SCRIPTS_PATH` - Default: `src/Databases`

For local testing with `act`, these fallback to the default values.

## Troubleshooting

### Database Deployment Fails with "No databases found"

- **Cause**: The database directory discovery failed
- **Solution**: Ensure `src/Databases` contains subdirectories with `.sql` files

### Database Deployment Fails with Authentication Error

- **Cause**: Azure credentials not loaded or SQL Server not accessible
- **Solutions**:
  1. Verify `.env.act` file exists and has correct `AZURE_SQL_SERVER` value
  2. Ensure you've enabled public access or added firewall rule
  3. Check that Azure credentials in `.env.act` are current

### "Cannot connect to Docker daemon"

- **Cause**: Docker Desktop is not running
- **Solution**: Start Docker Desktop and wait for it to fully start

### Act Shows "Error: no suitable container runtime found"

- **Cause**: Act can't find Docker
- **Solution**: Ensure Docker is installed and in your PATH

## Security Notes

⚠️ **Important**:

- Never commit `.env.act` or `.secrets` files to git (they're in `.gitignore`)
- The credentials in these files are sensitive
- For dev testing only - use proper GitHub Secrets for CI/CD
- Remember to disable public access on SQL Server after testing

## Additional Resources

- [Act Documentation](https://github.com/nektos/act)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure SQL Database Documentation](https://learn.microsoft.com/en-us/azure/azure-sql/database/)
