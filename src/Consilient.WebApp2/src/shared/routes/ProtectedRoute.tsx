import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from "@/shared/hooks/useAuth";
import { ROUTES } from "@/constants";
import type { ReactNode } from 'react';

interface ProtectedRouteProps {
  children: ReactNode;
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    // Optionally show a spinner here
    return null;
  }

  if (!isAuthenticated) {
    // Redirect to login with the current location so we can redirect back after login
    return <Navigate to={ROUTES.LOGIN} state={{ from: location.pathname }} replace />;
  }

  return children;
};

export default ProtectedRoute;
