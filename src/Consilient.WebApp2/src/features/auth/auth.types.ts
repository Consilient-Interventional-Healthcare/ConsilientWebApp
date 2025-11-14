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
  login: () => Promise<void>;
  logout: () => Promise<void>;
  isLoading: boolean;
  isAuthenticated: boolean;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface AuthResponse {
  user: User;
  token: string;
}