import { useEffect } from 'react';
import { useToast } from '@/shared/hooks/useToast';
import type { ApiErrorEventDetail } from '@/shared/core/api/api.events';

/**
 * Component that listens for API error events and displays toast notifications
 * Should be rendered once, typically near the root of the application
 */
export function ApiErrorToastListener() {
  const { error: showError, warning: showWarning } = useToast();

  useEffect(() => {
    const handleApiError = (event: CustomEvent<ApiErrorEventDetail>) => {
      const { type, status } = event.detail;

      switch (type) {
        case 'network':
          showError('Unable to connect to the server. Please check your connection.', 5000);
          break;
        case 'server': {
          const displayFn = status === 503 ? showWarning : showError;
          displayFn('The server encountered an error. Please try again later.', 5000);
          break;
        }
        case 'timeout':
          showWarning('The request timed out. Please try again.', 5000);
          break;
      }
    };

    window.addEventListener('api:error', handleApiError);

    return () => {
      window.removeEventListener('api:error', handleApiError);
    };
  }, [showError, showWarning]);

  return null;
}
