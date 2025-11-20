import axios, { type AxiosInstance, type AxiosError } from 'axios';
import appSettings from '@/config/index';
import { addAuthToken } from './interceptors/request/addAuthToken';
import { handleRequestError } from './interceptors/request/handleRequestError';
import { logSuccessfulRequest } from './interceptors/response/logSuccessfulRequest';
import { handleResponseError } from './interceptors/response/handleResponseError';
import { ApiClient } from './ApiClient';

// ============================================================================
// API Client Factory
// ============================================================================

/**
 * Factory class for creating and configuring ApiClient instances
 * 
 * Responsibilities:
 * - Creates Axios instances with proper configuration
 * - Configures request and response interceptors
 * - Provides a centralized place for API client instantiation
 */
export class ApiClientFactory {
  /**
   * Create a fully configured ApiClient instance
   * 
   * @param baseUrl - Optional custom base URL (defaults to config.api.baseUrl)
   * @param timeout - Optional custom timeout (defaults to config.api.timeout)
   * @returns Configured ApiClient instance
   */
  static create(baseUrl?: string, timeout?: number): ApiClient {
    const axiosInstance = this.createAxiosInstance(baseUrl, timeout);
    this.configureInterceptors(axiosInstance);
    return new ApiClient(axiosInstance, baseUrl ?? appSettings.api.baseUrl);
  }

  /**
   * Create and configure an Axios instance
   */
  private static createAxiosInstance(baseUrl?: string, timeout?: number): AxiosInstance {
    return axios.create({
      baseURL: baseUrl ?? appSettings.api.baseUrl,
      headers: {
        'Content-Type': 'application/json',
      },
      timeout: timeout ?? appSettings.api.timeout,
    });
  }

  /**
   * Configure request and response interceptors on an Axios instance
   */
  private static configureInterceptors(axiosInstance: AxiosInstance): void {
    // Request interceptor - add authentication and performance tracking
    axiosInstance.interceptors.request.use(
      async (config) => addAuthToken(config),
      (error: AxiosError) => handleRequestError(error)
    );

    // Response interceptor - handle success and errors with retry logic
    axiosInstance.interceptors.response.use(
      (response) => logSuccessfulRequest(response),
      async (error: AxiosError<{ message?: string }>) => handleResponseError(error, axiosInstance)
    );
  }
}
