/// <reference types="vite/client" />

interface ImportMetaEnv {
  // API Configuration
  readonly VITE_API_BASE_URL: string;
  
  // Application Environment
  readonly VITE_APP_ENV: 'development' | 'staging' | 'production';
  
  // Feature Flags
  readonly VITE_ENABLE_DEBUG_MODE?: string;
  readonly VITE_ENABLE_REMOTE_LOGGING?: string;
  
  // MS Entra ID (MSAL) Configuration
  readonly VITE_MSAL_CLIENT_ID?: string;
  readonly VITE_MSAL_TENANT_ID?: string;
  readonly VITE_MSAL_AUTHORITY?: string;
  readonly VITE_MSAL_REDIRECT_URI?: string;
  readonly VITE_MSAL_SCOPES?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
