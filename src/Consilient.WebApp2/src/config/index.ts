
// --- Types & Enums ---
export enum Environment {
  Development = 'development',
  Staging = 'staging',
  Production = 'production',
}

export interface AppSettings {
  api: {
    baseUrl: string;
    timeout: number;
    retryAttempts: number;
  };
  features: {
    disableAuth: boolean;
    useMockServices: boolean;
    enableRemoteLogging: boolean;
  };
  app: {
    name: string;
    version: string;
    environment: Environment;
    isDevelopment: boolean;
    isProduction: boolean;
    isDebugMode: boolean;
  };
  msal?: {
    clientId: string;
    tenantId: string;
    authority: string;
    redirectUri: string;
    scopes: string[];
  };
  logging: {
    enabled: boolean;
    logsEndpoint: string;
    level: 'trace' | 'debug' | 'info' | 'warn' | 'error';
  };
}

// --- Global Window Type ---
declare global {
  interface Window {
    __ENV?: {
      API_BASE_URL?: string;
      APP_ENV?: string;
      ENABLE_DEBUG_MODE?: string;
      DISABLE_AUTH?: string;
      USE_MOCK_SERVICES?: string;
      MSAL_CLIENT_ID?: string;
      MSAL_TENANT_ID?: string;
      MSAL_AUTHORITY?: string;
      MSAL_REDIRECT_URI?: string;
      MSAL_SCOPES?: string;
      ENABLE_REMOTE_LOGGING?: string;
    };
  }
}

// --- Internal Helpers ---
function getEnv(key: string): string | undefined {
  return typeof window !== 'undefined' && window.__ENV
    ? window.__ENV[key as keyof typeof window.__ENV]
    : undefined;
}

function validateAppEnv(value: string | undefined): Environment {
  const env = value ?? Environment.Development;
  if (!Object.values(Environment).includes(env as Environment)) {
    throw new Error(
      `Invalid APP_ENV value: "${env}"\nMust be one of: ${Object.values(Environment).join(', ')}`
    );
  }
  return env as Environment;
}

// --- Factory & Singleton ---
function createAppSettings(): AppSettings {
  const apiBaseUrl = getEnv('API_BASE_URL') ?? '/api';
  const appEnv = validateAppEnv(getEnv('APP_ENV'));
  const enableDebugMode = getEnv('ENABLE_DEBUG_MODE') === 'true';
  const disableAuth = getEnv('DISABLE_AUTH') === 'true';
  const useMockServices = getEnv('USE_MOCK_SERVICES') === 'true';

  const isProduction = appEnv === Environment.Production;
  const isDevelopment = appEnv === Environment.Development;
  const isDebugMode = enableDebugMode || isDevelopment;
  const enableRemoteLogging = isProduction || getEnv('ENABLE_REMOTE_LOGGING') === 'true';

  // Build MSAL configuration if available
  const clientId = getEnv('MSAL_CLIENT_ID');
  const tenantId = getEnv('MSAL_TENANT_ID');
  const authority = getEnv('MSAL_AUTHORITY');
  const redirectUri = getEnv('MSAL_REDIRECT_URI');
  const scopes = getEnv('MSAL_SCOPES');

  const appSettings: AppSettings = {
    api: {
      baseUrl: apiBaseUrl,
      timeout: 10000,
      retryAttempts: 3,
    },
    features: {
      disableAuth,
      useMockServices,
      enableRemoteLogging,
    },
    app: {
      name: 'Consilient',
      version: '1.0.0',
      environment: appEnv,
      isDevelopment,
      isProduction,
      isDebugMode,
    },
    logging: {
      enabled: enableRemoteLogging,
      logsEndpoint: '/loki/logs',
      level: 'info',
    },
  };

  if (clientId && tenantId && authority && redirectUri) {
    appSettings.msal = {
      clientId,
      tenantId,
      authority,
      redirectUri,
      scopes: scopes ? scopes.split(',').map(s => s.trim()) : ['User.Read'],
    };
  }

  // Log in development
  if (isDevelopment) {
    console.log('🔧 App Settings:', {
      environment: appEnv,
      apiBaseUrl,
      isDevelopment,
      isProduction,
      isDebugMode,
      features: appSettings.features,
      hasMsal: !!appSettings.msal,
    });
  }

  return appSettings;
}

// --- Export Singleton ---
const appSettings: AppSettings = createAppSettings();
export { appSettings };
export default appSettings;
