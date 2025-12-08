// React Query Configuration
export const QUERY_STALE_TIME: number = 5 * 60 * 1000; // 5 minutes
export const QUERY_GC_TIME: number = 10 * 60 * 1000; // 10 minutes (was cacheTime)

// API and Network Configuration
export const API_CONSTANTS = {
  /** Exponential backoff base multiplier in milliseconds (1s) */
  RETRY_BACKOFF_BASE_MS: 1000,
  
  /** Remote logging request timeout in milliseconds */
  REMOTE_LOG_TIMEOUT_MS: 5000,
} as const;

// Route Paths
export const ROUTES = {
  HOME: '/',
  LOGIN: '/auth/login',
  DASHBOARD: '/',
  LOGOUT: '/auth/logout',
  MICROSOFT_CALLBACK: '/auth/callback/microsoft',
};

// HTTP Status Codes
export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  SERVER_ERROR: 500,
  SERVICE_UNAVAILABLE: 503,
} as const;

// Claim Types for JWT/Authentication
export const CLAIM_TYPES = {
  NAME_IDENTIFIER: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
  NAME: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
  EMAIL: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
} as const;