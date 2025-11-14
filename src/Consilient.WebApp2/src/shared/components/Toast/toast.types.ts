/**
 * Toast notification types and interfaces
 */

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
  duration?: number;
}

export interface ToastOptions {
  message: string;
  type: ToastType;
  duration?: number;
}
