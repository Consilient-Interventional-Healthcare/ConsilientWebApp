import { logger } from '@/services/logging/logger';

// React Query Configuration
export const QUERY_STALE_TIME: number = 5 * 60 * 1000; // 5 minutes
export const QUERY_GC_TIME: number = 10 * 60 * 1000; // 10 minutes (was cacheTime)

// Storage Keys
export const STORAGE_KEYS = {
  AUTH_TOKEN: 'authToken',
  USER_DATA: 'user',
} as const;

/**
 * Type-safe storage utilities
 * Provides generic helpers for getting and setting values in sessionStorage
 */
export const storage = {
  /**
   * Get a value from sessionStorage with type safety
   * @param key - Storage key
   * @returns Parsed value or null if not found or parse fails
   */
  get: <T>(key: string): T | null => {
    try {
      const item = sessionStorage.getItem(key);
      return item ? (JSON.parse(item) as T) : null;
    } catch (error) {
      logger.error(`Failed to parse storage item '${key}'`, error instanceof Error ? error : undefined, { component: 'storage' });
      return null;
    }
  },

  /**
   * Set a value in sessionStorage with automatic JSON serialization
   * @param key - Storage key
   * @param value - Value to store
   */
  set: <T>(key: string, value: T): void => {
    try {
      sessionStorage.setItem(key, JSON.stringify(value));
    } catch (error) {
      logger.error(`Failed to set storage item '${key}'`, error instanceof Error ? error : undefined, { component: 'storage' });
    }
  },

  /**
   * Get a raw string value from sessionStorage without parsing
   * @param key - Storage key
   * @returns Raw string value or null if not found
   */
  getString: (key: string): string | null => {
    return sessionStorage.getItem(key);
  },

  /**
   * Set a raw string value in sessionStorage without serialization
   * @param key - Storage key
   * @param value - String value to store
   */
  setString: (key: string, value: string): void => {
    sessionStorage.setItem(key, value);
  },

  /**
   * Remove a value from sessionStorage
   * @param key - Storage key
   */
  remove: (key: string): void => {
    sessionStorage.removeItem(key);
  },

  /**
   * Clear all values from sessionStorage
   */
  clear: (): void => {
    sessionStorage.clear();
  },
};

// Route Paths
export const ROUTES = {
  HOME: '/',
  LOGIN: '/auth/login',
  DASHBOARD: '/',
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