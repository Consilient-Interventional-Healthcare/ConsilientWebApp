import type log from 'loglevel';
import type { LogContext } from './logging.types';

export type LogLevel = 'trace' | 'debug' | 'info' | 'warn' | 'error' | 'silent';

/**
 * Centralized logging service using loglevel
 * Logs to console and sends to backend API in Loki format for remote storage
 */
export class Logger {
  private logInstance: log.Logger;

  constructor(logInstance: log.Logger) {
    this.logInstance = logInstance;
  }

  // ============================================================================
  // Public Logging Methods
  // ============================================================================

  /**
   * Trace level logging (verbose debugging)
   */
  trace(message: string, context?: LogContext): void {
    this.logInstance.trace(message, context);
  }

  /**
   * Debug level logging
   */
  debug(message: string, context?: LogContext): void {
    this.logInstance.debug(message, context);
  }

  /**
   * Info level logging
   */
  info(message: string, context?: LogContext): void {
    this.logInstance.info(message, context);
  }

  /**
   * Warning level logging
   */
  warn(message: string, context?: LogContext): void {
    this.logInstance.warn(message, context);
  }

  /**
   * Error level logging
   */
  error(message: string, error?: Error, context?: LogContext): void {
    const errorContext: LogContext = {
      ...context,
      error: error ? {
        name: error.name,
        message: error.message,
        stack: error.stack,
      } : undefined,
    };
    this.logInstance.error(message, errorContext);
  }

  // ============================================================================
  // Configuration Methods
  // ============================================================================

  /**
   * Set log level dynamically
   */
  setLevel(level: LogLevel): void {
    this.logInstance.setLevel(level);
  }

  /**
   * Get current log level
   */
  getLevel(): number {
    return this.logInstance.getLevel();
  }
}

// ============================================================================
// Singleton Export
// ============================================================================

// Export singleton instance created via factory
import { LoggerFactory } from './LoggerFactory';

export const logger = LoggerFactory.create();
export default logger;
