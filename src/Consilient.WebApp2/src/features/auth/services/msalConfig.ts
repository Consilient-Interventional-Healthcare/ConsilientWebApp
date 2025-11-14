import type { Configuration, PopupRequest, RedirectRequest } from '@azure/msal-browser';
import { LogLevel } from '@azure/msal-browser';
import { config } from '@/config';

/**
 * MSAL Configuration
 * This configuration is used to initialize the MSAL instance
 * 
 * @see https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/configuration.md
 */
/**
 * Validate MSAL configuration and throw descriptive error if invalid
 */
const validateMsalConfig = () => {
  if (!config.msal) {
    throw new Error(
      'MSAL configuration is missing.\n' +
      'Please set the following environment variables:\n' +
      '  - VITE_MSAL_CLIENT_ID\n' +
      '  - VITE_MSAL_TENANT_ID\n' +
      '  - VITE_MSAL_AUTHORITY\n' +
      '  - VITE_MSAL_REDIRECT_URI\n' +
      'See docs/MS_ENTRA_AUTHENTICATION.md for setup instructions.'
    );
  }
  
  if (!config.msal.clientId) {
    throw new Error('VITE_MSAL_CLIENT_ID is required but not set');
  }
  
  if (!config.msal.authority) {
    throw new Error('VITE_MSAL_AUTHORITY is required but not set');
  }
  
  if (!config.msal.redirectUri) {
    throw new Error('VITE_MSAL_REDIRECT_URI is required but not set');
  }
};

// Validate config on module load if MSAL is intended to be used
if (config.msal) {
  validateMsalConfig();
}

// Type guard to ensure config.msal is defined
const getMsalConfig = () => {
  if (!config.msal) {
    throw new Error('MSAL configuration is not available');
  }
  return config.msal;
};

const msalCfg = getMsalConfig();

export const msalConfig: Configuration = {
  auth: {
    clientId: msalCfg.clientId,
    authority: msalCfg.authority,
    redirectUri: msalCfg.redirectUri,
    navigateToLoginRequestUrl: true,
  },
  cache: {
    cacheLocation: 'sessionStorage', // Use sessionStorage for better security
    storeAuthStateInCookie: false, // Set to true if you have issues on IE11 or Edge
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) return; // Don't log PII
        
        switch (level) {
          case LogLevel.Error:
            console.error(message);
            break;
          case LogLevel.Warning:
            console.warn(message);
            break;
          case LogLevel.Info:
            if (config.env.isDevelopment) console.info(message);
            break;
          case LogLevel.Verbose:
            if (config.env.isDevelopment) console.debug(message);
            break;
        }
      },
      logLevel: config.env.isDevelopment ? LogLevel.Verbose : LogLevel.Error,
      piiLoggingEnabled: false,
    },
  },
};

/**
 * Scopes for login request
 * These scopes will be requested when the user logs in
 */
export const loginRequest: RedirectRequest = {
  scopes: msalCfg.scopes,
};

/**
 * Scopes for token request (API calls)
 * Use this when acquiring tokens for API calls
 */
export const tokenRequest: PopupRequest = {
  scopes: msalCfg.scopes,
};

/**
 * Check if MSAL is configured
 * Returns true if all required MSAL config is present
 */
export const isMsalConfigured = (): boolean => {
  return !!(config.msal?.clientId && config.msal?.authority && config.msal?.redirectUri);
};