import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/shared/hooks/useAuth";
import { logger } from "@/shared/core/logging/Logger";
import { ROUTES } from "@/constants";

export default function Logout() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    let isMounted = true;
    logger.info('Logout component - initiating logout', { component: 'Logout' });

    const performLogout = async () => {
      try {
        await logout();

        // Only navigate if component is still mounted
        if (isMounted) {
          logger.info('Logout component - logout successful, redirecting to login', { component: 'Logout' });
          navigate(ROUTES.LOGIN, { replace: true });
        }
      } catch (error) {
        logger.error('Logout component - logout failed', {
          component: 'Logout',
          error: error instanceof Error ? error.message : String(error)
        });

        // Even on error, redirect to login if still mounted
        if (isMounted) {
          navigate(ROUTES.LOGIN, { replace: true });
        }
      }
    };

    void performLogout();

    // Cleanup function to prevent state updates after unmount
    return () => {
      isMounted = false;
    };
  }, [logout, navigate]);

  return null; // Don't render anything
}