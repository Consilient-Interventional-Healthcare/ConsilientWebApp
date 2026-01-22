import type { Auth } from "@/types/api.generated";

export interface IAuthService {
  linkExternalAccount(params: Auth.LinkExternalLoginRequest): Promise<void>;
  authenticate(providerKey: string): Promise<string>;
  login(params: Auth.AuthenticateUserRequest): Promise<Auth.AuthenticateUserApiResponse>;
  logout(): Promise<void>;
  getCurrentUser(): Promise<Auth.CurrentUserDto | null>;
  initiateMicrosoftLogin(returnUrl?: string): void;
}

export interface AuthContextType {
  user: Auth.CurrentUserDto | null;
  login: (credentials: Auth.AuthenticateUserRequest) => Promise<Auth.AuthenticateUserApiResponse>;
  logout: () => Promise<void>;
  isLoading: boolean;
  isAuthenticated: boolean;
}