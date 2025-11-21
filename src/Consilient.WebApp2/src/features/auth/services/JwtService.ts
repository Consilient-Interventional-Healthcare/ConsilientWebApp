import type { User } from '@/features/auth/auth.types';

const TOKEN_KEY = 'auth_token';

export class JwtService {
  static store(token: string) {
    localStorage.setItem(TOKEN_KEY, token);
  }

  static retrieve(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  static remove() {
    localStorage.removeItem(TOKEN_KEY);
  }

  static decode(): User | null {
    const token = JwtService.retrieve();
    if (!token) return null;
    try {
      const payloadPart = token.split('.')[1];
      if (!payloadPart) return null;
      const decoded = atob(payloadPart);
      const payload = JSON.parse(decoded) as {
        sub: string;
        email: string;
        given_name?: string;
        family_name?: string;
        name?: string;
      };
      // Map JWT payload to User type as needed
      return {
        id: payload.sub as User['id'],
        email: payload.email,
        firstName: payload.given_name ?? '',
        lastName: payload.family_name ?? '',
        name: payload.name ?? payload.email,
      };
    } catch {
      return null;
    }
  }
}