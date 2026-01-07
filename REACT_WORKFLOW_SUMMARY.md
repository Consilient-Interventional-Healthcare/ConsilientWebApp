# React Apps Workflow Summary

## Overview
The `react-apps.yml` workflow handles building, deploying, and validating React applications to Azure Web App for Containers using Docker and GitHub Actions.

---

## Job: build-and-deploy

### Purpose
Builds a Docker image of the React application and deploys it to Azure Web App for Containers.

### Configuration
- **Name**: Build and Deploy React Application
- **Runner**: `ubuntu-latest`
- **Timeout**: 45 minutes
- **Container**: Uses custom runner image passed via `inputs.runner_image`
- **Environment**: Configurable via `inputs.environment`

### Outputs
- `react_app_name`: The name of the deployed React App Service
- `previous_image`: The container image that was previously deployed (for rollback)

### Key Steps

| Step | Action | Purpose |
|------|--------|---------|
| **Checkout** | `actions/checkout@v4` | Clones repository code |
| **Validate environment input** | Custom action `./.github/actions/validate-inputs` | Validates environment parameter |
| **Validate Deployment Configuration** | Shell script check | Ensures required variables are set (REACT_APP_NAME, ACR_REGISTRY) |
| **Set up Docker Buildx** | `docker/setup-buildx-action@v3` | Prepares Docker for multi-platform builds |
| **Login to Azure (OIDC + act fallback)** | Custom action `./.github/actions/azure-login` | Authenticates to Azure using OIDC or client secret fallback |
| **Log in to ACR (via Azure CLI)** | `az acr login` command | Logs into Azure Container Registry |
| **Capture Previous Deployment** | Azure CLI query | Retrieves the previous container image for rollback capability |
| **Build and push Docker image** | `docker/build-push-action@v5` | Builds Docker image from `./src/Consilient.WebApp2/Dockerfile` and pushes to ACR |
| **Deploy image to Azure Web App** | `azure/webapps-deploy@v3` | Deploys the new Docker image to Azure Web App |

### Docker Build Details
- **Context**: `./src/Consilient.WebApp2`
- **Dockerfile**: `./src/Consilient.WebApp2/Dockerfile`
- **Build Args**: `BUILD_CONFIGURATION=Release`
- **Image Naming**: `{ACR_REGISTRY}/{react_image_name}:{IMAGE_TAG}`
  - Example: `consilientacr.azurecr.io/consilient-react:v123-abc1234def5678`
- **Caching**: Uses GitHub Actions cache (`type=gha`) for faster builds

### Environment Variables
- `IMAGE_NAME`: React Docker image name (default: `consilient-react`)
- `IMAGE_TAG`: `v{run_number}-{commit_sha}` (e.g., `v42-abc1234`)
- `REACT_APP_NAME`: Web App Service name (from input > secret fallback)
- `ACR_REGISTRY`: Azure Container Registry URL
- `RESOURCE_GROUP`: Azure Resource Group for the Web App

---

## Job: health-check-react

### Purpose
Performs post-deployment validation using Lighthouse performance testing and automatically rolls back if thresholds are not met.

### Configuration
- **Name**: Check React App Health
- **Runner**: `ubuntu-latest`
- **Dependencies**: Runs after `build-and-deploy` job succeeds
- **Condition**: Only runs if `build-and-deploy` completed successfully
- **Container**: Uses custom runner image (same as build-and-deploy)
- **Environment**: Configurable via `inputs.environment`
- **Failure Handling**: Non-blocking on dev environment (`continue-on-error: true` when `environment == 'dev'`)

### Workflow
1. **Checkout Code**: Prepares repository for health check actions
2. **Lighthouse Health Check with Rollback**: Executes comprehensive health assessment

### Health Check Parameters

| Parameter | Value | Purpose |
|-----------|-------|---------|
| **App URL** | `https://{react_app_name}.azurewebsites.net` | The deployed application endpoint |
| **App Name** | From `build-and-deploy` output | Identifies the Web App instance |
| **Wait Seconds** | 45 | Time allowed for app to stabilize before testing |
| **Lighthouse Runs** | 2 | Number of audit iterations for consistent results |
| **Min Performance Score** | 0.7 (70%) | Minimum acceptable Lighthouse performance |
| **Min Accessibility Score** | 0.9 (90%) | Minimum acceptable accessibility compliance |
| **Min Best Practices Score** | 0.7 (70%) | Minimum acceptable best practices adherence |
| **Min SEO Score** | 0.7 (70%) | Minimum acceptable SEO compliance |

### Health Check Action
- **Location**: `./.github/actions/health-check-lighthouse`
- **Capabilities**:
  - Runs Lighthouse audits for performance, accessibility, best practices, and SEO
  - Authenticates to Azure for rollback capability
  - Automatically rolls back to previous image if thresholds are not met
  - Compares current deployment against defined quality standards

### Rollback Support
The health check has access to:
- Previous image reference from `build-and-deploy` step
- ACR registry URL for retrieving previous image
- Azure credentials for triggering rollback deployment

---

## Workflow Dependencies

```
build-and-deploy (build & deploy container)
        ↓
        (if successful)
        ↓
health-check-react (validate performance, rollback if needed)
```

---

## Key Features

### Security
- OIDC authentication to Azure (preferred)
- Client secret fallback for local testing with `act`
- Minimal permissions: `contents: read`, `id-token: write`, `packages: read`

### Reliability
- Deployment concurrency control (one deployment per environment at a time)
- Previous image capture for rollback capability
- Health checks with automatic rollback on quality threshold failures
- Configurable thresholds for dev environment (non-blocking failures)

### Performance
- GitHub Actions cache for Docker builds
- Multi-stage Docker builds (via Buildx)
- Shallow clone (fetch-depth: 1) to reduce checkout time

### Flexibility
- Configurable deployment environment
- Terraform-first approach with fallback to manual inputs
- Adjustable Lighthouse quality thresholds
- Custom runner image support for different configurations
