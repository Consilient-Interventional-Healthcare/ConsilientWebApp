// Authentication type definitions

export type UserId = string & { readonly __brand: 'UserId' };

export interface User {
  id: UserId; // Changed from number to string to support MSAL account IDs
  email: string;
  firstName: string;
  lastName: string;
  name?: string;
  role?: string;
}

export interface AuthContextType {
  user: User | null;
  login: (credentials?: LoginCredentials) => Promise<void>; // Accepts credentials for username/password login
  logout: () => Promise<void>;
  isLoading: boolean;
  isAuthenticated: boolean;
}

export interface LoginCredentials {
  username: string; // Changed from email to username for consistency with AuthService
  password: string;
}

export interface AuthResponse{
  token: string;
};

export interface LinkExternalLoginResult {
  succeeded: boolean;
  errors?: string[];
};

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