import appSettings from '@/config/index';
import type { RetryableRequestConfig } from '../../api.types';

// Flag to enable/disable request retries
const ENABLE_RETRIES = false;

/**
 * Determine if a request should be retried
 */
export function shouldRetryRequest(originalRequest: RetryableRequestConfig, status?: number): boolean {
  if (!ENABLE_RETRIES) {
    return false;
  }

  return Boolean(
    originalRequest &&
    (!status || status >= 500) &&
    (!originalRequest._retry || originalRequest._retry < appSettings.api.retryAttempts)
  );
}
