/**
 * Loki format type definitions
 * Used for remote log transport to Loki-compatible backends
 */

/**
 * Loki label structure
 * Labels are indexed and used for filtering in Loki
 */
export interface LokiLabel {
  app: string;
  environment: string;
  level: string;
  component?: string;
}

/**
 * Loki stream structure
 * Contains labels and an array of log entries
 */
export interface LokiStream {
  stream: LokiLabel;
  values: [string, string][]; // [timestamp, log line]
}

/**
 * Loki payload structure
 * Top-level structure for Loki API requests
 */
export interface LokiPayload {
  streams: LokiStream[];
}

/**
 * Log context with optional metadata
 */
export interface LogContext {
  component?: string;
  userId?: number;
  action?: string;
  [key: string]: unknown;
}
