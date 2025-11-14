import { useState, useCallback, type ReactNode } from 'react';
import { ToastContext } from './ToastContext';
import type { Toast, ToastOptions } from './toast.types';

/**
 * Toast Provider Component
 * Manages toast notifications state and provides context to children
 */
export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const hideToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id));
  }, []);

  const showToast = useCallback((options: ToastOptions) => {
    const id = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    const duration = options.duration ?? 3000; // Default 3 seconds

    const newToast: Toast = {
      id,
      message: options.message,
      type: options.type,
      duration,
    };

    setToasts((prev) => [...prev, newToast]);

    // Auto-dismiss after duration
    if (duration > 0) {
      setTimeout(() => {
        hideToast(id);
      }, duration);
    }
  }, [hideToast]);

  return (
    <ToastContext.Provider value={{ toasts, showToast, hideToast }}>
      {children}
    </ToastContext.Provider>
  );
}
