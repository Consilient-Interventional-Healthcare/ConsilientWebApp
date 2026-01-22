import axios, { type AxiosError, type AxiosInstance } from 'axios';
import { HTTP_STATUS } from '@/constants';
import { ApiError, type RetryableRequestConfig } from '../../api.types';
import { shouldRetryRequest } from './shouldRetryRequest';
import { retryRequest } from './retryRequest';
import { logFailedRequest } from './logFailedRequest';
import { AuthService } from '@/features/auth/services/AuthService';
import { logger } from '@/shared/core/logging/Logger';
import { dispatchSessionExpired } from '@/features/auth/auth.events';
import { dispatchApiError, categorizeApiError } from '../../api.events';

// Promise-based lock to prevent multiple simultaneous logout calls
// Using a promise instead of a boolean flag prevents race conditions
let logoutPromise: Promise<void> | null = null;

/**
 * Response interceptor error handler
 * Handles retry logic, token refresh, and logging
 */
export async function handleResponseError(
  error: AxiosError<{ message?: string }>,
  axiosInstance: AxiosInstance
): Promise<never> {
  // Don't log or retry canceled requests
  if (axios.isCancel(error)) {
    return Promise.reject(error);
  }
  
  const originalRequest = error.config as RetryableRequestConfig;
  const status = error.response?.status;
  const message = error.response?.data?.message ?? error.message ?? 'An unexpected error occurred';

  // Handle 401 Unauthorized - session expired
  if (status === HTTP_STATUS.UNAUTHORIZED && !originalRequest._tokenRetry) {
    // Avoid handling 401 for auth endpoints to prevent infinite loops
    // /auth/claims is included because it's expected to return 401 when checking if user is logged in
    const url = originalRequest.url ?? '';
    const isAuthEndpoint = url.includes('/auth/login') || url.includes('/auth/logout') || url.includes('/auth/authenticate') || url.includes('/auth/claims');

    if (!isAuthEndpoint && !logoutPromise) {
      logger.warn('Session expired (401), logging out user', { component: 'handleResponseError' });

      // Create logout promise to prevent concurrent logout attempts
      const authService = new AuthService();
      logoutPromise = (async () => {
        try {
          await authService.logout();
        } catch (logoutError) {
          logger.error('Failed to logout on 401', logoutError as Error, { component: 'handleResponseError' });
        } finally {
          // Dispatch custom event to trigger navigation in React Router context
          // Don't redirect if already on auth pages (login, logout) to avoid loops
          const currentPath = window.location.pathname;
          if (!currentPath.startsWith('/auth/login') && !currentPath.startsWith('/auth/logout')) {
            dispatchSessionExpired(currentPath);
          }

          // Reset promise after logout process completes
          // Small delay to ensure all pending requests complete
          setTimeout(() => {
            logoutPromise = null;
          }, 1000);
        }
      })();
    }

    return Promise.reject(new ApiError(message, status, error.code, error.response?.data));
  }

  // Retry logic for network errors and 5xx server errors
  if (shouldRetryRequest(originalRequest, status)) {
    return retryRequest(originalRequest, axiosInstance);
  }
  
  // Log all failed requests
  logFailedRequest(error, originalRequest, status);

  // Dispatch API error event for toast notifications
  // Excludes 401 (handled by session expiration) and 4xx (business logic errors)
  const errorType = categorizeApiError(status, error.code);
  if (errorType !== 'unknown') {
    dispatchApiError({
      type: errorType,
      message,
      ...(status !== undefined && { status }),
      ...(error.code !== undefined && { code: error.code }),
    });
  }

  return Promise.reject(new ApiError(message, status, error.code, error.response?.data));
}
