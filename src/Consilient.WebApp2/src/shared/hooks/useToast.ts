import { useToastContext } from '@/shared/components/Toast/useToastContext';
import type { ToastOptions } from '@/shared/components/Toast/toast.types';

interface UseToastReturn {
  showToast: (options: ToastOptions) => void;
  success: (message: string, duration?: number) => void;
  error: (message: string, duration?: number) => void;
  info: (message: string, duration?: number) => void;
  warning: (message: string, duration?: number) => void;
}

/**
 * Hook to show toast notifications
 * Provides a simple API for displaying toast messages
 * 
 * @example
 * ```tsx
 * const { showToast } = useToast();
 * 
 * // Success message
 * showToast({ message: 'Saved successfully!', type: 'success' });
 * 
 * // Error message
 * showToast({ message: 'Failed to save', type: 'error' });
 * 
 * // Custom duration
 * showToast({ message: 'Processing...', type: 'info', duration: 5000 });
 * ```
 */
export function useToast(): UseToastReturn {
  const { showToast } = useToastContext();

  return {
    showToast,
    success: (message: string, duration?: number) => 
      showToast({ message, type: 'success', ...(duration !== undefined && { duration }) }),
    error: (message: string, duration?: number) => 
      showToast({ message, type: 'error', ...(duration !== undefined && { duration }) }),
    info: (message: string, duration?: number) => 
      showToast({ message, type: 'info', ...(duration !== undefined && { duration }) }),
    warning: (message: string, duration?: number) => 
      showToast({ message, type: 'warning', ...(duration !== undefined && { duration }) }),
  };
}
