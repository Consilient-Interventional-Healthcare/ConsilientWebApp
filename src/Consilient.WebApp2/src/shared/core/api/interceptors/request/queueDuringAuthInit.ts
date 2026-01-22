import type { AxiosRequestConfig } from 'axios';
import { authStateManager } from '@/features/auth/services/AuthStateManager';
import { logger } from '@/shared/core/logging/Logger';

/**
 * Request interceptor that queues requests until authentication is initialized
 * Prevents race conditions where API calls are made before we know the user's auth state
 *
 * Auth endpoints are excluded from queuing since they are needed for initialization
 */
export async function queueDuringAuthInit(config: AxiosRequestConfig): Promise<AxiosRequestConfig> {
  // Skip queuing for auth-related endpoints
  const authEndpoints = ['/auth/authenticate', '/auth/claims', '/auth/logout', '/auth/link-external', '/auth/me'];
  const isAuthEndpoint = authEndpoints.some(endpoint => config.url?.includes(endpoint));

  if (isAuthEndpoint) {
    return config;
  }

  // Wait for auth initialization before proceeding with the request
  if (!authStateManager.isInitialized()) {
    logger.debug('Request queued until auth initialization completes', {
      component: 'ApiClient.RequestInterceptor',
      url: config.url,
    });

    await authStateManager.waitForAuthInit();

    logger.debug('Request resumed after auth initialization', {
      component: 'ApiClient.RequestInterceptor',
      url: config.url,
    });
  }

  return config;
}
