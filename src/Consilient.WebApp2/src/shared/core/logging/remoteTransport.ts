import axios from 'axios';
import { config } from '@/config';
import { filterPII, filterMessagePII } from './piiFilter';
import type { LokiLabel, LokiPayload, LogContext } from './logging.types';

// Remote logging timeout constant (5 seconds)
const REMOTE_LOG_TIMEOUT_MS = 5000;

/**
 * Remote log transport for sending logs to backend API in Loki format
 */
export class RemoteLogTransport {
  /**
   * Send logs to backend API in Loki format for storage/forwarding
   */
  static async send(level: string, message: string, context?: LogContext): Promise<void> {
    if (!config.logging.enabled) {
      return;
    }

    if (config.env.isDevelopment) {
      console.log('[Logger] Sending to API in Loki format:', { level, message, context });
    }

    try {
      // Filter PII from message and context before sending
      const filteredMessage = filterMessagePII(message);
      const filteredContext = filterPII(context);
      
      const timestamp = Date.now() * 1000000; // Loki expects nanoseconds
      
      // Build Loki labels
      const labels: LokiLabel = {
        app: config.app.name,
        environment: config.app.environment,
        level: level.toUpperCase(),
      };

      if (filteredContext?.component) {
        labels.component = String(filteredContext.component);
      }

      // Note: userId is intentionally excluded from labels to avoid PII in Loki labels
      // It's already filtered from context

      // Build log line with structured data (using filtered context)
      const logLine = {
        message: filteredMessage,
        timestamp: new Date().toISOString(),
        ...filteredContext,
      };

      // Loki streams format
      const lokiPayload: LokiPayload = {
        streams: [
          {
            stream: labels,
            values: [[String(timestamp), JSON.stringify(logLine)]],
          },
        ],
      };

      // Send to backend API endpoint in Loki format
      await axios.post(
        `${config.api.baseUrl}${config.logging.logsEndpoint}`,
        lokiPayload,
        {
          timeout: REMOTE_LOG_TIMEOUT_MS,
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );
      
      if (config.env.isDevelopment) {
        console.log('[Logger] Successfully sent to API');
      }
    } catch (error) {
      // Silently fail in production, show errors in development
      if (config.env.isDevelopment) {
        console.error('[Logger] Error sending to API:', error);
      }
    }
  }
}
