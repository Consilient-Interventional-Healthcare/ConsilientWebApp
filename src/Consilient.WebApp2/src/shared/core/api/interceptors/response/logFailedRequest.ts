import { type AxiosError } from 'axios';
import { logger } from '@/shared/core/logging/Logger';
import type { RetryableRequestConfig } from '../../api.types';

/**
 * Log failed API requests
 */
export function logFailedRequest(
  error: AxiosError,
  originalRequest: RetryableRequestConfig,
  status?: number
): void {
  logger.error('API request failed', error, {
    component: 'ApiClient.ResponseInterceptor',
    status,
    url: originalRequest?.url,
    method: originalRequest?.method?.toUpperCase(),
  });
}
