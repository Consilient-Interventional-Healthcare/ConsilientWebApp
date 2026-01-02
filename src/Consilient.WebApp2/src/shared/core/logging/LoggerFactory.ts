import log from 'loglevel';
import { filterPII, filterMessagePII } from './piiFilter';
import type { LogContext } from './logging.types';
import { RemoteLogTransport } from './remoteTransport';
import { Logger, type LogLevel } from './Logger';
import { appSettings } from '@/config/index';

/**
 * Factory class for creating and configuring Logger instances
 * 
 * Responsibilities:
 * - Creates loglevel instances with proper configuration
 * - Configures environment-specific log levels
 * - Sets up custom formatting and PII filtering
 * - Configures remote transport integration
 */
export class LoggerFactory {
  /**
   * Create a fully configured Logger instance
   * 
   * @param customLevel - Optional custom log level override
   * @returns Configured Logger instance
   */
  static create(customLevel?: LogLevel): Logger {
    const logInstance = log;
    
    this.configureLogLevel(logInstance, customLevel);
    this.configureCustomFormatting(logInstance);
    
    return new Logger(logInstance);
  }

  /**
   * Configure log level based on environment or custom override
   */
  private static configureLogLevel(logInstance: log.Logger, customLevel?: LogLevel): void {
    if (customLevel) {
      logInstance.setLevel(customLevel);
      return;
    }

    if (appSettings.app.isDevelopment) {
      logInstance.setLevel('trace'); // Show all logs in development
    } else if (appSettings.app.isProduction) {
      logInstance.setLevel('warn'); // Only warnings and errors in production console
    } else {
      logInstance.setLevel('info'); // Info+ in staging
    }
  }

  /**
   * Configure custom formatting for console output with PII filtering
   */
  private static configureCustomFormatting(logInstance: log.Logger): void {
    const originalFactory = logInstance.methodFactory;
    
    logInstance.methodFactory = (methodName, logLevel, loggerName) => {
      const rawMethod = originalFactory(methodName, logLevel, loggerName);

      return (...messages: unknown[]) => {
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
          
          // Filter PII from console output to prevent accidental exposure
          const filteredContext = filterPII(context);
          const contextStr = filteredContext && Object.keys(filteredContext).length > 0 
            ? ` | ${JSON.stringify(filteredContext)}` 
            : '';
          
          // Filter PII from message strings for console output
          const filteredMessages = messages.map(m => 
            typeof m === 'string' ? filterMessagePII(m) : m
          );
          
          rawMethod(prefix, ...filteredMessages, contextStr);

          // Send to remote API if enabled
          if (appSettings.logging.enabled) {
            const logMessage = filteredMessages.map(m => String(m)).join(' ');
            void RemoteLogTransport.send(methodName, logMessage, context);
          }
        } else {
          // Filter PII from message strings even without context
          const filteredMessages = messages.map(m => 
            typeof m === 'string' ? filterMessagePII(m) : m
          );
          
          rawMethod(prefix, ...filteredMessages);

          // Send to remote API if enabled
          if (appSettings.logging.enabled) {
            const logMessage = filteredMessages.map(m => String(m)).join(' ');
            void RemoteLogTransport.send(methodName, logMessage);
          }
        }
      };
    };
    
    logInstance.setLevel(logInstance.getLevel()); // Trigger methodFactory
  }
}
