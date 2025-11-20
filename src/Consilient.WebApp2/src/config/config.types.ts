/**
 * Type definitions for application configuration
 */

/**
 * Application environment configuration
 */
export interface EnvConfig {
  apiBaseUrl: string;
  appEnv: 'development' | 'staging' | 'production';
  features: {
    enableDebugMode: boolean;
    disableAuth?: boolean;
    useMockServices?: boolean;
  };
  msal?: {
    clientId: string;
    tenantId: string;
    authority: string;
    redirectUri: string;
    scopes: string[];
  };
}
