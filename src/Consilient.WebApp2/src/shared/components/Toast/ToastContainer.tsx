import { useToastContext } from './useToastContext';
import { ToastItem } from './ToastItem';

/**
 * Toast Container Component
 * Renders all active toasts in a fixed position
 */
export function ToastContainer() {
  const { toasts, hideToast } = useToastContext();

  if (toasts.length === 0) {
    return null;
  }

  return (
    <div
      className="fixed top-4 right-4 z-50 w-full max-w-sm pointer-events-none"
      aria-live="polite"
      aria-atomic="true"
    >
      <div className="pointer-events-auto">
        {toasts.map((toast) => (
          <ToastItem key={toast.id} toast={toast} onClose={hideToast} />
        ))}
      </div>
    </div>
  );
}
