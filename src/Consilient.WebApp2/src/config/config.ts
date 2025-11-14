/**
 * Application configuration
 * Centralized configuration for the application
 * 
 * This is the ONLY place where environment variables should be accessed.
 * All other modules should import `config` instead of `env` directly.
 */

import { env } from '@/config/env';

export const config = {
  // API Configuration
  api: {
    baseUrl: env.apiBaseUrl,
    timeout: 10000, // 10 seconds
    retryAttempts: 3,
  },

  // Feature Flags
  features: {
    enableDebugMode: env.features.enableDebugMode,
    enableRemoteLogging: env.isProduction || import.meta.env.VITE_ENABLE_REMOTE_LOGGING === 'true',
  },

  // App Metadata
  app: {
    name: 'Consilient',
    version: '1.0.0',
    environment: env.appEnv,
  },

  // Environment Checks (expose common checks)
  env: {
    isDevelopment: env.isDevelopment,
    isProduction: env.isProduction,
    isDebugMode: env.isDebugMode,
  },

  // MSAL Configuration
  msal: env.msal,

  // Logging Configuration
  logging: {
    // In production, always enabled. In dev, controlled by VITE_ENABLE_REMOTE_LOGGING flag
    enabled: env.isProduction || import.meta.env.VITE_ENABLE_REMOTE_LOGGING === 'true',
    // API endpoint for sending logs
    logsEndpoint: '/loki/logs',
    // Log level (will be overridden in dev to 'debug')
    level: 'info' as 'trace' | 'debug' | 'info' | 'warn' | 'error',
  },
};

export default config;
