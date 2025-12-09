/**
 * Common API types used across the WebApp2 project.
 *
 * Keep this file small and focused on transport-level types that multiple
 * modules can share (responses, errors, pagination, common query shapes).
 */

/**
 * Supported HTTP methods for typed endpoints.
 */
export type HttpMethod =
  | 'GET'
  | 'POST'
  | 'PUT'
  | 'PATCH'
  | 'DELETE'
  | 'OPTIONS'
  | 'HEAD';

/**
 * Generic API error shape returned by the backend.
 */
export interface ApiError {
  /** Short machine-friendly error code (optional). */
  code?: string;
  /** Human readable error message. */
  message: string;
  /** Optional additional structured details. */
  details?: unknown;
  /** HTTP status code when available. */
  status?: number;
}

/**
 * Shape used for validation errors keyed by field name.
 * Each field may have one or more error messages.
 */
export interface ValidationErrors {
  [field: string]: string[];
}

/**
 * Generic API response envelope.
 * - when `success` is true, `data` is populated.
 * - when `success` is false, `error` is populated.
 */
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: ApiError | ValidationErrors;
  /** Optional metadata from the server (e.g. timestamps, request id). */
  meta?: Record<string, unknown>;
}

/**
 * Standard paginated result returned by list endpoints.
 */
export interface Paginated<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  pages: number;
}

/**
 * Sort direction used in list queries.
 */
export type SortDirection = 'asc' | 'desc';

/**
 * Standard list/query parameters used by list endpoints.
 */
export interface ListQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDir?: SortDirection;
  /**
   * Arbitrary filters where the key is the filter name and the value can be a
   * primitive or an array of primitives for multi-select filters.
   */
  filters?: Record<string, string | number | boolean | Array<string | number | boolean>>;
}

/**
 * Small helper: nullable type.
 */
export type Nullable<T> = T | null;

/**
 * Metadata for a typed endpoint.
 */
export interface EndpointMeta {
  path: string;
  method: HttpMethod;
  requiresAuth?: boolean;
  description?: string;
}

/**
 * Type for a client-side handler function for an endpoint.
 * Accepts a request payload and returns a Promise of ApiResponse.
 */
export type ApiHandler<TReq, TRes> = (payload: TReq) => Promise<ApiResponse<TRes>>;

/**
 * A map describing available API endpoints and their metadata.
 * Key typically matches an internal identifier for the endpoint.
 */
export type ApiMap = Record<string, EndpointMeta>;