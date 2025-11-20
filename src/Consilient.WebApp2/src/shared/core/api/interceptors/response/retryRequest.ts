import { type AxiosInstance } from 'axios';
import config from '@/config';
import { logger } from '@/shared/core/logging/Logger';
import type { RetryableRequestConfig } from '../../api.types';
import { sleep } from './sleep';

// Retry backoff base multiplier in milliseconds (1s)
const RETRY_BACKOFF_BASE_MS = 1000;

/**
 * Retry a failed request with exponential backoff
 */
export async function retryRequest(
  originalRequest: RetryableRequestConfig,
  axiosInstance: AxiosInstance
): Promise<never> {
  const retryCount = originalRequest._retry ?? 0;
  originalRequest._retry = retryCount + 1;
  
  // Exponential backoff: 1s, 2s, 4s
  const delay = Math.pow(2, retryCount) * RETRY_BACKOFF_BASE_MS;
  await sleep(delay);
  
  logger.warn(`Retrying request (attempt ${retryCount + 1}/${config.api.retryAttempts})`, {
    component: 'ApiClient.ResponseInterceptor',
    url: originalRequest.url,
    method: originalRequest.method?.toUpperCase(),
    status: undefined,
  });
  
  return axiosInstance(originalRequest);
}
