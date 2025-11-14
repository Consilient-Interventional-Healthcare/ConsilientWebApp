import type { LogContext } from './logging.types';

/**
 * PII (Personally Identifiable Information) filtering utilities
 */

/**
 * PII patterns to detect and filter
 */
const PII_PATTERNS = {
  email: /\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b/g,
  // Sensitive key names to redact
  sensitiveKeys: /accountId|userId|homeAccountId|token|authorization|bearer|password|secret|apiKey|api_key/i,
};

/**
 * Filter PII from context before sending to remote
 * @param context - Log context that may contain PII
 * @returns Filtered context with PII redacted
 */
export function filterPII(context?: LogContext): LogContext | undefined {
  if (!context) return context;
  
  const filtered: LogContext = {};
  
  for (const [key, value] of Object.entries(context)) {
    // Skip sensitive keys entirely and replace with [REDACTED]
    if (PII_PATTERNS.sensitiveKeys.test(key)) {
      filtered[key] = '[REDACTED]';
      continue;
    }
    
    // Filter string values for email patterns
    if (typeof value === 'string') {
      filtered[key] = value.replace(PII_PATTERNS.email, '[EMAIL_REDACTED]');
    } else if (value && typeof value === 'object' && !Array.isArray(value)) {
      // Recursively filter nested objects
      filtered[key] = filterPII(value as LogContext);
    } else {
      filtered[key] = value;
    }
  }
  
  return filtered;
}

/**
 * Filter PII from message string
 * @param message - Log message that may contain PII
 * @returns Message with PII redacted
 */
export function filterMessagePII(message: string): string {
  return message.replace(PII_PATTERNS.email, '[EMAIL_REDACTED]');
}
