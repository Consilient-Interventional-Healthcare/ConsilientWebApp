import axios, { type AxiosError, type AxiosInstance } from 'axios';
import { HTTP_STATUS } from '@/constants';
import { ApiError, type RetryableRequestConfig } from '../../api.types';
import { handleUnauthorizedError } from './handleUnauthorizedError';
import { shouldRetryRequest } from './shouldRetryRequest';
import { retryRequest } from './retryRequest';
import { logFailedRequest } from './logFailedRequest';

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
  
  // Try to refresh token on 401 Unauthorized
  if (status === HTTP_STATUS.UNAUTHORIZED && !originalRequest._tokenRetry) {
    return handleUnauthorizedError(originalRequest, message, status, error, axiosInstance);
  }

  // Retry logic for network errors and 5xx server errors
  if (shouldRetryRequest(originalRequest, status)) {
    return retryRequest(originalRequest, axiosInstance);
  }
  
  // Log all failed requests
  logFailedRequest(error, originalRequest, status);
  
  return Promise.reject(new ApiError(message, status, error.code, error.response?.data));
}
