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

let msalConfig: Configuration | undefined;
let loginRequest: RedirectRequest;
let tokenRequest: PopupRequest;
let isMsalConfigured: () => boolean;

if (config.features.disableAuth) {
  msalConfig = undefined;
  loginRequest = { scopes: [] };
  tokenRequest = { scopes: [] };
  isMsalConfigured = () => false;
} else {
  const msalCfg = getMsalConfig();
  msalConfig = {
    auth: {
      clientId: msalCfg.clientId,
      authority: msalCfg.authority,
      redirectUri: msalCfg.redirectUri,
      navigateToLoginRequestUrl: true,
    },
    cache: {
      cacheLocation: 'sessionStorage',
      storeAuthStateInCookie: false,
    },
    system: {
      loggerOptions: {
        loggerCallback: (level, message, containsPii) => {
          if (containsPii) return;
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
  loginRequest = { scopes: msalCfg.scopes };
  tokenRequest = { scopes: msalCfg.scopes };
  isMsalConfigured = () => !!(config.msal?.clientId && config.msal?.authority && config.msal?.redirectUri);
}

export { msalConfig, loginRequest, tokenRequest, isMsalConfigured };