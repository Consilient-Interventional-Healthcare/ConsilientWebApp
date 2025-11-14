/**
 * Environment variable validation and configuration
 * This module ensures all required environment variables are present at startup
 */

interface EnvConfig {
  apiBaseUrl: string;
  appEnv: 'development' | 'staging' | 'production';
  features: {
    enableAnalytics: boolean;
    enableDebugMode: boolean;
  };
  oauth?: {
    clientId: string;
    authority: string;
    redirectUri: string;
  };
}

/**
 * Validates that a required environment variable exists
 */
function getRequiredEnv(key: keyof ImportMetaEnv): string {
  const value = import.meta.env[key] as string | undefined;
  if (!value) {
    throw new Error(
      `Missing required environment variable: ${key}\n` +
      `Please check your .env file and ensure ${key} is set.\n` +
      `See .env.example for reference.`
    );
  }
  return value;
}

/**
 * Gets an optional environment variable with a default value
 */
function getOptionalEnv(key: keyof ImportMetaEnv, defaultValue: string): string {
  const value = import.meta.env[key] as string | undefined;
  return value ?? defaultValue;
}

/**
 * Validates and returns the application configuration
 * Throws an error if required variables are missing
 */
function validateEnv(): EnvConfig {
  // Required variables
  const apiBaseUrl = getRequiredEnv('VITE_API_BASE_URL');
  const appEnv = getOptionalEnv('VITE_APP_ENV', 'development');

  // Validate appEnv is one of the allowed values
  if (!['development', 'staging', 'production'].includes(appEnv)) {
    throw new Error(
      `Invalid VITE_APP_ENV value: "${appEnv}"\n` +
      `Must be one of: development, staging, production`
    );
  }

  // Optional feature flags
  const enableAnalytics = getOptionalEnv('VITE_ENABLE_ANALYTICS', 'false') === 'true';
  const enableDebugMode = getOptionalEnv('VITE_ENABLE_DEBUG_MODE', 'false') === 'true';

  // Optional OAuth configuration
  const oauthClientId = import.meta.env.VITE_OAUTH_CLIENT_ID;
  const oauthAuthority = import.meta.env.VITE_OAUTH_AUTHORITY;
  const oauthRedirectUri = import.meta.env.VITE_OAUTH_REDIRECT_URI;

  const config: EnvConfig = {
    apiBaseUrl,
    appEnv: appEnv as 'development' | 'staging' | 'production',
    features: {
      enableAnalytics,
      enableDebugMode,
    },
  };

  // Add OAuth config if all OAuth variables are present
  if (oauthClientId && oauthAuthority && oauthRedirectUri) {
    config.oauth = {
      clientId: oauthClientId,
      authority: oauthAuthority,
      redirectUri: oauthRedirectUri,
    };
  }

  return config;
}

/**
 * Application configuration - validated at startup
 * This will throw an error if required environment variables are missing
 */
export const env = validateEnv();

/**
 * Helper to check if we're in development mode
 */
export const isDevelopment = env.appEnv === 'development';

/**
 * Helper to check if we're in production mode
 */
export const isProduction = env.appEnv === 'production';

/**
 * Helper to check if debug mode is enabled
 */
export const isDebugMode = env.features.enableDebugMode || isDevelopment;

// Log configuration in development (excluding sensitive data)
if (isDevelopment) {
  // Use console.log here to avoid circular dependency with logger
  // This runs before logger is initialized
  console.log('🔧 Environment Configuration:', {
    appEnv: env.appEnv,
    apiBaseUrl: env.apiBaseUrl,
    features: env.features,
    hasOAuth: !!env.oauth,
  });
}
