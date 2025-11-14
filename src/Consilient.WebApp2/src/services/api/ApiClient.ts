import axios, { type AxiosInstance, type AxiosRequestConfig, type AxiosError } from 'axios';
import { STORAGE_KEYS, ROUTES, HTTP_STATUS, storage } from '@/constants';
import config from '@/config';

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
  }
}

const API_BASE_URL = config.api.baseUrl;

const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: config.api.timeout,
});

// Request interceptor - add auth token
apiClient.interceptors.request.use(
  (config) => {
    const token = storage.getString(STORAGE_KEYS.AUTH_TOKEN);
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor - handle errors globally
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<{ message?: string }>) => {
    const status = error.response?.status;
    const message = error.response?.data?.message ?? error.message ?? 'An unexpected error occurred';
    
    if (status === HTTP_STATUS.UNAUTHORIZED) {
      // Handle unauthorized - redirect to login
      storage.remove(STORAGE_KEYS.AUTH_TOKEN);
      storage.remove(STORAGE_KEYS.USER_DATA);
      window.location.href = ROUTES.LOGIN;
    }
    
    return Promise.reject(new ApiError(message, status, error.code, error.response?.data));
  }
);

// Encapsulated API methods
const api = {
  get: async <T = unknown>(url: string, config: AxiosRequestConfig = {}): Promise<T> => {
    const response = await apiClient.get<T>(url, config);
    return response.data;
  },

  post: async <T = unknown>(url: string, data: unknown = {}, config: AxiosRequestConfig = {}): Promise<T> => {
    const response = await apiClient.post<T>(url, data, config);
    return response.data;
  },

  put: async <T = unknown>(url: string, data: unknown = {}, config: AxiosRequestConfig = {}): Promise<T> => {
    const response = await apiClient.put<T>(url, data, config);
    return response.data;
  },

  patch: async <T = unknown>(url: string, data: unknown = {}, config: AxiosRequestConfig = {}): Promise<T> => {
    const response = await apiClient.patch<T>(url, data, config);
    return response.data;
  },

  delete: async <T = unknown>(url: string, config: AxiosRequestConfig = {}): Promise<T> => {
    const response = await apiClient.delete<T>(url, config);
    return response.data;
  },
};

export default api;
