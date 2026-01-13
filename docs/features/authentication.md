# Authentication

## Overview

The Consilient application supports two methods for users to authenticate:

1. **Local Authentication** - Traditional username and password login managed directly by the application
2. **Microsoft Entra (Azure AD)** - Single Sign-On (SSO) using your organization's Microsoft account

Both methods provide secure access to the application. Local authentication is ideal for development, testing, and users without Microsoft accounts. Microsoft Entra authentication is recommended for production environments as it leverages your organization's existing identity management and security policies.

---

## User Roles

The application uses role-based access control (RBAC) to manage permissions. Each user is assigned one or more roles that determine what actions they can perform.

| Role | Description |
|------|-------------|
| **Administrator** | Full system access. Can manage users, configure settings, and access all features. |
| **Provider** | Healthcare provider role. Access to clinical features and patient management. |
| **Nurse** | Nursing staff role. Access to patient care features and clinical documentation. |

Roles are assigned when creating a user account and can be modified by administrators.

---

## Local Authentication

### How It Works

Local authentication allows users to log in with a username and password stored in the application's database. When a user submits their credentials:

1. The application looks up the user by username
2. The password is verified against the stored hash
3. If valid, a secure token (JWT) is generated and returned
4. The token is stored in a secure cookie for subsequent requests

### Password Policy

Passwords must meet the following requirements:

| Requirement | Value |
|-------------|-------|
| Minimum length | 8 characters |
| Uppercase letter | Required |
| Lowercase letter | Required |
| Digit (0-9) | Required |
| Special character | Not required |

### Password Security

Passwords are never stored in plain text. The application uses **PBKDF2** (Password-Based Key Derivation Function 2) for password hashing, which is the industry-standard algorithm provided by ASP.NET Core Identity. This algorithm:

- Applies multiple iterations of hashing to slow down brute-force attacks
- Uses a unique salt for each password
- Is resistant to rainbow table attacks

### Database Schema

User authentication data is stored in the `[Identity]` schema:

**Users Table** (`[Identity].[Users]`)

| Column | Description |
|--------|-------------|
| Id | Unique user identifier |
| UserName | Login username (unique) |
| Email | User email address (unique) |
| PasswordHash | PBKDF2 hashed password |
| EmailConfirmed | Whether email has been verified |
| LockoutEnabled | Whether account lockout is enabled |
| AccessFailedCount | Failed login attempt counter |

**Roles Table** (`[Identity].[Roles]`)

| Column | Description |
|--------|-------------|
| Id | Role identifier |
| Name | Role name (Administrator, Nurse, Provider) |

**User Roles** (`[Identity].[UserRoles]`)

| Column | Description |
|--------|-------------|
| UserId | Foreign key to Users |
| RoleId | Foreign key to Roles |

### Development Environment Users

For local development and testing, the following users are pre-configured:

| Username | Password | Role | Description |
|----------|----------|------|-------------|
| `administrator@local` | `administrator@local` | Administrator | Full system access |
| `nurse@local` | `nurse@local` | Nurse | Nursing staff access |
| `provider@local` | `provider@local` | Provider | Healthcare provider access |

> **Note:** These credentials are for development only. Production environments should use strong, unique passwords or Microsoft Entra authentication.

---

## Microsoft Entra Authentication

### How It Works

Microsoft Entra (formerly Azure AD) authentication enables Single Sign-On (SSO), allowing users to log in with their organizational Microsoft account. The authentication flow:

1. User clicks "Sign in with Microsoft"
2. User is redirected to Microsoft's login page
3. After successful Microsoft authentication, user is redirected back to the application
4. The application validates the response and creates a session
5. If the user doesn't exist locally, they can be auto-provisioned (if enabled)

This provides a seamless experience for users who are already signed into Microsoft 365 or other Microsoft services.

### Registering the Application in Microsoft Entra

To enable Microsoft authentication, you must register the application in your Azure portal:

1. **Navigate to Azure Portal**
   - Go to [Azure Portal](https://portal.azure.com)
   - Select "Microsoft Entra ID" (or "Azure Active Directory")

2. **Create App Registration**
   - Go to "App registrations" > "New registration"
   - Name: Choose a descriptive name (e.g., "Consilient Web App")
   - Supported account types: Select based on your requirements
     - "Single tenant" for your organization only
     - "Multitenant" for multiple organizations
   - Redirect URI: Add your application's callback URL
     - Type: Web
     - URL: `https://your-app-domain/auth/microsoft/callback`

3. **Configure Authentication**
   - Go to "Authentication" section
   - Verify redirect URIs are correct
   - Enable "ID tokens" under Implicit grant

4. **Create Client Secret**
   - Go to "Certificates & secrets"
   - Click "New client secret"
   - Add a description and select expiration
   - **Important:** Copy the secret value immediately (it won't be shown again)

5. **Note the Required Values**
   - Application (client) ID - found on Overview page
   - Directory (tenant) ID - found on Overview page
   - Client secret - from step 4

### API Configuration

Configure the following settings in your API's configuration (appsettings.json or environment variables):

```json
{
  "ApplicationSettings": {
    "Authentication": {
      "UserService": {
        "AutoProvisionUser": false,
        "AllowedEmailDomains": ["yourdomain.com"],
        "OAuth": {
          "Enabled": true,
          "ProviderName": "Microsoft",
          "Authority": "https://login.microsoftonline.com",
          "TenantId": "<your-tenant-id>",
          "ClientId": "<your-client-id>",
          "ClientSecret": "<your-client-secret>",
          "Scopes": ["openid", "profile", "email"]
        }
      }
    }
  }
}
```

| Setting | Description |
|---------|-------------|
| `Enabled` | Set to `true` to enable Microsoft authentication |
| `Authority` | Microsoft identity endpoint (typically `https://login.microsoftonline.com`) |
| `TenantId` | Your Azure AD tenant ID |
| `ClientId` | Application ID from Azure app registration |
| `ClientSecret` | Client secret from Azure app registration |
| `Scopes` | OAuth scopes (default: openid, profile, email) |

### Auto-Provisioning Users

The `AutoProvisionUser` setting controls whether new users are automatically created when they authenticate via Microsoft Entra for the first time:

| Value | Behavior |
|-------|----------|
| `false` (default) | Users must exist in the application database before they can log in. Administrators must create user accounts manually. |
| `true` | New users are automatically created when they authenticate via Microsoft for the first time. Their email becomes their username. |

**When to enable auto-provisioning:**
- When you want all employees in your organization to access the application without manual account creation
- When combined with domain restrictions to control who can access the application

**When to disable auto-provisioning:**
- When you need to pre-configure user roles or permissions before granting access
- When only specific users should have access to the application

### Domain Restrictions

The `AllowedEmailDomains` setting restricts which email domains can access the application. This provides an additional security layer on top of Microsoft Entra authentication.

**Examples:**

```json
// Only allow users from your company domain
"AllowedEmailDomains": ["yourcompany.com"]

// Allow multiple domains
"AllowedEmailDomains": ["yourcompany.com", "subsidiary.com"]

// Allow all domains (not recommended for production)
"AllowedEmailDomains": ["*"]
```

Users whose email domain is not in the allowed list will see an "Email domain not allowed" error, even if they successfully authenticate with Microsoft.

---

## Known Issues and Future Enhancements

### Known Issues

*No known issues at this time.*

### Future Enhancements

The following features are planned for future releases:

| Enhancement | Description |
|-------------|-------------|
| **Password Reset** | Allow users to reset their password via a secure email link when they forget their credentials. |
| **Authentication Emails** | Send email notifications for authentication events such as password changes, new device logins, and account lockouts. |

<!--
AI_CONTEXT: Key source files for authentication implementation:
- src/Databases/consilient_main/seed.sql - Development user seed data
- src/Databases/consilient_main/01_identity.sql - Database schema
- src/Consilient.Users.Services/PasswordPolicyOptions.cs - Password policy configuration
- src/Consilient.Users.Services/UserServiceConfiguration.cs - User service settings
- src/Consilient.Data/Entities/Identity/RoleNames.cs - Role definitions
-->
