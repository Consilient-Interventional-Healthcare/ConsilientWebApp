// Authentication type definitions

export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  name?: string;
  role?: string;
}

export interface AuthContextType {
  user: User | null;
  login: (userData: User, token: string) => void;
  logout: () => void;
  loading: boolean;
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