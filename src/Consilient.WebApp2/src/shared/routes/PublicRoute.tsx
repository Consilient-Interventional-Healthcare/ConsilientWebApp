import { Navigate, useSearchParams } from 'react-router-dom';
import { useAuth } from "@/shared/hooks/useAuth";
import { logger } from "@/shared/core/logging/Logger";
import { ROUTES } from "@/constants";
import type { ReactNode } from 'react';

interface PublicRouteProps {
  children: ReactNode;
}

export default function PublicRoute({ children }: PublicRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();
  const [searchParams] = useSearchParams();
  const redirect = searchParams.get('redirect');

  logger.debug('PublicRoute component - checking isAuthenticated for redirect', { component: 'PublicRoute', isAuthenticated, isLoading, currentPath: window.location.pathname });

  if (isLoading) {
    // Optionally show a spinner here
    return null;
  }

  if (isAuthenticated) {
    const destination = redirect || ROUTES.DASHBOARD;
    logger.info('PublicRoute component - User is authenticated, redirecting', { component: 'PublicRoute', destination });
    return <Navigate to={destination} replace />;
  } else {
    logger.debug('PublicRoute component - User not authenticated, rendering children', { component: 'PublicRoute' });
  }

  return children;
}
