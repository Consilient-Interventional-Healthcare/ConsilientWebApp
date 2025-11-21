import { useState, useEffect, type ReactNode } from 'react';
import { AuthContext } from '@/features/auth/contexts/AuthContext';
import { msalService } from '@/features/auth/services/MsalService';
import { logger } from '@/shared/core/logging/Logger';
import appSettings from '@/config';
import { Auth } from '@/features/auth/services/index'; // Use the factory
import { JwtService } from '@/features/auth/services/JwtService';

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [isLoading, setLoading] = useState(true);

  useEffect(() => {
    const initAuth = async () => {
      if (!msalService.isConfigured()) {
        logger.warn('MSAL is not configured, skipping authentication initialization', {
          component: 'AuthProvider'
        });
        setLoading(false);
        return;
      }

      await msalService.initialize();
      const account = msalService.getAccount();

      if (account) {
        await Auth.linkExternalAccount({
          email: account.username,
          provider: 'msal',
          providerKey: account.homeAccountId,
          providerDisplayName: account.name ?? '',
        });
      } else {
        logger.info('No authenticated user found', { component: 'AuthProvider' });
      }
      setLoading(false);
    };

    void initAuth();
  }, []);

  const login = async (params?: { username: string; password: string }): Promise<void> => {
    if (params) {
      await Auth.login({ email: params.username, password: params.password });
    } else {
      await msalService.login();
    }
  };

  const logout = async (): Promise<void> => {
    await msalService.logout();
    Auth.logout();
  };

  const user = JwtService.decode();

  return (
    <AuthContext.Provider value={{
      user,
      login,
      logout,
      isLoading,
      isAuthenticated: appSettings.features.disableAuth ? true : !!user
    }}>
      {children}
    </AuthContext.Provider>
  );
};
