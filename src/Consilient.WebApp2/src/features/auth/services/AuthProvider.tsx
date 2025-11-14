import { useState, useEffect, type ReactNode } from 'react';
import type { AccountInfo } from '@azure/msal-browser';
import { AuthContext } from '@/features/auth/services/AuthContext';
import { msalService } from '@/features/auth/services/MsalService';
import { logger } from '@/shared/core/logging/logger';
import type { User } from '@/features/auth/types/auth';

interface AuthProviderProps {
  children: ReactNode;
}

/**
 * Convert MSAL AccountInfo to User object
 */
const accountToUser = (account: AccountInfo): User => {
  const nameParts = account.name?.split(' ') ?? [];
  return {
    id: account.homeAccountId, // Use MSAL's unique account ID
    email: account.username,
    firstName: nameParts[0] ?? '',
    lastName: nameParts.slice(1).join(' ') ?? '',
    name: account.name ?? account.username,
    // role is optional, so we can omit it
  };
};

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setLoading] = useState(true);

  useEffect(() => {
    // Initialize MSAL and check for authenticated user
    const initAuth = async () => {
      try {
        // Only initialize if MSAL is configured
        if (!msalService.isConfigured()) {
          logger.warn('MSAL is not configured, skipping authentication initialization', {
            component: 'AuthProvider'
          });
          setLoading(false);
          return;
        }

        // Initialize MSAL - this handles redirect promise
        await msalService.initialize();
        
        // Check if user is authenticated (after redirect or from existing session)
        const account = msalService.getAccount();
        
        if (account) {
          logger.info('User authenticated via MSAL', {
            component: 'AuthProvider',
            accountId: account.homeAccountId,
          });
          setUser(accountToUser(account));
        } else {
          logger.info('No authenticated user found', { component: 'AuthProvider' });
        }
      } catch (error) {
        logger.error('Failed to initialize authentication', error as Error, {
          component: 'AuthProvider'
        });
      } finally {
        setLoading(false);
      }
    };

    void initAuth();
  }, []);

  const login = async (): Promise<void> => {
    try {
      await msalService.login();
    } catch (error) {
      logger.error('Login failed', error as Error, { component: 'AuthProvider' });
      throw error;
    }
  };

  const logout = async (): Promise<void> => {
    try {
      await msalService.logout();
      setUser(null);
    } catch (error) {
      logger.error('Logout failed', error as Error, { component: 'AuthProvider' });
      // Still clear user state on logout error
      setUser(null);
      throw error;
    }
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, isLoading, isAuthenticated: !!user }}>
      {children}
    </AuthContext.Provider>
  );
};
