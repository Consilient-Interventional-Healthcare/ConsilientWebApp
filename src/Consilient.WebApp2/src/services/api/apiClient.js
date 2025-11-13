import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
});

// Request interceptor - add auth token
// apiClient.interceptors.request.use(
//   (config) => {
//     const token = localStorage.getItem('authToken');
//     if (token) {
//       config.headers.Authorization = `Bearer ${token}`;
//     }
//     return config;
//   },
//   (error) => {
//     return Promise.reject(error);
//   }
// );

// Response interceptor - handle errors globally
// apiClient.interceptors.response.use(
//   (response) => response,
//   (error) => {
//     if (error.response?.status === 401) {
//       // Handle unauthorized - redirect to login
//       localStorage.removeItem('authToken');
//       window.location.href = '/login';
//     }
//     return Promise.reject(error);
//   }
// );

// Encapsulated API methods
const api = {
  get: async (url, config = {}) => {
      const response = await apiClient.get(url, config);
      return response.data;
  },

  post: async (url, data = {}, config = {}) => {
      const response = await apiClient.post(url, data, config);
      return response.data;
  },

  put: async (url, data = {}, config = {}) => {
      const response = await apiClient.put(url, data, config);
      return response.data;
  },

  patch: async (url, data = {}, config = {}) => {
      const response = await apiClient.patch(url, data, config);
      return response.data;
  },

  delete: async (url, config = {}) => {
      const response = await apiClient.delete(url, config);
      return response.data;
  },
};

export default api;