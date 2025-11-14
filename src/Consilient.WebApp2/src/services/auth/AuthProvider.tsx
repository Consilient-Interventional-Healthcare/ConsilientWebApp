import { useState, useEffect, type ReactNode } from 'react';
import { AuthContext } from '@/services/auth/AuthContext';
import { STORAGE_KEYS, storage } from '@/constants';
import { logger } from '@/services/logging/logger';
import type { User } from '@/types/auth';

interface AuthProviderProps {
  children: ReactNode;
}

/**
 * Validates JWT token expiration
 * @param token - JWT token string
 * @returns true if token is valid and not expired
 */
const isTokenValid = (token: string): boolean => {
  try {
    // JWT tokens have three parts separated by dots
    const parts = token.split('.');
    if (parts.length !== 3) {
      return false;
    }

    // Decode the payload (second part)
    const payload = JSON.parse(atob(parts[1]!));
    
    // Check if token has expiration claim
    if (!payload.exp) {
      logger.warn('Token does not have expiration claim', { component: 'AuthProvider' });
      return true; // Allow tokens without exp claim for now
    }

    // Check if token is expired (exp is in seconds, Date.now() is in milliseconds)
    const isValid = payload.exp * 1000 > Date.now();
    
    if (!isValid) {
      logger.info('Token has expired', { component: 'AuthProvider' });
    }
    
    return isValid;
  } catch (error) {
    logger.error('Failed to validate token', error as Error, { component: 'AuthProvider' });
    return false;
  }
};

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Check if user is logged in (using sessionStorage for better security)
    const checkAuth = () => {
      const token = storage.getString(STORAGE_KEYS.AUTH_TOKEN);
      const userData = storage.get<User>(STORAGE_KEYS.USER_DATA);
      
      if (token && userData) {
        // Validate token before setting user
        if (!isTokenValid(token)) {
          logger.info('Token is invalid or expired, clearing session', { component: 'AuthProvider' });
          storage.remove(STORAGE_KEYS.AUTH_TOKEN);
          storage.remove(STORAGE_KEYS.USER_DATA);
          setLoading(false);
          return;
        }

        setUser(userData);
      }
      setLoading(false);
    };

    checkAuth();

    // Set up periodic token validation (every 60 seconds)
    const validationInterval = setInterval(() => {
      const token = storage.getString(STORAGE_KEYS.AUTH_TOKEN);
      if (token && !isTokenValid(token)) {
        logger.info('Token expired during session, logging out', { component: 'AuthProvider' });
        storage.remove(STORAGE_KEYS.AUTH_TOKEN);
        storage.remove(STORAGE_KEYS.USER_DATA);
        setUser(null);
      }
    }, 60000); // Check every 60 seconds

    return () => clearInterval(validationInterval);
  }, []);

  const login = (userData: User, token: string) => {
    // Validate token before storing
    if (!isTokenValid(token)) {
      logger.error('Cannot login with invalid or expired token', undefined, { component: 'AuthProvider', userId: userData.id });
      throw new Error('Invalid or expired token');
    }

    storage.setString(STORAGE_KEYS.AUTH_TOKEN, token);
    storage.set(STORAGE_KEYS.USER_DATA, userData);
    setUser(userData);
  };

  const logout = () => {
    storage.remove(STORAGE_KEYS.AUTH_TOKEN);
    storage.remove(STORAGE_KEYS.USER_DATA);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, loading, isAuthenticated: !!user }}>
      {children}
    </AuthContext.Provider>
  );
};
