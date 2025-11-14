import {
  PublicClientApplication,
  AccountInfo,
  InteractionRequiredAuthError,
  BrowserAuthError,
  EndSessionRequest
} from '@azure/msal-browser';
import { msalConfig, loginRequest, tokenRequest, isMsalConfigured } from './msalConfig';
import { logger } from '@/shared/core/logging/logger';

/**
 * MSAL Service - Wrapper around PublicClientApplication
 * Provides simplified methods for authentication operations
 */
class MsalService {
  private msalInstance: PublicClientApplication | null = null;
  private initializePromise: Promise<void> | null = null;
  private instancePromise: Promise<PublicClientApplication> | null = null;

  /**
   * Get the MSAL instance (lazy initialization)
   */
  private async getMsalInstance(): Promise<PublicClientApplication> {
    if (!isMsalConfigured()) {
      throw new Error('MSAL is not configured. Please check your environment variables.');
    }

    // Return existing instance if already created
    if (this.msalInstance) {
      return this.msalInstance;
    }

    // Return in-flight promise if instance is being created
    if (this.instancePromise) {
      return this.instancePromise;
    }

    // Create new instance with promise-based lock to prevent race conditions
    this.instancePromise = (async () => {
      this.msalInstance = new PublicClientApplication(msalConfig);
      await this.initialize();
      return this.msalInstance;
    })();

    return this.instancePromise;
  }

  /**
   * Initialize MSAL
   * Must be called before any other MSAL operations
   */
  async initialize(): Promise<void> {
    if (this.initializePromise) {
      return this.initializePromise;
    }

    this.initializePromise = (async () => {
      try {
        if (!this.msalInstance) {
          throw new Error('MSAL instance not created');
        }

        await this.msalInstance.initialize();
        
        // Handle the redirect promise on page load
        const response = await this.msalInstance.handleRedirectPromise();
        
        if (response) {
          logger.info('Authentication redirect handled successfully', {
            component: 'MsalService',
            accountId: response.account?.homeAccountId,
          });
        }
      } catch (error) {
        logger.error('MSAL initialization failed', error as Error, { component: 'MsalService' });
        throw error;
      }
    })();

    return this.initializePromise;
  }

  /**
   * Initiate login using redirect
   */
  async login(): Promise<void> {
    try {
      const instance = await this.getMsalInstance();
      logger.info('Initiating login redirect', { component: 'MsalService' });
      await instance.loginRedirect(loginRequest);
    } catch (error) {
      logger.error('Login failed', error as Error, { component: 'MsalService' });
      throw error;
    }
  }

  /**
   * Get access token for API calls
   * Attempts silent token acquisition first, falls back to interactive if needed
   */
  async getAccessToken(): Promise<string | null> {
    try {
      const instance = await this.getMsalInstance();
      const account = this.getAccount();
      
      if (!account) {
        logger.warn('No account found, cannot acquire token', { component: 'MsalService' });
        return null;
      }

      try {
        // Try silent token acquisition
        const response = await instance.acquireTokenSilent({
          ...tokenRequest,
          account,
        });
        
        return response.accessToken;
      } catch (error) {
        // Handle specific MSAL error types for better debugging and user experience
        if (error instanceof InteractionRequiredAuthError) {
          logger.info('Silent token acquisition failed - user interaction required', {
            component: 'MsalService',
            errorCode: error.errorCode,
            errorMessage: error.errorMessage,
          });
          
          // Redirect to login for interactive authentication
          await instance.loginRedirect(loginRequest);
          return null;
        }
        
        if (error instanceof BrowserAuthError) {
          logger.error('Browser authentication error occurred', error, {
            component: 'MsalService',
            errorCode: error.errorCode,
            errorMessage: error.errorMessage,
          });
          return null;
        }
        
        // Re-throw unexpected errors
        throw error;
      }
    } catch (error) {
      logger.error('Token acquisition failed unexpectedly', error as Error, { 
        component: 'MsalService',
        action: 'getAccessToken',
      });
      return null;
    }
  }

  /**
   * Get the currently signed-in account
   */
  getAccount(): AccountInfo | null {
    if (!this.msalInstance) {
      return null;
    }

    const accounts = this.msalInstance.getAllAccounts();
    
    if (accounts.length === 0) {
      return null;
    }

    // Return the first account (most recent)
    return accounts[0] ?? null;
  }

  /**
   * Get all accounts
   */
  getAllAccounts(): AccountInfo[] {
    if (!this.msalInstance) {
      return [];
    }

    return this.msalInstance.getAllAccounts();
  }

  /**
   * Logout the user
   */
  async logout(): Promise<void> {
    try {
      const instance = await this.getMsalInstance();
      const account = this.getAccount();
      
      const logoutRequest: EndSessionRequest = account
        ? { account }
        : {};

      logger.info('Logging out user', { 
        component: 'MsalService',
        accountId: account?.homeAccountId,
      });

      await instance.logoutRedirect(logoutRequest);
    } catch (error) {
      logger.error('Logout failed', error as Error, { component: 'MsalService' });
      throw error;
    }
  }

  /**
   * Check if MSAL is configured
   */
  isConfigured(): boolean {
    return isMsalConfigured();
  }

  /**
   * Get the MSAL instance (for advanced usage)
   * Note: You should use the service methods instead of accessing the instance directly
   */
  getInstance(): PublicClientApplication | null {
    return this.msalInstance;
  }
}

// Export singleton instance
export const msalService = new MsalService();