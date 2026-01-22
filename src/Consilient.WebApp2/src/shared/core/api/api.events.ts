/**
 * Custom API error events
 * These events allow components outside React context (like API interceptors)
 * to trigger toast notifications within the React context
 */

export type ApiErrorType = 'network' | 'server' | 'timeout' | 'unknown';

export interface ApiErrorEventDetail {
  type: ApiErrorType;
  message: string;
  status?: number;
  code?: string;
}

// Extend the WindowEventMap to include our custom events
declare global {
  interface WindowEventMap {
    'api:error': CustomEvent<ApiErrorEventDetail>;
  }
}

/**
 * Dispatch an API error event
 * This should be called from the response error interceptor
 */
export function dispatchApiError(detail: ApiErrorEventDetail): void {
  window.dispatchEvent(new CustomEvent('api:error', { detail }));
}

/**
 * Determine the error type from Axios error properties
 */
export function categorizeApiError(status?: number, code?: string): ApiErrorType {
  // Network errors - no response received
  if (!status && code === 'ERR_NETWORK') {
    return 'network';
  }

  // Timeout errors
  if (code === 'ECONNABORTED' || code === 'ETIMEDOUT') {
    return 'timeout';
  }

  // Server errors (5xx)
  if (status && status >= 500) {
    return 'server';
  }

  return 'unknown';
}
