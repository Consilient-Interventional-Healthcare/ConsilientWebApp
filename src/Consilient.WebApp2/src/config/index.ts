// --- Types & Enums ---
export enum Environment {
  Development = 'development',
  Staging = 'staging',
  Production = 'production',
}

export enum LogLevel {
  Trace = 'trace',
  Debug = 'debug',
  Info = 'info',
  Warn = 'warn',
  Error = 'error',
}

export interface MockServicesConfig {
  auth: boolean;
  employees: boolean;
  dailyLog: boolean;
  appSettings: boolean;
  // Add more services as needed
}

export interface AppSettings {
  api: {
    baseUrl: string;
    timeout: number;
    retryAttempts: number;
  };
  features: {
    useMockServices: boolean; // Global override - if true, all services use mocks
    mockServices: MockServicesConfig; // Granular control per service
    enableRemoteLogging: boolean;
    enableExternalLoginMock: boolean;
  };
  app: {
    name: string;
    version: string;
    environment: Environment;
    isDevelopment: boolean;
    isProduction: boolean;
    isDebugMode: boolean;
  };
  logging: {
    enabled: boolean;
    logsEndpoint: string;
    level: LogLevel;
  };
}

// --- Global Window Type ---
declare global {
  interface Window {
    __ENV?: {
      APP_API_BASE_URL?: string;
      APP_ENV?: string;
      APP_ENABLE_DEBUG_MODE?: string;
      APP_USE_MOCK_SERVICES?: string;
      APP_MOCK_AUTH_SERVICE?: string;
      APP_MOCK_EMPLOYEES_SERVICE?: string;
      APP_MOCK_DAILY_LOG_SERVICE?: string;
      APP_MOCK_APP_SETTINGS_SERVICE?: string;
      APP_ENABLE_REMOTE_LOGGING?: string;
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
  const apiBaseUrl = getEnv('APP_API_BASE_URL') ?? '/api';
  const appEnv = validateAppEnv(getEnv('APP_ENV'));
  const enableDebugMode = getEnv('APP_ENABLE_DEBUG_MODE') === 'true';
  const useMockServices = getEnv('APP_USE_MOCK_SERVICES') === 'true';
  const enableExternalLoginMock = getEnv('APP_ENABLE_EXTERNAL_LOGIN_MOCK') === 'true';

  const isProduction = appEnv === Environment.Production;
  const isDevelopment = appEnv === Environment.Development;
  const isDebugMode = enableDebugMode || isDevelopment;
  const enableRemoteLogging = isProduction || getEnv('APP_ENABLE_REMOTE_LOGGING') === 'true';

  // Granular mock service configuration
  // If useMockServices is true, all services use mocks unless explicitly set to false
  // If useMockServices is false, individual services can still be mocked via specific env vars
  const mockServices = {
    auth: useMockServices || getEnv('APP_MOCK_AUTH_SERVICE') === 'true',
    employees: useMockServices || getEnv('APP_MOCK_EMPLOYEES_SERVICE') === 'true',
    dailyLog: useMockServices || getEnv('APP_MOCK_DAILY_LOG_SERVICE') === 'true',
    appSettings: useMockServices || getEnv('APP_MOCK_APP_SETTINGS_SERVICE') === 'true',
  } satisfies MockServicesConfig;

  const appSettings: AppSettings = {
    api: {
      baseUrl: apiBaseUrl,
      timeout: 10000,
      retryAttempts: 3,
    },
    features: {
      useMockServices,
      mockServices,
      enableRemoteLogging,
      enableExternalLoginMock,
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
      level: LogLevel.Info,
    },
  };

  // Log in development
  if (isDevelopment) {
    console.log('🔧 App Settings:', {
      environment: appEnv,
      apiBaseUrl,
      isDevelopment,
      isProduction,
      isDebugMode,
      useMockServices,
      mockServices: appSettings.features.mockServices,
      features: appSettings.features
    });
  }

  return appSettings;
}

// --- Export Singleton ---
const appSettings: AppSettings = createAppSettings();
export { appSettings };
export default appSettings;
