import { useAuth } from "@/shared/hooks/useAuth";
import { ROUTES } from "@/constants";
import type { ReactNode } from 'react';

interface ProtectedRouteProps {
  children: ReactNode;
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    // Optionally show a spinner here
    return null;
  }

  if (!isAuthenticated) {
    window.location.replace(ROUTES.LOGIN);
    return null;
  }

  return children;
};

export default ProtectedRoute;
