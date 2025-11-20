import type { Configuration, PopupRequest, RedirectRequest } from '@azure/msal-browser';
import { LogLevel } from '@azure/msal-browser';
import appSettings from '@/config';

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
  if (!appSettings.msal) {
    throw new Error(
      'MSAL configuration is missing.\n' +
      'Please set the following environment variables:\n' +
      '  - MSAL_CLIENT_ID\n' +
      '  - MSAL_TENANT_ID\n' +
      '  - MSAL_AUTHORITY\n' +
      '  - MSAL_REDIRECT_URI\n' +
      'See docs/MS_ENTRA_AUTHENTICATION.md for setup instructions.'
    );
  }
  
  if (!appSettings.msal.clientId) {
    throw new Error('MSAL_CLIENT_ID is required but not set');
  }
  
  if (!appSettings.msal.authority) {
    throw new Error('MSAL_AUTHORITY is required but not set');
  }
  
  if (!appSettings.msal.redirectUri) {
    throw new Error('MSAL_REDIRECT_URI is required but not set');
  }
};

// Validate config on module load if MSAL is intended to be used
if (appSettings.msal) {
  validateMsalConfig();
}

// Type guard to ensure appSettings.msal is defined
const getMsalConfig = () => {
  // if (!appSettings.msal) {
  //   throw new Error('MSAL configuration is not available');
  // }
  return appSettings.msal;
};

let msalConfig: Configuration | undefined;
let loginRequest: RedirectRequest;
let tokenRequest: PopupRequest;
let isMsalConfigured: () => boolean;

if (appSettings.features.disableAuth) {
  msalConfig = undefined;
  loginRequest = { scopes: [] };
  tokenRequest = { scopes: [] };
  isMsalConfigured = () => false;
} else {
  interface MsalCfgType {
    clientId?: string;
    authority?: string;
    redirectUri?: string;
    scopes?: string[];
  };
  const msalCfg: MsalCfgType = getMsalConfig() ?? {};
  msalConfig = {
    auth: {
      clientId: msalCfg.clientId ?? '',
      authority: msalCfg.authority ?? '',
      redirectUri: msalCfg.redirectUri ?? '',
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
              if (appSettings.app.isDevelopment) console.info(message);
              break;
            case LogLevel.Verbose:
              if (appSettings.app.isDevelopment) console.debug(message);
              break;
          }
        },
        logLevel: appSettings.app.isDevelopment ? LogLevel.Verbose : LogLevel.Error,
        piiLoggingEnabled: false,
      },
    },
  };
  const scopes = Array.isArray(msalCfg.scopes) ? msalCfg.scopes : [];
  loginRequest = { scopes };
  tokenRequest = { scopes };
  isMsalConfigured = () => !!(appSettings.msal?.clientId && appSettings.msal?.authority && appSettings.msal?.redirectUri);
}

export { msalConfig, loginRequest, tokenRequest, isMsalConfigured };