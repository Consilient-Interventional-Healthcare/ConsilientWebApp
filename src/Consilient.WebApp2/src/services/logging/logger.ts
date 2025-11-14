/**
 * Centralized logging service using loglevel
 * Logs to console and sends to backend API for remote storage
 */
import log from 'loglevel';
import { isDevelopment, isProduction } from '@/config/env';
import config from '@/config';
import api from '@/services/api/ApiClient';

interface LogContext {
  component?: string;
  userId?: number;
  action?: string;
  [key: string]: unknown;
}

interface LogPayload {
  app: string;
  environment: string;
  level: string;
  message: string;
  timestamp: string;
  context?: LogContext;
}

/**
 * Send logs to backend API for storage/forwarding
 */
async function sendToRemote(level: string, message: string, context?: LogContext): Promise<void> {
  if (!config.logging.enabled) {
    return;
  }

  if (isDevelopment) {
    console.log('[Logger] Sending to API:', { level, message, context });
  }

  try {
    const payload: LogPayload = {
      app: config.app.name,
      environment: config.app.environment,
      level: level.toUpperCase(),
      message,
      timestamp: new Date().toISOString(),
      context,
    };

    // Send to backend API endpoint
    await api.post('/loki/logs', payload);
    
    if (isDevelopment) {
      console.log('[Logger] Successfully sent to API');
    }
  } catch (error) {
    // Silently fail in production, show errors in development
    if (isDevelopment) {
      console.error('[Logger] Error sending to API:', error);
    }
  }
}

/**
 * Initialize the logger
 */
function initializeLogger() {
  // Set log level based on environment
  if (isDevelopment) {
    log.setLevel('trace'); // Show all logs in development
  } else if (isProduction) {
    log.setLevel('warn'); // Only warnings and errors in production console
  } else {
    log.setLevel('info'); // Info+ in staging
  }

  // Custom formatting for console output
  const originalFactory = log.methodFactory;
  log.methodFactory = function (methodName, logLevel, loggerName) {
    const rawMethod = originalFactory(methodName, logLevel, loggerName);

    return function (this: unknown, ...messages: unknown[]) {
      const timestamp = new Date().toISOString();
      const level = methodName.toUpperCase().padEnd(5);
      const prefix = `[${timestamp}] ${level}`;

      // Check if last argument is context object
      const lastArg = messages[messages.length - 1];
      const hasContext =
        lastArg &&
        typeof lastArg === 'object' &&
        !Array.isArray(lastArg) &&
        !(lastArg instanceof Error);

      if (hasContext) {
        const context = messages.pop() as LogContext;
        const contextStr = Object.keys(context).length > 0 ? ` | ${JSON.stringify(context)}` : '';
        rawMethod(prefix, ...messages, contextStr);

        // Send to remote API if enabled
        if (config.logging.enabled) {
          const logMessage = messages.map(m => String(m)).join(' ');
          void sendToRemote(methodName, logMessage, context);
        }
      } else {
        rawMethod(prefix, ...messages);

        // Send to remote API if enabled
        if (config.logging.enabled) {
          const logMessage = messages.map(m => String(m)).join(' ');
          void sendToRemote(methodName, logMessage);
        }
      }
    };
  };
  log.setLevel(log.getLevel()); // Trigger methodFactory
}

/**
 * Enhanced logger with context support
 */
export const logger = {
  /**
   * Trace level logging (verbose debugging)
   */
  trace: (message: string, context?: LogContext) => {
    log.trace(message, context);
  },

  /**
   * Debug level logging
   */
  debug: (message: string, context?: LogContext) => {
    log.debug(message, context);
  },

  /**
   * Info level logging
   */
  info: (message: string, context?: LogContext) => {
    log.info(message, context);
  },

  /**
   * Warning level logging
   */
  warn: (message: string, context?: LogContext) => {
    log.warn(message, context);
  },

  /**
   * Error level logging
   */
  error: (message: string, error?: Error, context?: LogContext) => {
    const errorContext: LogContext = {
      ...context,
      error: error ? {
        name: error.name,
        message: error.message,
        stack: error.stack,
      } : undefined,
    };
    log.error(message, errorContext);
  },

  /**
   * Set log level dynamically
   */
  setLevel: (level: 'trace' | 'debug' | 'info' | 'warn' | 'error' | 'silent') => {
    log.setLevel(level);
  },

  /**
   * Get current log level
   */
  getLevel: () => {
    return log.getLevel();
  },
};

// Initialize logger on module load
initializeLogger();

export default logger;
