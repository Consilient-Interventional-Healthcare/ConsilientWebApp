# BackgroundHost Hangfire Dashboard Authentication

## Overview

The BackgroundHost service hosts the Hangfire dashboard for background job monitoring. Access to this dashboard is protected using Microsoft Entra (Azure AD) authentication when running in Azure.

| Environment | Authentication |
|-------------|----------------|
| **Local Development** | No authentication (direct access) |
| **Azure (Production/Dev)** | Microsoft Entra SSO |

---

## Azure App Registration Setup

### Step 1: Navigate to App Registrations

1. Go to [Azure Portal](https://portal.azure.com)
2. Search for **"Microsoft Entra ID"** in the top search bar
3. Click **"App registrations"** in the left sidebar
4. Click **"+ New registration"** button

### Step 2: Register the Application

| Field | Value |
|-------|-------|
| **Name** | `Consilient-BackgroundHost` |
| **Supported account types** | Single tenant (your organization only) |
| **Redirect URI** | Leave blank (add in next step) |

Click **"Register"**

### Step 3: Configure Redirect URIs

1. Click **"Authentication"** in the left sidebar
2. Click **"+ Add a platform"** → Select **"Web"**
3. Add the following Redirect URIs:

| Environment | Redirect URI |
|-------------|--------------|
| **Production** | `https://consilient-bghost-prod-westus2.azurewebsites.net/signin-oidc` |
| **Dev** | `https://consilient-bghost-dev-westus2.azurewebsites.net/signin-oidc` |
| **Local Testing** | `https://localhost:8092/signin-oidc` |

4. Under **"Implicit grant and hybrid flows"**:
   - ✅ Check **ID tokens**

5. Click **"Save"**

### Step 4: Copy Application IDs

From the **Overview** page, copy:

| Value | Configuration Key |
|-------|-------------------|
| Application (client) ID | `ClientId` |
| Directory (tenant) ID | `TenantId` |

### Step 5: Create Client Secret

1. Click **"Certificates & secrets"**
2. Click **"+ New client secret"**
3. Description: `BackgroundHost-Production`
4. Expiration: 12 or 24 months
5. Click **"Add"**
6. **Copy the Value immediately** (won't be shown again)

| Value | Configuration Key |
|-------|-------------------|
| Secret Value | `ClientSecret` |

---

## Configuration

### Local Development (appsettings.local.json)

For local development **without** authentication:

```json
{
  "Authentication": {
    "ForceEntraAuth": false,
    "UserService": {
      "OAuth": {
        "Enabled": false
      }
    }
  }
}
```

For local testing **with** Entra authentication:

```json
{
  "Authentication": {
    "ForceEntraAuth": true,
    "UserService": {
      "OAuth": {
        "Enabled": true,
        "Authority": "https://login.microsoftonline.com/",
        "ProviderName": "AzureAD",
        "ClientId": "<your-client-id>",
        "ClientSecret": "<your-client-secret>",
        "TenantId": "<your-tenant-id>"
      }
    }
  }
}
```

> **Note:** Add `https://localhost:8092/signin-oidc` to your App Registration redirect URIs for local testing.

### Azure App Configuration (Production)

Add these keys with prefix `BackgroundHost:` (prefix is stripped when loaded):

| Key | Value |
|-----|-------|
| `BackgroundHost:Authentication:ForceEntraAuth` | `false` |
| `BackgroundHost:Authentication:UserService:OAuth:Enabled` | `true` |
| `BackgroundHost:Authentication:UserService:OAuth:Authority` | `https://login.microsoftonline.com/` |
| `BackgroundHost:Authentication:UserService:OAuth:ProviderName` | `AzureAD` |
| `BackgroundHost:Authentication:UserService:OAuth:ClientId` | `<client-id>` |
| `BackgroundHost:Authentication:UserService:OAuth:ClientSecret` | `@Microsoft.KeyVault(...)` |
| `BackgroundHost:Authentication:UserService:OAuth:TenantId` | `<tenant-id>` |

### Azure Key Vault (Secrets)

Store the client secret in Key Vault:

1. Go to your Key Vault in Azure Portal
2. Click **"Secrets"** → **"+ Generate/Import"**
3. Name: `BackgroundHost-ClientSecret`
4. Value: Your client secret from App Registration
5. Click **"Create"**

Reference in Azure App Configuration:
```
@Microsoft.KeyVault(SecretUri=https://<your-keyvault>.vault.azure.net/secrets/BackgroundHost-ClientSecret)
```

---

## Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ForceEntraAuth` | bool | `false` | Forces Entra auth even when not in Azure (for local testing) |
| `OAuth:Enabled` | bool | `false` | Enables/disables OAuth authentication |
| `OAuth:Authority` | string | - | Microsoft login endpoint |
| `OAuth:ClientId` | string | - | App Registration client ID |
| `OAuth:ClientSecret` | string | - | App Registration client secret |
| `OAuth:TenantId` | string | - | Azure AD tenant ID |

---

## How It Works

### Authentication Flow

1. User navigates to `/hangfire`
2. If not authenticated → Redirect to Microsoft login
3. User enters Microsoft credentials
4. Microsoft redirects back to `/signin-oidc` with auth code
5. Application validates and creates session cookie
6. User is redirected to Hangfire dashboard

### Environment Detection

The application automatically determines when to use Entra authentication:

```
Is Running in Azure? (WEBSITE_SITE_NAME env var exists)
    OR
ForceEntraAuth = true?
    AND
OAuth.Enabled = true?
    AND
ClientId, TenantId, ClientSecret all present?
    → Use Entra Authentication
    → Otherwise, no authentication required
```

---

## Troubleshooting

### "response_type 'id_token' is not enabled"

**Cause:** ID tokens not enabled in App Registration

**Fix:** Azure Portal → App Registration → Authentication → Enable "ID tokens"

### "redirect_uri does not match"

**Cause:** Redirect URI in config doesn't match App Registration

**Fix:** Ensure exact URL match including protocol, port, and path (`/signin-oidc`)

### "Unable to find required services"

**Cause:** Authorization services not registered (OAuth.Enabled is false)

**Fix:** Set `OAuth.Enabled: true` in configuration

### "EndpointRoutingMiddleware matches endpoints setup by EndpointMiddleware"

**Cause:** Missing `UseRouting()` middleware

**Fix:** Ensure `app.UseRouting()` is called before `UseAuthentication()` and `UseAuthorization()`

---

## Related Documentation

- [Authentication Overview](authentication.md) - General app authentication
- [Infrastructure Authentication](../infra/components/authentication.md) - CI/CD authentication

<!--
AI_CONTEXT: Key source files for BackgroundHost authentication:
- src/Consilient.BackgroundHost/Init/ConfigureEntraAuthenticationExtensions.cs - Entra auth setup
- src/Consilient.BackgroundHost/Init/ConfigureHangfireDashboardExtensions.cs - Hangfire dashboard config
- src/Consilient.BackgroundHost/Configuration/AuthenticationSettings.cs - Settings classes
- src/Consilient.BackgroundHost/Program.cs - Middleware configuration
-->
