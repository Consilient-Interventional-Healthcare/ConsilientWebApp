import appSettings from '@/config/index';
import { logger } from '@/shared/core/logging/Logger';
import type { RequestConfigWithMetadata, RetryableRequestConfig } from '../../api.types';

/**
 * Response interceptor success handler
 * Logs successful requests in development mode and resets retry flags
 */
export function logSuccessfulRequest<T = unknown>(response: { 
  data: T; 
  status: number; 
  config: RequestConfigWithMetadata & RetryableRequestConfig;
}) {
  // Reset token retry flag on successful response to allow future token refresh attempts
  if (response.config._tokenRetry) {
    delete response.config._tokenRetry;
  }
  
  if (appSettings.app.isDevelopment) {
    const reqConfig = response.config;
    const duration = reqConfig.metadata?.startTime 
      ? Date.now() - reqConfig.metadata.startTime
      : undefined;
    
    logger.debug('API request successful', {
      component: 'ApiClient.ResponseInterceptor',
      method: reqConfig.method?.toUpperCase(),
      url: reqConfig.url,
      status: response.status,
      duration,
    });
  }
  return response;
}
