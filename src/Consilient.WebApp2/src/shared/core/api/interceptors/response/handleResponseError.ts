import axios, { type AxiosError, type AxiosInstance } from 'axios';
import { HTTP_STATUS } from '@/constants';
import { ApiError, type RetryableRequestConfig } from '../../api.types';
import { shouldRetryRequest } from './shouldRetryRequest';
import { retryRequest } from './retryRequest';
import { logFailedRequest } from './logFailedRequest';
import { getAuthService } from '@/features/auth/services/AuthServiceFactory';
import { logger } from '@/shared/core/logging/Logger';
import { dispatchSessionExpired } from '@/features/auth/auth.events';

// Flag to prevent multiple simultaneous logout calls
let isLoggingOut = false;

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
    // Avoid handling 401 for login/logout endpoints to prevent infinite loops
    const url = originalRequest.url ?? '';
    const isAuthEndpoint = url.includes('/auth/login') || url.includes('/auth/logout') || url.includes('/auth/authenticate');

    if (!isAuthEndpoint && !isLoggingOut) {
      logger.warn('Session expired (401), logging out user', { component: 'handleResponseError' });

      // Set flag to prevent multiple simultaneous logout calls
      isLoggingOut = true;

      // Perform logout and dispatch event for navigation
      const authService = getAuthService();
      authService.logout()
        .catch((logoutError) => {
          logger.error('Failed to logout on 401', logoutError as Error, { component: 'handleResponseError' });
        })
        .finally(() => {
          // Dispatch custom event to trigger navigation in React Router context
          const currentPath = window.location.pathname;
          dispatchSessionExpired(currentPath);

          // Reset flag after logout process completes
          // Small delay to ensure all pending requests complete
          setTimeout(() => {
            isLoggingOut = false;
          }, 1000);
        });
    }

    return Promise.reject(new ApiError(message, status, error.code, error.response?.data));
  }

  // Retry logic for network errors and 5xx server errors
  if (shouldRetryRequest(originalRequest, status)) {
    return retryRequest(originalRequest, axiosInstance);
  }
  
  // Log all failed requests
  logFailedRequest(error, originalRequest, status);
  
  return Promise.reject(new ApiError(message, status, error.code, error.response?.data));
}
