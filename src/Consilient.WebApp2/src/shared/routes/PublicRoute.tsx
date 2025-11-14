import { Navigate } from 'react-router-dom';
import { useAuth } from "@/shared/hooks/useAuth";
import Loading from "@/shared/components/Loading";
import { ROUTES } from "@/constants";
import type { ReactNode } from 'react';

interface PublicRouteProps {
  children: ReactNode;
}

export default function PublicRoute({ children }: PublicRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <Loading message="Loading..." />;
  }

  if (isAuthenticated) {
    // Redirect authenticated users to dashboard
    return <Navigate to={ROUTES.DASHBOARD} replace />;
  }

  return children;
}
