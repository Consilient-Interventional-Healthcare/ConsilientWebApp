# Database Deployment Guide for Azure SQL

This guide covers setting up Azure SQL Database and deploying your database using GitHub Actions.

## Table of Contents
- [Azure SQL Setup](#azure-sql-setup)
- [GitHub Actions Workflow](#github-actions-workflow)
- [Required Secrets](#required-secrets)
- [Usage Examples](#usage-examples)
- [Troubleshooting](#troubleshooting)

## Overview

This workflow provides **automatic multi-database deployment** to Azure SQL with **full infrastructure automation**:

✅ **Auto-Discovery**: Automatically finds all databases in `src/.docker/Db/`
✅ **Infrastructure Setup**: Creates SQL Server, resource group, and databases if they don't exist
✅ **Environment-Specific Names**: Database name = `{directory}_{environment}`
✅ **Parallel Deployment**: All databases deploy simultaneously
✅ **Ordered Execution**: SQL files run alphabetically (use `01_`, `02_` prefixes)
✅ **Production Safety**: Database deletion is FORBIDDEN in production and staging
✅ **Service Tier Selection**: Choose database pricing tier (Basic, S0, S1, etc.)
✅ **Zero Configuration**: No need to list databases - just add directories!

**Quick Example:**

Directory structure:
```
src/.docker/Db/
├── consilient_main/
├── consilient_hangfire/
└── analytics/
```

**Development environment** creates:
- `consilient_main_development`
- `consilient_hangfire_development`
- `analytics_development`

**Production environment** creates:
- `consilient_main_production`
- `consilient_hangfire_production`
- `analytics_production`

This allows **one SQL Server** to host databases for multiple environments!

## Azure SQL Setup

You have two options for setting up Azure SQL Server and databases:

1. **Automated (Recommended)**: Let the GitHub Actions workflow create everything automatically
2. **Manual**: Use the provided bash scripts for manual setup

### Option 1: Automated Setup (Recommended)

The GitHub Actions workflow automatically creates:
- Resource group (if doesn't exist)
- SQL Server (if doesn't exist)
- Databases (if don't exist)
- Firewall rules

**Benefits:**
- No manual setup required
- Fully automated
- Works on first run
- Idempotent (safe to run multiple times)

**Steps:**
1. Configure GitHub secrets (see [Required Secrets](#required-secrets))
2. Run the workflow - it handles everything automatically!

### Option 2: Manual Setup

Use the interactive script for manual setup:

**Interactive Script**: `src/infra/03_setup_database.sh`
- For manual local setup
- Prompts for passwords and confirmations
- Adds your local IP to firewall
- Good for initial testing

**CI/CD Script**: `src/infra/setup_database_ci.sh`
- Used by GitHub Actions
- Non-interactive (all config via environment variables)
- No prompts or confirmations

### Step 1: Create Azure SQL Server (Manual Method)

Run these commands in Azure Cloud Shell or local terminal with Azure CLI installed:

```bash
# Set your variables (CHANGE THESE!)
RESOURCE_GROUP="consilient-rg"
LOCATION="eastus"
SQL_SERVER_NAME="consilient-sql-prod"  # Must be globally unique!
ADMIN_USER="sqladmin"
ADMIN_PASSWORD="YourSecureP@ssw0rd123!"  # Use a strong password!

# Create SQL Server
az sql server create \
  --name $SQL_SERVER_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $ADMIN_USER \
  --admin-password $ADMIN_PASSWORD

# Configure firewall to allow Azure services (required for GitHub Actions)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### Step 2: Create Databases (Optional)

The workflow automatically creates databases when they don't exist. However, you can create them manually if needed:

```bash
# Create databases for development environment
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name consilient_main_development \
  --service-objective S0 \
  --backup-storage-redundancy Local

az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name consilient_hangfire_development \
  --service-objective S0 \
  --backup-storage-redundancy Local

# For production environment, use different names:
# consilient_main_production, consilient_hangfire_production, etc.
```

**Note**: The workflow creates databases automatically with the naming pattern `{directory}_{environment}`. You typically don't need to create them manually.

### Step 3: Database Pricing Tiers

Choose the appropriate tier for your environment:

| Tier | Monthly Cost | Storage | Use Case |
|------|--------------|---------|----------|
| **Basic** | ~$5 | 2GB | Development/Testing |
| **S0** | ~$15 | 250GB | Staging (recommended) |
| **S1** | ~$30 | 250GB | Small Production |
| **S2-S3** | $75-120 | 250GB | Production |
| **Premium** | $465+ | 500GB+ | High-performance Production |

To change the tier later:
```bash
az sql db update \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name consilient_main \
  --service-objective S1
```

### Step 4: Optional - Allow Your Local IP

For testing from your local machine:

```bash
# Get your IP
MY_IP=$(curl -s ifconfig.me)

# Add firewall rule
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowMyIP \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

### Step 5: Test Connection

Test your connection locally:

```bash
# Using sqlcmd (install from: https://learn.microsoft.com/en-us/sql/tools/sqlcmd)
sqlcmd -S $SQL_SERVER_NAME.database.windows.net \
  -d consilient_main \
  -U $ADMIN_USER \
  -P "$ADMIN_PASSWORD"
```

## GitHub Actions Workflow

### What It Does

The workflow (`.github/workflows/deploy-database-azure.yml`) automatically discovers and deploys **multiple databases** with full infrastructure setup:

1. **Discovers databases** - Scans `src/.docker/Db/` for subdirectories
2. **Sets up infrastructure** - Creates Azure SQL Server and resource group if they don't exist
3. **Creates databases** - Creates databases with environment-specific names: `{directory}_{environment}`
4. **Handles recreation** - Can drop and recreate databases (development only, blocked in staging/production)
5. **Applies SQL scripts** - All `.sql` files in each directory are applied in alphabetical order
6. **Parallel deployment** - All databases are deployed simultaneously using matrix strategy

**Example Structure:**
```
src/.docker/Db/
├── consilient_main/
│   ├── 01_init.sql
│   ├── 02_identity.sql
│   └── seed.sql
├── consilient_hangfire/
│   ├── 01_schema.sql
│   └── 02_tables.sql
└── reporting/
    └── setup.sql
```

**Development deployment** creates:
- `consilient_main_development`
- `consilient_hangfire_development`
- `reporting_development`

**Staging deployment** creates:
- `consilient_main_staging`
- `consilient_hangfire_staging`
- `reporting_staging`

**Production deployment** creates:
- `consilient_main_production`
- `consilient_hangfire_production`
- `reporting_production`

### Triggers

**Automatic**: Pushes to `main` branch that modify files in:
- `src/.docker/Db/**/*.sql`
- The workflow file itself
- Automatically uses staging environment with Basic tier

**Manual**: Via GitHub Actions UI with options:
- Choose environment (development/staging/production)
- Choose service tier (Basic, S0, S1, etc.) - defaults to Basic
- Option to recreate ALL databases (development only, blocked in staging/production)

## Required Secrets

### Setup GitHub Environments

1. Go to **Settings → Environments** in GitHub
2. Create three environments: `development`, `staging`, `production`
3. Add secrets to each environment

### Secrets for Each Environment

| Secret Name | Description | Example | Required |
|------------|-------------|---------|----------|
| `AZURE_CREDENTIALS` | Service principal JSON for Azure login | `{"clientId":"...","clientSecret":"...","subscriptionId":"...","tenantId":"..."}` | Yes |
| `AZURE_RESOURCE_GROUP` | Azure resource group name | `consilient-resource-group` | Yes |
| `AZURE_LOCATION` | Azure region | `canadacentral` | Yes |
| `AZURE_SQL_SERVER_NAME` | SQL Server name (without .database.windows.net) | `consilient-sql-dev` | Yes |
| `AZURE_SQL_SERVER` | SQL Server FQDN (for sqlcmd) | `consilient-sql-dev.database.windows.net` | Yes |
| `AZURE_SQL_ADMIN_USER` | SQL admin username | `sqladmin` | Yes |
| `AZURE_SQL_ADMIN_PASSWORD` | SQL admin password | `YourSecureP@ssw0rd123!` | Yes |

**Important Notes:**
- You need BOTH `AZURE_SQL_SERVER_NAME` (for az cli commands) AND `AZURE_SQL_SERVER` (for sqlcmd)
- `AZURE_CREDENTIALS` is used for Azure login during infrastructure setup
- Database names are automatically discovered from directory names in `src/.docker/Db/`
- The workflow will create the SQL Server and resource group if they don't exist

### Setting Up Secrets

#### Via GitHub Web UI

1. Go to **Settings → Environments**
2. Click on an environment (e.g., `development`)
3. Click **Add secret**
4. Add each secret with its value
5. Repeat for other environments

**Important**: Use different servers/databases/passwords for each environment!

#### Via GitHub CLI

```bash
# Install: https://cli.github.com/
gh auth login

# First, create the Azure service principal and get the JSON output
# See "Creating Azure Service Principal" section below

# Add secrets to development environment
gh secret set AZURE_CREDENTIALS -e development -b '{"clientId":"...","clientSecret":"...","subscriptionId":"...","tenantId":"..."}'
gh secret set AZURE_RESOURCE_GROUP -e development -b "consilient-resource-group"
gh secret set AZURE_LOCATION -e development -b "canadacentral"
gh secret set AZURE_SQL_SERVER_NAME -e development -b "consilient-sql-dev"
gh secret set AZURE_SQL_SERVER -e development -b "consilient-sql-dev.database.windows.net"
gh secret set AZURE_SQL_ADMIN_USER -e development -b "sqladmin"
gh secret set AZURE_SQL_ADMIN_PASSWORD -e development -b "DevPassword123!"

# Add secrets to staging environment
gh secret set AZURE_CREDENTIALS -e staging -b '{"clientId":"...","clientSecret":"...","subscriptionId":"...","tenantId":"..."}'
gh secret set AZURE_RESOURCE_GROUP -e staging -b "consilient-resource-group"
gh secret set AZURE_LOCATION -e staging -b "canadacentral"
gh secret set AZURE_SQL_SERVER_NAME -e staging -b "consilient-sql-staging"
gh secret set AZURE_SQL_SERVER -e staging -b "consilient-sql-staging.database.windows.net"
gh secret set AZURE_SQL_ADMIN_USER -e staging -b "sqladmin"
gh secret set AZURE_SQL_ADMIN_PASSWORD -e staging -b "StagingPassword123!"

# Add secrets to production environment
gh secret set AZURE_CREDENTIALS -e production -b '{"clientId":"...","clientSecret":"...","subscriptionId":"...","tenantId":"..."}'
gh secret set AZURE_RESOURCE_GROUP -e production -b "consilient-resource-group"
gh secret set AZURE_LOCATION -e production -b "canadacentral"
gh secret set AZURE_SQL_SERVER_NAME -e production -b "consilient-sql-prod"
gh secret set AZURE_SQL_SERVER -e production -b "consilient-sql-prod.database.windows.net"
gh secret set AZURE_SQL_ADMIN_USER -e production -b "sqladmin"
gh secret set AZURE_SQL_ADMIN_PASSWORD -e production -b "ProductionPassword123!"
```

#### Creating Azure Service Principal

To create the `AZURE_CREDENTIALS` secret, run this command:

```bash
az ad sp create-for-rbac \
  --name "github-actions-database-deployment" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
  --sdk-auth
```

The output JSON should be used as the value for `AZURE_CREDENTIALS` secret.

## Usage Examples

### 1. Deploy All Databases to Staging

1. Go to **Actions** tab in GitHub
2. Click **Deploy Database to Azure SQL**
3. Click **Run workflow**
4. Select:
   - Environment: `staging`
   - Service tier: `Basic` (or `S0` for better performance)
   - Recreate database: `false` (unchecked)
5. Click **Run workflow**

The workflow will:
- Create Azure SQL Server if it doesn't exist
- Create resource group if it doesn't exist
- Discover all directories in `src/.docker/Db/`
- Create each database if it doesn't exist (with specified service tier)
- Apply all SQL scripts in each directory (sorted alphabetically)
- Deploy all databases in parallel

### 2. Fresh Development Databases (Delete & Recreate ALL)

1. Go to **Actions** → **Deploy Database to Azure SQL**
2. Click **Run workflow**
3. Select:
   - Environment: `development`
   - Recreate database: `true` (checked)
4. Click **Run workflow**

⚠️ **WARNING**: This DROPS **ALL** databases and recreates them from scratch!

### 3. Deploy to Production

1. Go to **Actions** → **Deploy Database to Azure SQL**
2. Click **Run workflow**
3. Select:
   - Environment: `production`
   - Service tier: `S1` or higher (for production workloads)
   - Recreate database: `false` (MUST be unchecked)
4. Click **Run workflow**

**CRITICAL SAFETY NOTES**:
- ❌ Database recreation is **FORBIDDEN** in production environment
- ❌ Database recreation is **NOT ALLOWED** in staging environment
- ✅ The workflow will fail with a clear error if you attempt to recreate in production/staging
- ✅ This protection exists at multiple layers (workflow validation + bash script validation)

### 4. Automatic Deployment

When you push changes to `main` that affect SQL files:
- Workflow automatically deploys to **staging** environment
- Applies changes to all discovered databases
- Recreate database: **false** (never recreates automatically)

## Connection Strings

### For Azure Web App

Configure connection strings in Azure Portal or via CLI. **Important**: Use the full database name including the environment suffix!

```bash
# For production environment
az webapp config connection-string set \
  --name consilient-api-prod \
  --resource-group consilient-rg \
  --connection-string-type SQLAzure \
  --settings \
    Default="Server=tcp:consilient-sql-prod.database.windows.net,1433;Initial Catalog=consilient_main_production;User ID=sqladmin;Password=YourPassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
    Hangfire="Server=tcp:consilient-sql-prod.database.windows.net,1433;Initial Catalog=consilient_hangfire_production;User ID=sqladmin;Password=YourPassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# For staging environment
az webapp config connection-string set \
  --name consilient-api-staging \
  --resource-group consilient-rg \
  --connection-string-type SQLAzure \
  --settings \
    Default="Server=tcp:consilient-sql-staging.database.windows.net,1433;Initial Catalog=consilient_main_staging;User ID=sqladmin;Password=StagingPassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
    Hangfire="Server=tcp:consilient-sql-staging.database.windows.net,1433;Initial Catalog=consilient_hangfire_staging;User ID=sqladmin;Password=StagingPassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### Format

```
Server=tcp:{SQL_SERVER},1433;Initial Catalog={DIRECTORY_NAME}_{ENVIRONMENT};User ID={ADMIN_USER};Password={ADMIN_PASSWORD};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

**Examples:**
- Development: `Initial Catalog=consilient_main_development`
- Staging: `Initial Catalog=consilient_main_staging`
- Production: `Initial Catalog=consilient_main_production`

## SQL Scripts

### Directory Structure

Each subdirectory in `src/.docker/Db/` represents one database family (deployed to each environment with environment suffix):

```
src/.docker/Db/
├── consilient_main/          ← Databases: consilient_main_{environment}
│   ├── 01_init.sql
│   ├── 02_identity.sql
│   └── seed.sql
├── consilient_hangfire/       ← Databases: consilient_hangfire_{environment}
│   ├── 01_schema.sql
│   └── 02_tables.sql
└── analytics/                 ← Databases: analytics_{environment}
    └── setup.sql
```

**Environment-Specific Database Names:**
- **Development**: `consilient_main_development`, `consilient_hangfire_development`, `analytics_development`
- **Staging**: `consilient_main_staging`, `consilient_hangfire_staging`, `analytics_staging`
- **Production**: `consilient_main_production`, `consilient_hangfire_production`, `analytics_production`

**Benefits of Environment Suffix:**
- ✅ **Single SQL Server**: Host all environments on one server (saves cost)
- ✅ **Clear Separation**: Never accidentally connect to wrong environment
- ✅ **Easy Testing**: Can test staging and development side-by-side
- ✅ **Cost Optimization**: Share server infrastructure across environments

### Script Execution Order

- SQL files are executed in **alphabetical order** within each directory
- Use numeric prefixes (01_, 02_, etc.) to control execution order
- All scripts in a directory are applied to that database

### Example: consilient_main Database

**01_init.sql**
- Creates schemas: `Compensation`, `Clinical`
- Creates main tables: Employees, Facilities, Patients, Visits, etc.
- Creates indexes and constraints
- Uses `__EFMigrationsHistory` for idempotency

**02_identity.sql**
- Creates `Identity` schema
- Creates Identity tables: Users, Roles, UserRoles, etc.
- Creates indexes
- Idempotent (safe to run multiple times)

**seed.sql**
- Inserts seed data for Roles, Users, Facilities, etc.
- Uses `SET IDENTITY_INSERT` for specific IDs
- Should be idempotent (or made idempotent with IF NOT EXISTS checks)

### Adding a New Database

To add a new database:

1. Create a new directory: `src/.docker/Db/your_database_name/`
2. Add SQL scripts with numeric prefixes:
   ```
   src/.docker/Db/your_database_name/
   ├── 01_schema.sql
   ├── 02_tables.sql
   └── 03_seed.sql
   ```
3. Commit and push - the workflow will automatically discover and deploy it!

### Making Scripts Idempotent

To make scripts safe to run multiple times, use checks like:

```sql
-- Check before creating table
IF OBJECT_ID(N'[Schema].[TableName]') IS NULL
BEGIN
    CREATE TABLE [Schema].[TableName] (...)
END

-- Check before inserting data
IF NOT EXISTS (SELECT 1 FROM [Schema].[Table] WHERE Id = 1)
BEGIN
    INSERT INTO [Schema].[Table] VALUES (...)
END

-- Check migrations history
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251124205308_Initial'
)
BEGIN
    -- Your migration code here
END
```

## Troubleshooting

### Error: "Azure login failed" or "Not logged in to Azure"

**Cause**: `AZURE_CREDENTIALS` secret is missing or invalid

**Solution**:
1. Create service principal:
   ```bash
   az ad sp create-for-rbac \
     --name "github-actions-database-deployment" \
     --role contributor \
     --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
     --sdk-auth
   ```
2. Copy the entire JSON output
3. Add it as `AZURE_CREDENTIALS` secret in GitHub environment settings

### Error: "Resource group creation failed" or "Location not valid"

**Cause**: Wrong `AZURE_LOCATION` or insufficient permissions

**Solution**:
1. Check that `AZURE_LOCATION` secret is a valid Azure region (e.g., `canadacentral`, `eastus`)
2. List available locations: `az account list-locations -o table`
3. Ensure service principal has contributor role on the subscription or resource group

### Error: "Cannot open server"

**Cause**: Firewall blocking connection from GitHub Actions

**Solution**:
```bash
# Ensure Azure services are allowed
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### Error: "Login failed for user"

**Cause**: Wrong username or password in GitHub secrets

**Solution**:
1. Test credentials locally first:
   ```bash
   sqlcmd -S your-server.database.windows.net -U sqladmin -P "password"
   ```
2. Update GitHub secrets with correct values
3. Check for special characters in password (may need escaping)

### Error: "Database 'consilient_main_development' does not exist"

**Cause**: Database not created yet

**Solution**: The workflow creates it automatically with the environment suffix. If it fails, create manually:
```bash
# Create with environment suffix
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name consilient_main_development \
  --service-objective S0

# Or for staging
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name consilient_main_staging \
  --service-objective S0
```

### Error: "Object already exists" when running scripts

**Cause**: Scripts not fully idempotent

**Solutions**:
1. Use development environment with "Recreate database" checked to start fresh
2. Update scripts to include `IF NOT EXISTS` checks
3. Use `__EFMigrationsHistory` table to track applied changes

### Workflow succeeds but data not there

**Cause**: Seed data using IDENTITY_INSERT may fail if IDs already exist

**Solution**:
1. Use "Recreate database" in development
2. Add checks in seed.sql:
   ```sql
   IF NOT EXISTS (SELECT 1 FROM [Identity].[Users] WHERE Id = 100)
   BEGIN
       SET IDENTITY_INSERT [Identity].[Users] ON;
       INSERT INTO [Identity].[Users] (...) VALUES (...);
       SET IDENTITY_INSERT [Identity].[Users] OFF;
   END
   ```

### Error: "Database recreation is FORBIDDEN in production"

**This is by design!** This is a critical safety feature.

**Cause**: Attempted to run workflow with `recreate_database: true` in production or staging environment

**Why this protection exists**:
- Prevents accidental data loss in production
- Protects staging environment from accidental deletion
- Multiple validation layers ensure safety

**Solution**:
- For production/staging: Always use `recreate_database: false`
- For development: Recreate is allowed and will work

**If you really need to reset staging/production**:
1. Manually delete database in Azure Portal (requires explicit action)
2. Run workflow with `recreate_database: false` (it will create new database)
3. This ensures you consciously make the decision to delete data

### One database fails but others succeed

**Cause**: Matrix strategy deploys all databases in parallel with `fail-fast: false`

**Behavior**: If one database deployment fails, others continue

**Solution**:
1. Check the failed job logs in the Actions tab
2. Fix the SQL scripts for the failing database
3. Re-run the workflow - it will only affect the databases that need updates

### No databases found

**Symptom**: Workflow says "No databases found"

**Cause**: No subdirectories in `src/.docker/Db/`

**Solution**:
1. Ensure you have at least one subdirectory: `src/.docker/Db/database_name/`
2. Add at least one `.sql` file in the directory
3. Commit and push

### Application can't connect to database

**Symptom**: Application throws "Cannot open database" error

**Cause**: Connection string missing environment suffix

**Solution**: Update connection string to include environment suffix:

❌ **Wrong**:
```
Initial Catalog=consilient_main
```

✅ **Correct**:
```
Initial Catalog=consilient_main_production  (for production)
Initial Catalog=consilient_main_staging     (for staging)
Initial Catalog=consilient_main_development (for development)
```

**Check your connection strings** in Azure Portal:
1. Go to Web App → Configuration → Connection strings
2. Verify database name includes `_{environment}` suffix
3. Save and restart the app

## Security Best Practices

### 1. Use Strong Passwords
```bash
# Generate a strong password
openssl rand -base64 32
```

### 2. Rotate Passwords Regularly
Change SQL admin passwords every 90 days and update GitHub secrets.

### 3. Environment Isolation

**Option 1: Shared SQL Server (Cost-Effective)**
Use one SQL Server with environment suffixes:
- Database names: `consilient_main_development`, `consilient_main_staging`, `consilient_main_production`
- Pros: Lower cost, easier management
- Cons: All environments on same server

**Option 2: Separate SQL Servers (Most Secure)**
Use different SQL servers for each environment:
- `consilient-sql-dev.database.windows.net` → development databases
- `consilient-sql-staging.database.windows.net` → staging databases
- `consilient-sql-prod.database.windows.net` → production databases
- Pros: Complete isolation, better security
- Cons: Higher cost, more management overhead

**Recommendation**: Start with Option 1 (shared server) for dev/staging, use separate server for production.

### 4. Limit Firewall Rules
Only allow specific IPs in production:
```bash
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowOfficeIP \
  --start-ip-address 203.0.113.10 \
  --end-ip-address 203.0.113.10
```

### 5. Enable Azure AD Authentication (Advanced)
```bash
# Set Azure AD admin
az sql server ad-admin create \
  --resource-group $RESOURCE_GROUP \
  --server-name $SQL_SERVER_NAME \
  --display-name "SQL Admin" \
  --object-id <your-object-id>
```

### 6. Enable Auditing
```bash
# Create storage for audit logs
az storage account create \
  --name consilientsqlaudit \
  --resource-group $RESOURCE_GROUP \
  --sku Standard_LRS

# Enable auditing
az sql server audit-policy update \
  --resource-group $RESOURCE_GROUP \
  --name $SQL_SERVER_NAME \
  --state Enabled \
  --storage-account consilientsqlaudit
```

## Monitoring

### View Deployment Logs

1. Go to **Actions** tab
2. Click on a workflow run
3. Click on **Deploy Database Scripts** job
4. Expand each step to see detailed output

### Query Database

After deployment, verify tables were created. **Remember to use the full database name with environment suffix!**

```bash
# Connect to development database
sqlcmd -S consilient-sql-dev.database.windows.net \
  -d consilient_main_development \
  -U sqladmin \
  -P "password"

# Connect to production database
sqlcmd -S consilient-sql-prod.database.windows.net \
  -d consilient_main_production \
  -U sqladmin \
  -P "password"
```

```sql
-- List all tables
SELECT
    SCHEMA_NAME(schema_id) as SchemaName,
    name as TableName
FROM sys.tables
ORDER BY SchemaName, TableName;

-- Check row counts
SELECT 'Users' as TableName, COUNT(*) as RowCount FROM [Identity].[Users]
UNION ALL
SELECT 'Roles', COUNT(*) FROM [Identity].[Roles]
UNION ALL
SELECT 'Facilities', COUNT(*) FROM [Clinical].[Facilities];

-- List all databases on server (to see all environments)
SELECT name, create_date
FROM sys.databases
WHERE name LIKE 'consilient%'
ORDER BY name;
```

### Azure Portal

Monitor your database:
- **Azure Portal → SQL Database → Query editor** (preview)
- **Azure Portal → SQL Database → Metrics** (CPU, DTU, storage)
- **Azure Portal → SQL Database → Connection strings**

## Common Workflows

### Initial Setup
1. Create Azure SQL Server
2. Configure GitHub secrets for each environment
3. Run workflow for development with "Recreate database" checked
4. Verify all databases, tables and data
5. Deploy to staging

### Adding a New Database
1. Create directory: `src/.docker/Db/new_database_name/`
2. Add SQL scripts:
   ```bash
   mkdir src/.docker/Db/analytics
   echo "CREATE TABLE Reports (...)" > src/.docker/Db/analytics/01_tables.sql
   ```
3. Test locally (optional):
   ```bash
   sqlcmd -S localhost -U sa -P password -i src/.docker/Db/analytics/01_tables.sql
   ```
4. Commit and push to feature branch
5. Merge to main → workflow automatically discovers and deploys the new database!

### Updating Database Schema
1. Modify SQL files in `src/.docker/Db/{database_name}/`
2. Add new script with next number (e.g., `04_new_feature.sql`)
3. Test locally with Docker Compose or sqlcmd
4. Commit and push to feature branch
5. Merge to main → auto-deploys to staging
6. Manually deploy to production after testing

### Resetting All Development Databases
1. Run workflow
2. Environment: `development`
3. Recreate database: `true` (checked)
4. All databases will be dropped and recreated

### Resetting a Single Database (Manual)
1. Connect to Azure SQL Server
2. Drop specific database:
   ```sql
   DROP DATABASE [database_name];
   ```
3. Run workflow without recreate - it will recreate just that database

### Promoting to Production
1. Test in development
2. Deploy to staging and verify all databases
3. Manually trigger workflow for production
4. Monitor logs for each database deployment
5. Verify each database individually

## Additional Resources

- [Azure SQL Database Documentation](https://learn.microsoft.com/en-us/azure/azure-sql/)
- [sqlcmd Utility](https://learn.microsoft.com/en-us/sql/tools/sqlcmd)
- [GitHub Actions Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [Azure SQL Pricing](https://azure.microsoft.com/en-us/pricing/details/azure-sql-database/)
