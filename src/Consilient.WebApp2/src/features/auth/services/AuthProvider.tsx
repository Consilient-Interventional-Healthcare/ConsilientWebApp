import { useState, useEffect, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { AuthContext } from "@/features/auth/contexts/AuthContext";
import { logger } from "@/shared/core/logging/Logger";
import { AuthService } from "@/features/auth/services/AuthService";
import { authStateManager } from "@/features/auth/services/AuthStateManager";
import { ROUTES } from "@/constants";
import { ApiError } from "@/shared/core/api/api.types";
import type { SessionExpiredDetail } from "../auth.events";
import type { Auth } from "@/types/api.generated";

interface AuthProviderProps {
  children: ReactNode;
}

const authService = new AuthService();

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const navigate = useNavigate();

  const [user, setUser] = useState<Auth.CurrentUserDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const initAuth = async () => {
      try {
        const currentUser = await authService.getCurrentUser();
        setUser(currentUser);
      } catch (error) {
        logger.error("AuthProvider - Failed to fetch current user on init", error as Error, { component: "AuthProvider" });
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
    setIsLoading(true);
    try {
      const result = await authService.login(credentials);
      if (result.succeeded && result.user) {
        setUser(result.user);
        logger.debug(
          "AuthProvider - setUser called after login",
          {
            component: "AuthProvider",
            user: result.user.email,
            isAuthenticated: true,
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

      // Extract detailed error messages from ApiError.details (contains response.data)
      let errors: string[];
      if (error instanceof ApiError && error.details) {
        const details = error.details as { errors?: string[] };
        errors = details.errors ?? [error.message];
      } else {
        errors = [error instanceof Error ? error.message : String(error)];
      }

      return {
        succeeded: false,
        errors,
      };
    } finally {
      setIsLoading(false);
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
      // Don't include redirect if destination is login page itself (avoids loops)
      const redirectPath = event.detail.redirectPath;
      if (redirectPath && redirectPath !== ROUTES.LOGIN && !redirectPath.startsWith(ROUTES.LOGIN)) {
        void navigate(`${ROUTES.LOGIN}?redirect=${encodeURIComponent(redirectPath)}`);
      } else {
        void navigate(ROUTES.LOGIN);
      }
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
