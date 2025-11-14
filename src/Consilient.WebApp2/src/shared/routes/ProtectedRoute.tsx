import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from "@/shared/hooks/useAuth";
import Loading from "@/shared/components/Loading";
import { ROUTES } from "@/constants";
import type { ReactNode } from 'react';

interface ProtectedRouteProps {
  children: ReactNode;
}

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return <Loading message="Authenticating..." />;
  }

  if (!isAuthenticated) {
    // Redirect to login and save the attempted location
    return <Navigate to={ROUTES.LOGIN} state={{ from: location }} replace />;
  }

  return children;
}
