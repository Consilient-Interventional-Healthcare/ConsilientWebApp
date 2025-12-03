import { Navigate } from 'react-router-dom';
import { useAuth } from "@/shared/hooks/useAuth";
import { logger } from "@/shared/core/logging/Logger";
import Loading from "@/shared/components/Loading";
import { ROUTES } from "@/constants";
import type { ReactNode } from 'react';

interface PublicRouteProps {
  children: ReactNode;
}

export default function PublicRoute({ children }: PublicRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();
  logger.debug('PublicRoute component - checking isAuthenticated for redirect', { component: 'PublicRoute', isAuthenticated, isLoading, currentPath: window.location.pathname });

  if (isLoading) {
    return <Loading message="Loading..." />;
  }

  if (isAuthenticated) {
    logger.info('PublicRoute component - User is authenticated, redirecting to dashboard', { component: 'PublicRoute' });
    return <Navigate to={ROUTES.DASHBOARD} replace />;
  } else {
    logger.debug('PublicRoute component - User not authenticated, rendering children', { component: 'PublicRoute' });
  }

  return children;
}
