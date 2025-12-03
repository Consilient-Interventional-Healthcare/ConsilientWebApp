import { useState, useEffect, type ReactNode } from 'react';
import { AuthContext } from '@/features/auth/contexts/AuthContext';
import { logger } from '@/shared/core/logging/Logger';
import appSettings from '@/config';
import { getAuthService } from '@/features/auth/services/AuthServiceFactory';

import type { CurrentUser } from '@/features/auth/currentUser.types';
import type { UserClaim } from '../auth.types';

interface AuthProviderProps {
  children: ReactNode;
}

const authService = getAuthService();

export const AuthProvider = ({ children }: AuthProviderProps) => {

  // Helper to map claims array to CurrentUser
  const mapClaimsToCurrentUser = (claims: UserClaim[]): CurrentUser => {
    const getClaimValue = (type: string) => {
      const found = Array.isArray(claims) ? claims.find((c) => c.type === type) : undefined;
      return found?.value ?? '';
    };
    return {
      id: getClaimValue('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'),
      userName: getClaimValue('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'),
      email: getClaimValue('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'),
    };
  };

  const [isLoading, setLoading] = useState(true);
  const [user, setUser] = useState<CurrentUser | null>(null);

  useEffect(() => {
    const initAuth = async () => {
      const claims = await authService.getCurrentUserClaims();
      if (claims) {
        const mappedUser = mapClaimsToCurrentUser(claims);
        setUser(mappedUser);
      } else {
        setUser(null);
      }
      setLoading(false);
    };
    void initAuth();
  }, []);

  const login = async (params: { username: string; password: string }): Promise<void> => {
    const result = await authService.login({ username: params.username, password: params.password });
    if (result.success && result.userClaims) {
      const mappedUser = mapClaimsToCurrentUser(result.userClaims);
      setUser(mappedUser);
      logger.debug('AuthProvider - setUser called after regular login with claims', { component: 'AuthProvider', user: mappedUser.email, isAuthenticated: !!result.userClaims });
    } else {
      // Throw with a message
      throw new Error(result.errors.length ? result.errors.join(', ') : 'Login failed');
    }
  };

  const logout = async (): Promise<void> => {
    await authService.logout();
    setUser(null); // Clear user on logout
  };

  useEffect(() => {
    logger.debug('AuthProvider user state updated', { component: 'AuthProvider', user: user?.email, isAuthenticated: !!user });
  }, [user]);

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
