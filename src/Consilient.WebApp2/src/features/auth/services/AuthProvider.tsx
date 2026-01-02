import { useState, useEffect, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { AuthContext } from "@/features/auth/contexts/AuthContext";
import { logger } from "@/shared/core/logging/Logger";
import { getAuthService } from "@/features/auth/services/AuthServiceFactory";
import { authStateManager } from "@/features/auth/services/AuthStateManager";
import { ROUTES, CLAIM_TYPES } from "@/constants";
import type { SessionExpiredDetail } from "../auth.events";
import type { Auth } from "@/types/api.generated";
import type { CurrentUser } from "./../auth.types"; ;

interface AuthProviderProps {
  children: ReactNode;
}

const authService = getAuthService();

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const navigate = useNavigate();

  // Helper to map claims array to CurrentUser
  const mapClaimsToCurrentUser = (claims: Auth.ClaimDto[]): CurrentUser => {
    const getClaimValue = (type: string) => {
      const found = Array.isArray(claims)
        ? claims.find((c) => c.type === type)
        : undefined;
      return found?.value ?? "";
    };
    return {
      id: getClaimValue(CLAIM_TYPES.NAME_IDENTIFIER),
      userName: getClaimValue(CLAIM_TYPES.NAME),
      email: getClaimValue(CLAIM_TYPES.EMAIL),
    };
  };

  const [user, setUser] = useState<CurrentUser | null>(null);
  const [isLoading, setIsLoading] = useState(true); // New loading state

  useEffect(() => {
    const initAuth = async () => {
      try {
        const claims = await authService.getCurrentUserClaims();
        if (claims) {
          const mappedUser = mapClaimsToCurrentUser(claims);
          setUser(mappedUser);
        } else {
          setUser(null);
        }
      } catch (error) {
        logger.error("AuthProvider - Failed to fetch user claims on init", error as Error, { component: "AuthProvider" });
        setUser(null);
      } finally {
        setIsLoading(false);
        // Notify that auth initialization is complete
        authStateManager.setAuthInitialized();
        logger.debug("AuthProvider - Auth initialization complete", { component: "AuthProvider" });
      }
    };
    void initAuth();
  }, []); // Run once on mount

  const login = async (
    credentials: Auth.AuthenticateUserRequest
  ): Promise<Auth.AuthenticateUserApiResponse> => {
    setIsLoading(true); // Set loading true on login attempt
    try {
      const result = await authService.login(credentials);
      if (result.succeeded && result.userClaims) {
        const mappedUser = mapClaimsToCurrentUser(result.userClaims);
        setUser(mappedUser);
        logger.debug(
          "AuthProvider - setUser called after regular login with claims",
          {
            component: "AuthProvider",
            user: mappedUser.email,
            isAuthenticated: !!result.userClaims,
          }
        );
        return result;
      } else {
        throw new Error(
          result.errors?.length ? result.errors.join(", ") : "Login failed"
        );
      }
    } catch (error) {
      logger.error("AuthProvider - Login failed", error as Error, { component: "AuthProvider" });
      return {
        succeeded: false,
        errors: [error instanceof Error ? error.message : String(error)],
        userClaims: null,
      };
    } finally {
      setIsLoading(false); // Ensure loading is set to false after login attempt
    }
  };

  const logout = async () => {
    try {
      await authService.logout();
      logger.info('Logout successful on the server.', { component: 'AuthProvider' });
    } catch (error) {
      logger.error('Server logout failed', error as Error, { component: 'AuthProvider' });
      // Even if server logout fails, proceed to clear client-side session
    } finally {
      setUser(null);
      logger.info('Client-side user state cleared.', { component: 'AuthProvider' });
    }
  };

  useEffect(() => {
    logger.debug("AuthProvider user state updated", {
      component: "AuthProvider",
      user: user?.email,
      isAuthenticated: !!user,
      isLoading,
    });
  }, [user, isLoading]);

  // Listen for session expired events from API interceptors
  useEffect(() => {
    const handleSessionExpired = (event: CustomEvent<SessionExpiredDetail>) => {
      logger.warn('Session expired event received, clearing user state and navigating to login', {
        component: 'AuthProvider',
        redirectPath: event.detail.redirectPath,
      });

      // Clear user state immediately
      setUser(null);

      // Navigate to login page with redirect parameter
      void navigate(`${ROUTES.LOGIN}?redirect=${encodeURIComponent(event.detail.redirectPath)}`);
    };

    window.addEventListener('auth:sessionExpired', handleSessionExpired);

    return () => {
      window.removeEventListener('auth:sessionExpired', handleSessionExpired);
    };
  }, [navigate]);

  return (
    <AuthContext.Provider
      value={{
        user,
        login,
        logout,
        isLoading, // Use the new isLoading state
        isAuthenticated: !!user,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
