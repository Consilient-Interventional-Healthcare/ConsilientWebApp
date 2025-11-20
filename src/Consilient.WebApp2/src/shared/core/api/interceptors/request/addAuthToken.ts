import { config } from '@/config';
import { logger } from '@/shared/core/logging/Logger';
import { msalService } from '@/features/auth/services/MsalService';
import type { RequestConfigWithMetadata } from '../../api.types';

/**
 * Request interceptor success handler
 * Adds authentication token and performance tracking metadata
 */
export async function addAuthToken(requestConfig: RequestConfigWithMetadata): Promise<RequestConfigWithMetadata> {
  // Add performance tracking metadata
  requestConfig.metadata = {
    ...requestConfig.metadata,
    startTime: Date.now(),
  };

  // Add authentication token
  try {
    const token = await msalService.getAccessToken();
    if (token && requestConfig.headers) {
      requestConfig.headers.Authorization = `Bearer ${token}`;
    }
  } catch (_error) {
    // If token retrieval fails, log it but don't block the request
    // This allows non-authenticated requests to proceed
    if (config.env.isDevelopment) {
      logger.warn('Failed to get access token for request', {
        component: 'ApiClient.RequestInterceptor',
        url: requestConfig.url,
      });
    }
  }

  return requestConfig;
}
