import { Navigate } from 'react-router-dom';
import { useAuth } from "@/hooks/useAuth";
import Loading from "@/components/common/Loading";
import { ROUTES } from "@/constants";
import type { ReactNode } from 'react';

interface PublicRouteProps {
  children: ReactNode;
}

export default function PublicRoute({ children }: PublicRouteProps) {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return <Loading message="Loading..." />;
  }

  if (isAuthenticated) {
    // Redirect authenticated users to dashboard
    return <Navigate to={ROUTES.DASHBOARD} replace />;
  }

  return children;
}
