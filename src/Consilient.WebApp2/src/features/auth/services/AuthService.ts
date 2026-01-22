import type { IAuthService } from '@/features/auth/auth.types';
import type { Auth } from '@/types/api.generated';
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

  async linkExternalAccount(params: Auth.LinkExternalLoginRequest): Promise<void> {
    const settings = await this.getSettings();
    if (!settings.externalLoginEnabled) {
      throw new Error("External login is not enabled.");
    }
    const response = await apiClient.post<Auth.LinkExternalLoginResult>('/auth/link-external', params);
    if (!response.data.succeeded) {
      throw new Error(response.data.errors?.join(', ') ?? 'Failed to link external account');
    }
    // Only proceed to authenticate if succeeded
    if (!params.providerKey) {
      throw new Error("providerKey is required to authenticate.");
    }
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

  async login(params: Auth.AuthenticateUserRequest): Promise<Auth.AuthenticateUserApiResponse> {
    const response = await apiClient.post<Auth.AuthenticateUserApiResponse>('/auth/authenticate', params, {
      withCredentials: true
    });
    if (response.status !== 200) {
      return {
        succeeded: false,
        errors: response.data?.errors ?? ['Login failed'],
      };
    }
    // Validate that user exists
    const user = response.data.user;
    if (!user) {
      return {
        succeeded: false,
        errors: ['Invalid response: missing user data'],
      };
    }
    return {
      succeeded: true,
      errors: [],
      user,
    };
  }

async logout(): Promise<void> {
    await apiClient.post('/auth/logout', {}, {
        withCredentials: true
    });
  }

  async getCurrentUser(): Promise<Auth.CurrentUserDto | null> {
    try {
      const response = await apiClient.get<Auth.CurrentUserDto>('/auth/me', {
        withCredentials: true
      });
      if (response.status === 200 && response.data) {
        return response.data;
      }
      return null;
    } catch (error) {
      logger.error("Failed to fetch current user", error as Error, { component: "AuthService" });
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

