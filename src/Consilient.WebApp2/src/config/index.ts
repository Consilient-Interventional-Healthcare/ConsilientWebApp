/**
 * Application configuration
 * Centralized configuration for the application
 */

import { env, isProduction } from '@/config/env';

const appConfig = {
  // API Configuration
  api: {
    baseUrl: env.apiBaseUrl,
    timeout: 10000, // 10 seconds
    retryAttempts: 3,
  },

  // Feature Flags
  features: {
    enableAnalytics: env.features.enableAnalytics,
    enableDebugMode: env.features.enableDebugMode,
  },

  // OAuth Configuration (if available)
  oauth: env.oauth,

  // App Metadata
  app: {
    name: 'Consilient',
    version: '1.0.0',
    environment: env.appEnv,
  },

  // Logging Configuration
  logging: {
    // In production, always enabled. In dev, controlled by VITE_ENABLE_REMOTE_LOGGING flag
    enabled: isProduction || import.meta.env.VITE_ENABLE_REMOTE_LOGGING === 'true',
  },
};

export { appConfig as config };
export default appConfig;
