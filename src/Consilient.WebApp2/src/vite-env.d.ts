/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
  readonly VITE_APP_ENV: string;
  readonly VITE_ENABLE_DEBUG_MODE?: string;
  readonly VITE_OAUTH_CLIENT_ID?: string;
  readonly VITE_OAUTH_AUTHORITY?: string;
  readonly VITE_OAUTH_REDIRECT_URI?: string;
  readonly VITE_ENABLE_REMOTE_LOGGING?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}