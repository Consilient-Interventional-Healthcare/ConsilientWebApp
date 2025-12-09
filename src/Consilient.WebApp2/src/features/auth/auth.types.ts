import type { Auth } from "@/types/api.generated";
import type { CurrentUser, MockUser } from "@/types/db.types";
export interface IAuthService {
  linkExternalAccount(params: Auth.LinkExternalLoginRequest): Promise<void>;
  authenticate(providerKey: string): Promise<string>;
  login(params: Auth.AuthenticateUserRequest): Promise<Auth.AuthenticateUserApiResponse>;
  logout(): Promise<void>;
  getCurrentUserClaims(): Promise<Auth.ClaimDto[] | null>;
  initiateMicrosoftLogin(returnUrl?: string): void;
}

export interface AuthContextType {
  user: CurrentUser | null;
  login: (credentials: Auth.AuthenticateUserRequest) => Promise<Auth.AuthenticateUserApiResponse>;
  logout: () => Promise<void>;
  isLoading: boolean;
  isAuthenticated: boolean;
}

export { MockUser, CurrentUser }