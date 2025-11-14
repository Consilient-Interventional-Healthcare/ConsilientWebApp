import { type AxiosError, type AxiosInstance } from 'axios';
import { logger } from '@/shared/core/logging/logger';
import { msalService } from '@/features/auth/services/MsalService';
import { ApiError, type RetryableRequestConfig } from '../../api.types';

/**
 * Handle 401 Unauthorized errors by attempting token refresh
 */
export async function handleUnauthorizedError(
  originalRequest: RetryableRequestConfig,
  message: string,
  status: number | undefined,
  error: AxiosError,
  axiosInstance: AxiosInstance
): Promise<never> {
  originalRequest._tokenRetry = true;
  
  logger.warn('Unauthorized request, attempting token refresh', {
    component: 'ApiClient.ResponseInterceptor',
    url: originalRequest?.url,
  });
  
  try {
    // Attempt to get a fresh token from MSAL
    const token = await msalService.getAccessToken();
    
    if (token && originalRequest.headers) {
      // Update the failed request with new token and retry
      originalRequest.headers.Authorization = `Bearer ${token}`;
      return axiosInstance(originalRequest);
    }
  } catch (tokenError) {
    logger.error('Token refresh failed, redirecting to login', tokenError as Error, {
      component: 'ApiClient.ResponseInterceptor',
    });
  }
  
  // If token refresh fails, MSAL will handle redirect to login
  // We don't do hard redirect here to avoid losing app state unnecessarily
  return Promise.reject(new ApiError(message, status, error.code, error.response?.data));
}
