import type { UserClaims, IJwtService } from "../auth.types";

const TOKEN_KEY = 'auth_token';

export class JwtService implements IJwtService {
  store(token: string) {
    localStorage.setItem(TOKEN_KEY, token);
  }

  retrieve(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  remove() {
    localStorage.removeItem(TOKEN_KEY);
  }

  decode(): UserClaims | null {
    const token = this.retrieve();
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
      return {
        id: parseInt(payload.sub, 10),
        email: payload.email,
        firstName: payload.given_name ?? "",
        lastName: payload.family_name ?? "",
      };
    } catch {
      return null;
    }
  }
}
export const jwtService = new JwtService();