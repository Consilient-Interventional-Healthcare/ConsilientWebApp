import { createContext } from 'react';
import type { Toast, ToastOptions } from './toast.types';

/**
 * Toast Context
 * Provides toast notification state and methods throughout the app
 */
export interface ToastContextValue {
  toasts: Toast[];
  showToast: (options: ToastOptions) => void;
  hideToast: (id: string) => void;
}

export const ToastContext = createContext<ToastContextValue | undefined>(undefined);
