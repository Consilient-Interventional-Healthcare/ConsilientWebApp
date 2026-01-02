# Deployment Guide for Consilient Applications

This guide covers the deployment process for the Consilient application suite to Azure.

## Overview

The Consilient project consists of multiple applications:
- **Consilient.Api** - Main REST API (.NET 9.0)
- **Consilient.WebApp2** - React frontend application
- **Consilient.BackgroundHost** - Background job processor

## Automated Deployment via GitHub Actions

### Consilient.Api

**Workflow**: `.github/workflows/deploy-api-azure.yml`

**Triggers**:
- Automatically on push to `main` branch
- Manually via workflow_dispatch

**Process**:
1. Builds Docker image from `Consilient.Api/Dockerfile`
2. Pushes image to Azure Container Registry (ACR)
3. Deploys image to Azure Web App for Containers

**Image Naming**: `consilientapi:v{run_number}-{sha}`

### Consilient.WebApp2

**Workflow**: `.github/workflows/deploy-webapp2-azure.yml`

**Triggers**:
- Automatically on push to any branch
- Manually via workflow_dispatch

## Required GitHub Secrets

Configure these secrets in GitHub repository settings (Settings → Secrets and variables → Actions):

| Secret Name | Description | Required For |
|------------|-------------|--------------|
| `AZURE_CREDENTIALS` | Azure Service Principal JSON credentials | All deployments |
| `ACR_REGISTRY` | Azure Container Registry login server (e.g., `myregistry.azurecr.io`) | All deployments |
| `ACR_USERNAME` | Azure Container Registry username | All deployments |
| `ACR_PASSWORD` | Azure Container Registry password | All deployments |
| `AZURE_API_WEBAPP_NAME` | Name of Azure Web App for Consilient.Api | API deployment |
| `AZURE_WEBAPP_NAME` | Name of Azure Web App for Consilient.WebApp2 | WebApp2 deployment |

### Creating Azure Service Principal

To create the `AZURE_CREDENTIALS` secret:

```bash
az ad sp create-for-rbac --name "github-actions-consilient" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
  --sdk-auth
```

The output JSON should be added as the `AZURE_CREDENTIALS` secret.

## Azure Resources Setup

### Consilient.Api - Azure Web App Configuration

#### Create Web App

```bash
# Create resource group (if not exists)
az group create --name consilient-rg --location eastus

# Create App Service Plan (Linux)
az appservice plan create \
  --name consilient-api-plan \
  --resource-group consilient-rg \
  --is-linux \
  --sku B1

# Create Web App for Containers
az webapp create \
  --name consilient-api-prod \
  --resource-group consilient-rg \
  --plan consilient-api-plan \
  --deployment-container-image-name mcr.microsoft.com/dotnet/aspnet:9.0
```

#### Required Application Settings

Configure these in Azure Portal (Web App → Configuration → Application settings) or via Azure CLI:

```bash
# Core settings
az webapp config appsettings set \
  --name consilient-api-prod \
  --resource-group consilient-rg \
  --settings \
    ASPNETCORE_ENVIRONMENT="Production" \
    WEBSITES_PORT="8090" \
    ASPNETCORE_HTTP_PORTS="80"

# Connection strings (as connection strings, not app settings)
az webapp config connection-string set \
  --name consilient-api-prod \
  --resource-group consilient-rg \
  --connection-string-type SQLAzure \
  --settings \
    Default="Server=tcp:consilient-sql.database.windows.net,1433;Initial Catalog=consilient_main;Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
    Hangfire="Server=tcp:consilient-sql.database.windows.net,1433;Initial Catalog=consilient_hangfire;Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**Additional settings to configure** (adjust values as needed):
- `AllowedOrigins__0` - Frontend URL (e.g., `https://consilient-webapp2.azurewebsites.net`)
- `AllowedOrigins__1` - Additional CORS origins if needed
- `ApplicationSettings__*` - Any application-specific settings
- `Logging__LogLevel__Default` - Logging level (e.g., `Information`)

#### Configure Container Registry

```bash
# Enable ACR authentication
az webapp config container set \
  --name consilient-api-prod \
  --resource-group consilient-rg \
  --docker-custom-image-name {acr-registry}/consilientapi:latest \
  --docker-registry-server-url https://{acr-registry} \
  --docker-registry-server-user {acr-username} \
  --docker-registry-server-password {acr-password}
```

### Health Checks and Monitoring

#### Enable Health Check

```bash
az webapp config set \
  --name consilient-api-prod \
  --resource-group consilient-rg \
  --health-check-path "/health"
```

#### Enable Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app consilient-api-insights \
  --location eastus \
  --resource-group consilient-rg

# Get instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app consilient-api-insights \
  --resource-group consilient-rg \
  --query instrumentationKey -o tsv)

# Configure Web App
az webapp config appsettings set \
  --name consilient-api-prod \
  --resource-group consilient-rg \
  --settings \
    APPINSIGHTS_INSTRUMENTATIONKEY="$INSTRUMENTATION_KEY" \
    ApplicationInsightsAgent_EXTENSION_VERSION="~3"
```

## Local Docker Build and Test

### Consilient.Api

Build the Docker image locally:

```bash
# From the repository root
cd src

# Build the image
docker build -t consilientapi:local -f Consilient.Api/Dockerfile .

# Run locally with environment variables
docker run -p 8090:8090 -p 8091:8091 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__Default="Server=host.docker.internal,1434;Database=consilient_main;User Id=sa;Password=YourPassword;TrustServerCertificate=True" \
  -e ConnectionStrings__Hangfire="Server=host.docker.internal,1434;Database=consilient_hangfire;User Id=sa;Password=YourPassword;TrustServerCertificate=True" \
  consilientapi:local

# Test the API
curl http://localhost:8090/health
curl http://localhost:8090/swagger
```

### Using Docker Compose (Development)

For full local development with all services:

```bash
cd src/.docker
docker-compose up -d
```

This starts:
- Database (SQL Server 2022)
- Consilient.Api
- Consilient.WebApp2
- Consilient.BackgroundHost
- Loki, Promtail, Grafana (logging/monitoring)
- Ollama (LLM services)

## Deployment Process

### Standard Deployment

1. **Develop and test locally**
2. **Create pull request** to `main` branch
3. **Review and merge** PR
4. **Automatic deployment** triggered on merge to `main`
5. **Monitor GitHub Actions** workflow for any errors
6. **Verify deployment** by checking:
   - Azure Portal → Web App → Log stream
   - API health endpoint: `https://{webapp-name}.azurewebsites.net/health`
   - API documentation: `https://{webapp-name}.azurewebsites.net/swagger`

### Manual Deployment

To manually trigger a deployment:

1. Go to GitHub repository
2. Click **Actions** tab
3. Select **Build and deploy Consilient.Api to Azure** workflow
4. Click **Run workflow**
5. Select branch (typically `main`)
6. Click **Run workflow**

## Rollback Procedure

If a deployment causes issues:

### Via Azure Portal

1. Navigate to Azure Portal → Web App → Deployment Center
2. Under **Logs**, find the last successful deployment
3. Note the image tag (e.g., `v122-abc123d`)
4. Go to **Container settings**
5. Update **Full Image Name and Tag** to the previous version
6. Click **Save**
7. Restart the Web App

### Via Azure CLI

```bash
# List recent deployments from ACR
az acr repository show-tags \
  --name {acr-name} \
  --repository consilientapi \
  --top 10 \
  --orderby time_desc

# Deploy specific version
az webapp config container set \
  --name consilient-api-prod \
  --resource-group consilient-rg \
  --docker-custom-image-name {acr-registry}/consilientapi:v122-abc123d

# Restart the app
az webapp restart \
  --name consilient-api-prod \
  --resource-group consilient-rg
```

## Troubleshooting

### Deployment Fails at Build Step

**Symptoms**: GitHub Actions workflow fails during Docker build

**Solutions**:
1. Check build logs in GitHub Actions
2. Verify all project references in Consilient.Api.csproj exist
3. Test build locally: `docker build -f src/Consilient.Api/Dockerfile src`
4. Check .dockerignore isn't excluding required files

### Deployment Succeeds but App Won't Start

**Symptoms**: Deployment completes but web app shows error

**Solutions**:
1. Check Azure Web App logs: Portal → Log stream
2. Verify `WEBSITES_PORT=8090` is configured
3. Check connection strings are set correctly
4. Verify database is accessible from Azure
5. Check Application Insights for exceptions

### Database Connection Errors

**Symptoms**: API starts but can't connect to database

**Solutions**:
1. Verify connection string format is correct
2. Check SQL Server firewall allows Azure services
3. Verify SQL credentials are correct
4. Test connection from Azure Cloud Shell:
   ```bash
   sqlcmd -S {server}.database.windows.net -d {database} -U {user} -P {password}
   ```

### CORS Errors

**Symptoms**: Frontend can't call API endpoints

**Solutions**:
1. Add frontend URL to `AllowedOrigins__0` setting
2. Verify CORS middleware is configured in Program.cs
3. Check browser developer console for specific CORS error
4. Ensure protocol (http/https) matches in CORS origin

## Monitoring and Alerts

### Key Metrics to Monitor

- **HTTP 5xx errors** - Server errors
- **Response time** - API performance
- **Request rate** - Traffic patterns
- **CPU/Memory usage** - Resource utilization

### Setting Up Alerts

```bash
# Create action group for notifications
az monitor action-group create \
  --name consilient-alerts \
  --resource-group consilient-rg \
  --short-name "ConAlerts" \
  --email-receiver name="DevTeam" email="devteam@example.com"

# Create alert rule for HTTP 5xx errors
az monitor metrics alert create \
  --name "API-HighErrorRate" \
  --resource-group consilient-rg \
  --scopes "/subscriptions/{subscription-id}/resourceGroups/consilient-rg/providers/Microsoft.Web/sites/consilient-api-prod" \
  --condition "count Http5xx > 10" \
  --window-size 5m \
  --evaluation-frequency 1m \
  --action consilient-alerts
```

## Security Best Practices

1. **Use Azure Key Vault** for sensitive configuration (connection strings, API keys)
2. **Enable HTTPS only** in Azure Web App settings
3. **Configure minimum TLS version** to 1.2
4. **Use Managed Identity** where possible instead of connection strings
5. **Regularly rotate secrets** in GitHub and Azure
6. **Enable Azure Security Center** for vulnerability scanning
7. **Review and audit** Application Insights logs regularly

## CI/CD Workflow Details

### Build Context Difference

**Important**: The Consilient.Api Dockerfile requires the **entire solution directory** as build context:

```yaml
context: ./src                          # Solution root, NOT ./src/Consilient.Api
file: ./src/Consilient.Api/Dockerfile   # Dockerfile path
```

This is because the API has dependencies on 14+ other projects in the solution that must be available during the build.

### Image Tagging Strategy

Images are tagged with: `v{run_number}-{short_sha}`

- **run_number**: Sequential GitHub Actions run number (e.g., 123)
- **short_sha**: First 7 characters of git commit SHA (e.g., a1b2c3d)

Example: `consilientapi:v123-a1b2c3d`

Benefits:
- Unique identifier for each build
- Traceable to specific commit
- Easy to identify and rollback

## Additional Resources

- [Azure Web Apps Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Docker Multi-Stage Builds](https://docs.docker.com/build/building/multi-stage/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [ASP.NET Core Deployment](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/)
