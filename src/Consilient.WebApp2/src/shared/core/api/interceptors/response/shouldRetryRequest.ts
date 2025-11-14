import config from '@/config';
import type { RetryableRequestConfig } from '../../api.types';

/**
 * Determine if a request should be retried
 */
export function shouldRetryRequest(originalRequest: RetryableRequestConfig, status?: number): boolean {
  return Boolean(
    originalRequest &&
    (!status || status >= 500) &&
    (!originalRequest._retry || originalRequest._retry < config.api.retryAttempts)
  );
}
