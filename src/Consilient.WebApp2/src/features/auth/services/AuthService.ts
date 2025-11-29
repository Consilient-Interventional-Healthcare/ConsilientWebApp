import type { LinkExternalLoginRequest, LinkExternalLoginResult, AuthenticateUserRequest, AuthenticateUserResult, IAuthService } from '@/features/auth/auth.types';
import api from '@/shared/core/api/ApiClient';
import { jwtService } from '@/features/auth/services/JwtService';


export class AuthService implements IAuthService {

  async linkExternalAccount(params: LinkExternalLoginRequest): Promise<void> {
    const response: LinkExternalLoginResult = await api.post('/auth/link-external', params);
    if (!response.succeeded) {
      throw new Error(response.errors?.join(', ') ?? 'Failed to link external account');
    }
    // Only proceed to authenticate if succeeded
    await this.authenticate(params.providerKey);
  }

  async authenticate(providerKey: string): Promise<string> {
    const request: { providerKey: string } = { providerKey };
    const response: AuthenticateUserResult = await api.post('/auth/authenticate', request);
    if (!response.token) {
      throw new Error(response.errors?.join(', ') ?? 'Authentication failed');
    }
    jwtService.store(response.token);
    return response.token;
  }

  async login(params: AuthenticateUserRequest): Promise<string> {
    const response: AuthenticateUserResult = await api.post('/auth/login', params);
    if (!response.token) {
      throw new Error(response.errors?.join(', ') ?? 'Login failed');
    }
    jwtService.store(response.token);
    return response.token;
  }

  logout(): void {
    jwtService.remove();
  }
}

