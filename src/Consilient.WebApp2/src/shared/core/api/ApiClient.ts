import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios';

export { ApiError } from './api.types';


export class ApiClient {
  private readonly client: AxiosInstance;
  private readonly baseUrl: string;

  constructor(axiosInstance: AxiosInstance, baseUrl: string) {
    this.client = axiosInstance;
    this.baseUrl = baseUrl;
  }

  async get<T = unknown>(url: string, config: AxiosRequestConfig = {}): Promise<T> {
    const response = await this.client.get<T>(url, config);
    return response.data;
  }

  async post<T = unknown>(url: string, data: unknown = {}, config: AxiosRequestConfig = {}): Promise<T> {
    const response = await this.client.post<T>(url, data, config);
    return response.data;
  }

  async put<T = unknown>(url: string, data: unknown = {}, config: AxiosRequestConfig = {}): Promise<T> {
    const response = await this.client.put<T>(url, data, config);
    return response.data;
  }

  async patch<T = unknown>(url: string, data: unknown = {}, config: AxiosRequestConfig = {}): Promise<T> {
    const response = await this.client.patch<T>(url, data, config);
    return response.data;
  }

  async delete<T = unknown>(url: string, config: AxiosRequestConfig = {}): Promise<T> {
    const response = await this.client.delete<T>(url, config);
    return response.data;
  }

  getBaseUrl(): string {
    return this.baseUrl;
  }

  getInstance(): AxiosInstance {
    return this.client;
  }

  /**
   * Create an AbortController for request cancellation
   * Use with the 'signal' option in request config
   * 
   * @example
   * ```typescript
   * const controller = apiClient.createAbortController();
   * 
   * try {
   *   const data = await apiClient.get('/employees', { 
   *     signal: controller.signal 
   *   });
   * } catch (error) {
   *   if (apiClient.isCancelError(error)) {
   *     // Request was canceled
   *   }
   * }
   * 
   * // Later, to cancel:
   * controller.abort();
   * ```
   * 
   * @example React hook cleanup
   * ```typescript
   * useEffect(() => {
   *   const controller = apiClient.createAbortController();
   *   
   *   fetchData({ signal: controller.signal });
   *   
   *   return () => controller.abort(); // Cleanup on unmount
   * }, []);
   * ```
   */
  createAbortController(): AbortController {
    return new AbortController();
  }

  /**
   * Check if an error is a request cancellation error
   * Use this to distinguish cancellations from actual errors
   * 
   * @param error - The error to check
   * @returns true if the error is from a canceled request
   */
  isCancelError(error: unknown): boolean {
    return axios.isCancel(error);
  }
}

// ============================================================================
// Singleton Instance
// ============================================================================

// Export singleton instance created via factory
import { ApiClientFactory } from './ApiClientFactory';

const apiClient = ApiClientFactory.create();
export default apiClient;
