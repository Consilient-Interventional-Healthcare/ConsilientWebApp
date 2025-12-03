import type { LoginResults, LinkExternalLoginRequest, LinkExternalLoginResult, AuthenticateUserRequest, AuthenticateUserResult, IAuthService, UserClaim } from '@/features/auth/auth.types';
import apiClient from '@/shared/core/api/ApiClient';
import { logger } from "@/shared/core/logging/Logger";


export class AuthService implements IAuthService {

  async linkExternalAccount(params: LinkExternalLoginRequest): Promise<void> {
    const response = await apiClient.post<LinkExternalLoginResult>('/auth/link-external', params);
    if (!response.data.succeeded) {
      throw new Error(response.data.errors?.join(', ') ?? 'Failed to link external account');
    }
    // Only proceed to authenticate if succeeded
    await this.authenticate(params.providerKey);
  }

  authenticate(_providerKey: string): Promise<string> {
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
}

