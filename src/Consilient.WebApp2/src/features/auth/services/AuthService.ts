import type { LoginResults, LinkExternalLoginRequest, LinkExternalLoginResult, AuthenticateUserRequest, AuthenticateUserResult, IAuthService, UserClaim } from '@/features/auth/auth.types';
import type { AppSettings } from '@/shared/core/appSettings/appSettings.types';
import apiClient from '@/shared/core/api/ApiClient';
import { logger } from "@/shared/core/logging/Logger";
import { AppSettingsServiceFactory } from '@/shared/core/appSettings/AppSettingsServiceFactory';

export class AuthService implements IAuthService {
  private settingsPromise: Promise<AppSettings> | null = null;

  private async getSettings(): Promise<AppSettings> {
    this.settingsPromise ??= AppSettingsServiceFactory.create().getAppSettings();
    return this.settingsPromise;
  }

  async linkExternalAccount(params: LinkExternalLoginRequest): Promise<void> {
    const settings = await this.getSettings();
    if (!settings.externalLoginEnabled) {
      throw new Error("External login is not enabled.");
    }
    const response = await apiClient.post<LinkExternalLoginResult>('/auth/link-external', params);
    if (!response.data.succeeded) {
      throw new Error(response.data.errors?.join(', ') ?? 'Failed to link external account');
    }
    // Only proceed to authenticate if succeeded
    await this.authenticate(params.providerKey);
  }

  async authenticate(_providerKey: string): Promise<string> {
    const settings = await this.getSettings();
    if (!settings.externalLoginEnabled) {
      throw new Error("External login is not enabled.");
    }
    throw new Error("Not implemented");
    // const request: { providerKey: string } = { providerKey };
    // const response = await apiClient.post<AuthenticateUserResult>('/auth/authenticate', request);
    // if (!response.data) {
    //   throw new Error(response.data.errors?.join(', ') ?? 'Authentication failed');
    // }
    // return response.token;
  }

  async login(params: AuthenticateUserRequest): Promise<LoginResults> {
    const response = await apiClient.post<AuthenticateUserResult>('/auth/authenticate', params, {
      withCredentials: true
    });
    if (response.status !== 200) {
      return {
        success: false,
        errors: response.data?.errors ?? ['Login failed']
      };
    }
    return {
      success: true,
      errors: [],
      userClaims: response.data.userClaims,
    };
  }

async logout(): Promise<void> {
    await apiClient.post('/auth/logout', {}, {
        withCredentials: true
    });
  }

  async getCurrentUserClaims(): Promise<UserClaim[] | null> {
    try {
      const response = await apiClient.get<UserClaim[]>('/auth/claims', {
        withCredentials: true
      });
      if (response.status === 200 && response.data) {
        return response.data;
      }
      return null;
    } catch (error) {
      logger.error("Failed to fetch user claims", error as Error, { component: "AuthService" });
      return null;
    }
  }

  initiateMicrosoftLogin(returnUrl?: string): void {
    const settings = AppSettingsServiceFactory.create().getAppSettings();
    settings.then((config) => {
      if (!config.externalLoginEnabled) {
        logger.error("External login is not enabled", undefined, { component: "AuthService" });
        throw new Error("External login is not enabled.");
      }

      // Build the backend URL that will redirect to Microsoft
      const baseUrl = apiClient.getBaseUrl();
      const apiUrl = new URL('/auth/microsoft/login', baseUrl);

      // Construct full frontend URL with the returnUrl path
      if (returnUrl) {
        const frontendBaseUrl = `${window.location.protocol}//${window.location.host}`;
        const fullReturnUrl = `${frontendBaseUrl}${returnUrl.startsWith('/') ? returnUrl : '/' + returnUrl}`;
        apiUrl.searchParams.set('returnUrl', fullReturnUrl);
      }

      logger.debug("Initiating Microsoft login redirect", {
        component: "AuthService",
        redirectUrl: apiUrl.toString()
      });

      // Redirect the entire page to the backend endpoint
      window.location.href = apiUrl.toString();
    }).catch((error) => {
      logger.error("Failed to initiate Microsoft login", error as Error, { component: "AuthService" });
      throw error;
    });
  }
}

