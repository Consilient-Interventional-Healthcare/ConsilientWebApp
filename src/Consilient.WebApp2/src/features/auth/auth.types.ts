import type { User } from "@/types/db.types";
import type { CurrentUser } from "./currentUser.types";

export interface UserClaim {
  type: string;
  value: string;
}
export interface LoginResults {
  success: boolean;
  errors: string[];
  userClaims?: UserClaim[] | undefined;
}
export interface IAuthService {
  linkExternalAccount(params: LinkExternalLoginRequest): Promise<void>;
  authenticate(providerKey: string): Promise<string>;
  login(params: AuthenticateUserRequest): Promise<LoginResults>;
  logout(): Promise<void>;
  getCurrentUserClaims(): Promise<UserClaim[] | null>;
  initiateMicrosoftLogin(returnUrl?: string): void;
}

export interface AuthContextType {
  user: CurrentUser | null;
  login: (credentials: LoginCredentials) => Promise<void>;
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
  username: string;
  password: string;
}

export interface AuthenticateUserResult {
  succeeded: boolean;
  errors?: string[];
  userClaims?: UserClaim[];
}

export type { User };