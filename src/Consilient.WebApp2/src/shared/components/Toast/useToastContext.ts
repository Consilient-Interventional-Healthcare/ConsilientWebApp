import { useContext } from 'react';
import { ToastContext, type ToastContextValue } from './ToastContext';

/**
 * Hook to access toast context
 * Must be used within ToastProvider
 */
export function useToastContext(): ToastContextValue {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToastContext must be used within ToastProvider');
  }
  return context;
}
