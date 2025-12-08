/**
 * Custom authentication events
 * These events allow components outside React context (like API interceptors)
 * to trigger navigation within the React Router context
 */

export interface SessionExpiredDetail {
  redirectPath: string;
}

// Extend the WindowEventMap to include our custom events
declare global {
  interface WindowEventMap {
    'auth:sessionExpired': CustomEvent<SessionExpiredDetail>;
  }
}

/**
 * Dispatch a session expired event
 * This should be called when a 401 response is received
 */
export function dispatchSessionExpired(redirectPath: string): void {
  window.dispatchEvent(
    new CustomEvent<SessionExpiredDetail>('auth:sessionExpired', {
      detail: { redirectPath },
    })
  );
}
