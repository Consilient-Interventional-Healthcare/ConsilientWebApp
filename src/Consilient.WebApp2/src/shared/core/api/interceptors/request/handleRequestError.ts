import { config } from '@/config';
import { logger } from '@/shared/core/logging/Logger';

/**
 * Request interceptor error handler
 * Logs request configuration errors in development mode
 */
export function handleRequestError(error: Error): Promise<never> {
  if (config.env.isDevelopment) {
    logger.error('Request configuration error', error, {
      component: 'ApiClient.RequestInterceptor',
    });
  }
  return Promise.reject(error);
}
