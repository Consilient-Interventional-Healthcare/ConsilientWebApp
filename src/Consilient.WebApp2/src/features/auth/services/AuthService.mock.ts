import type { LinkExternalLoginRequest, AuthenticateUserRequest } from '@/features/auth/auth.types';
import { JwtService } from '@/features/auth/services/JwtService';

function createMockJwt(payload: object): string {
  const header = { alg: 'HS256', typ: 'JWT' };
  const encode = (obj: object) => btoa(JSON.stringify(obj));
  // No signature for mock
  return `${encode(header)}.${encode(payload)}.`;
}

export class AuthServiceMock {
    
  async linkExternalAccount(params: LinkExternalLoginRequest): Promise<void> {
    if (params.providerKey === 'fail') {
      return Promise.reject(new Error('Failed to link external account'));
    }
    return Promise.resolve();
  }

  async authenticate(providerKey: string): Promise<string> {
    if (providerKey === 'fail') {
      return Promise.reject(new Error('Authentication failed'));
    }
    const payload = {
      sub: providerKey,
      email: 'mockuser@example.com',
      given_name: 'Mock',
      family_name: 'User',
      name: 'Mock User'
    };
    const token = createMockJwt(payload);
    JwtService.store(token);
    return Promise.resolve(token);
  }

  async login(params: AuthenticateUserRequest): Promise<string> {
    if (params.email === 'fail@example.com') {
      return Promise.reject(new Error('Login failed'));
    }
    const payload = {
      sub: 'mock-id',
      email: params.email,
      given_name: 'Mock',
      family_name: 'User',
      name: 'Mock User'
    };
    const token = createMockJwt(payload);
    JwtService.store(token);
    return Promise.resolve(token);
  }

  logout(): void {
    JwtService.remove();
  }
}