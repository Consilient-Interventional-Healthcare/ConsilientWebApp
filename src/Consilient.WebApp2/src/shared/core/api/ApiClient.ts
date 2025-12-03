// Utility to extend config with withCredentials
function withCredentials(config: AxiosRequestConfig = {}): AxiosRequestConfig {
  return { ...config, withCredentials: true };
}

// Utility to map Axios response to ApiResponse
function mapApiResponse<T>(response: { data: unknown; status: number }): ApiResponse<T> {
  return {
    data: response.data as T,
    status: response.status
  };
}
import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios';
export { ApiError } from './api.types';

export interface ApiResponse<T> {
  data: T;
  status: number;
}

export class ApiClient {
  private readonly client: AxiosInstance;
  private readonly baseUrl: string;

  constructor(axiosInstance: AxiosInstance, baseUrl: string) {
    this.client = axiosInstance;
    this.baseUrl = baseUrl;
  }

  async get<T = unknown>(url: string, config: AxiosRequestConfig = {}): Promise<ApiResponse<T>> {
    const response = await this.client.get(url, withCredentials(config));
    return mapApiResponse<T>(response);
  }

  async post<T = unknown>(url: string, data: unknown = {}, config: AxiosRequestConfig = {}): Promise<ApiResponse<T>> {
    const response = await this.client.post(url, data, withCredentials(config));
    return mapApiResponse<T>(response);
  }

  async put<T = unknown>(url: string, data: unknown = {}, config: AxiosRequestConfig = {}): Promise<ApiResponse<T>> {
    const response = await this.client.put(url, data, withCredentials(config));
    return mapApiResponse<T>(response);
  }

  async patch<T = unknown>(url: string, data: unknown = {}, config: AxiosRequestConfig = {}): Promise<ApiResponse<T>> {
    const response = await this.client.patch(url, data, withCredentials(config));
    return mapApiResponse<T>(response);
  }

  async delete<T = unknown>(url: string, config: AxiosRequestConfig = {}): Promise<ApiResponse<T>> {
    const response = await this.client.delete(url, withCredentials(config));
    return mapApiResponse<T>(response);
  }

  getBaseUrl(): string {
    return this.baseUrl;
  }

  isCancelError(error: unknown): boolean {
    return axios.isCancel(error);
  }
}

import { ApiClientFactory } from './ApiClientFactory';

const apiClient = ApiClientFactory.create();
export default apiClient;
