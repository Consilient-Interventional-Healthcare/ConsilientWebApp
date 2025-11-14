import type { EnvConfig } from './config.types';

/**
 * Environment configuration class that wraps and validates environment variables
 * Provides type-safe access to environment configuration
 */
class Environment {
  private readonly config: EnvConfig;

  constructor() {
    this.config = this.buildConfig();
    this.logConfigurationInDev();
  }

  // ============================================================================
  // Configuration Building
  // ============================================================================

  /**
   * Build and validate the complete configuration from environment variables
   */
  private buildConfig(): EnvConfig {
    const apiBaseUrl = this.getRequiredEnv('VITE_API_BASE_URL');
    const appEnv = this.validateAppEnv(this.getOptionalEnv('VITE_APP_ENV', 'development'));
    const enableDebugMode = this.getOptionalEnv('VITE_ENABLE_DEBUG_MODE', 'false') === 'true';

    const config: EnvConfig = {
      apiBaseUrl,
      appEnv,
      features: { enableDebugMode },
    };

    // Add MSAL configuration if all required variables are present
    const msalConfig = this.buildMsalConfig();
    if (msalConfig) {
      config.msal = msalConfig;
    }

    return config;
  }

  /**
   * Build MSAL configuration if all required variables are present
   */
  private buildMsalConfig(): EnvConfig['msal'] | undefined {
    const clientId = import.meta.env.VITE_MSAL_CLIENT_ID;
    const tenantId = import.meta.env.VITE_MSAL_TENANT_ID;
    const authority = import.meta.env.VITE_MSAL_AUTHORITY;
    const redirectUri = import.meta.env.VITE_MSAL_REDIRECT_URI;
    const scopes = import.meta.env.VITE_MSAL_SCOPES;

    // Return undefined if any required MSAL variable is missing
    if (!clientId || !tenantId || !authority || !redirectUri) {
      return undefined;
    }

    return {
      clientId,
      tenantId,
      authority,
      redirectUri,
      scopes: scopes ? scopes.split(',').map(s => s.trim()) : ['User.Read'],
    };
  }

  // ============================================================================
  // Environment Variable Access
  // ============================================================================

  /**
   * Get a required environment variable (throws if missing)
   */
  private getRequiredEnv(key: keyof ImportMetaEnv): string {
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
   * Get an optional environment variable with a default value
   */
  private getOptionalEnv(key: keyof ImportMetaEnv, defaultValue: string): string {
    const value = import.meta.env[key] as string | undefined;
    return value ?? defaultValue;
  }

  // ============================================================================
  // Validation
  // ============================================================================

  /**
   * Validate and return the app environment value
   */
  private validateAppEnv(value: string): 'development' | 'staging' | 'production' {
    const validEnvs = ['development', 'staging', 'production'] as const;
    if (!validEnvs.includes(value as typeof validEnvs[number])) {
      throw new Error(
        `Invalid VITE_APP_ENV value: "${value}"\n` +
        `Must be one of: ${validEnvs.join(', ')}`
      );
    }
    return value as 'development' | 'staging' | 'production';
  }

  // ============================================================================
  // Logging
  // ============================================================================

  /**
   * Log configuration in development mode (excluding sensitive data)
   */
  private logConfigurationInDev(): void {
    if (this.isDevelopment) {
      // Use console.log to avoid circular dependency with logger
      console.log('🔧 Environment Configuration:', {
        appEnv: this.config.appEnv,
        apiBaseUrl: this.config.apiBaseUrl,
        features: this.config.features,
        hasMsal: !!this.config.msal,
      });
    }
  }

  // ============================================================================
  // Public Getters
  // ============================================================================

  get apiBaseUrl(): string {
    return this.config.apiBaseUrl;
  }

  get appEnv(): 'development' | 'staging' | 'production' {
    return this.config.appEnv;
  }

  get features() {
    return this.config.features;
  }

  get msal() {
    return this.config.msal;
  }

  get isDevelopment(): boolean {
    return this.config.appEnv === 'development';
  }

  get isProduction(): boolean {
    return this.config.appEnv === 'production';
  }

  get isDebugMode(): boolean {
    return this.config.features.enableDebugMode || this.isDevelopment;
  }
}

/**
 * Application environment configuration - validated at startup
 * This will throw an error if required environment variables are missing
 */
export const env = new Environment();
