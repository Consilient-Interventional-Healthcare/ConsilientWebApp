import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from "@/hooks/useAuth";
import Loading from "@/components/common/Loading";
import { ROUTES } from "@/constants";
import type { ReactNode } from 'react';

interface ProtectedRouteProps {
  children: ReactNode;
}

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated, loading } = useAuth();
  const location = useLocation();

  if (loading) {
    return <Loading message="Authenticating..." />;
  }

  if (!isAuthenticated) {
    // Redirect to login and save the attempted location
    return <Navigate to={ROUTES.LOGIN} state={{ from: location }} replace />;
  }

  return children;
}
