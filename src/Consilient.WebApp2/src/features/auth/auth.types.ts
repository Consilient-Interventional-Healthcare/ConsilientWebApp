import type { User } from "@/types/db.types";

export interface UserClaims {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
}
export interface IAuthService {
  linkExternalAccount(params: LinkExternalLoginRequest): Promise<void>;
  authenticate(providerKey: string): Promise<string>;
  login(params: AuthenticateUserRequest): Promise<string>;
  logout(): void;
}

export interface IJwtService {
  store(token: string): void;
  retrieve(): string | null;
  remove(): void;
  decode(): UserClaims | null;
}
export interface AuthContextType {
  user: UserClaims | null;
  login: (credentials?: LoginCredentials) => Promise<void>;
  logout: () => Promise<void>;
  isLoading: boolean;
  isAuthenticated: boolean;
}

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
}

export interface LinkExternalLoginResult {
  succeeded: boolean;
  errors?: string[];
}

export interface LinkExternalLoginRequest {
  email: string;
  provider: string;
  providerKey: string;
  providerDisplayName?: string;
}

export interface AuthenticateUserRequest {
  email: string;
  password: string;
}

export interface AuthenticateUserResult {
  succeeded: boolean;
  token?: string;
  errors?: string[];
}

export type { User };