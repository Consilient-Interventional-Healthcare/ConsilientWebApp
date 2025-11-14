import type { InternalAxiosRequestConfig } from 'axios';

/**
 * Custom API Error class for structured error handling
 */
export class ApiError extends Error {
  constructor(
    message: string,
    public status?: number,
    public code?: string,
    public details?: unknown
  ) {
    super(message);
    this.name = 'ApiError';
    // Fix prototype chain for proper instanceof checks in transpiled code
    Object.setPrototypeOf(this, ApiError.prototype);
  }
}

/**
 * Extended Axios request config with metadata for tracking
 */
export interface RequestConfigWithMetadata extends InternalAxiosRequestConfig {
  metadata?: {
    startTime: number;
  };
}

/**
 * Extended request config with retry tracking
 */
export interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: number;
  _tokenRetry?: boolean;
}
