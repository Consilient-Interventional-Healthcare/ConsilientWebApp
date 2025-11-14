// @vitest-environment jsdom
import { describe, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ToastProvider } from '../src/shared/components/Toast/ToastProvider';
import { useToastContext } from '../src/shared/components/Toast/useToastContext';
import React from 'react';

describe('ToastProvider', () => {
  it('provides toast context', () => {
    function TestComponent() {
      const { showToast, toasts } = useToastContext();
      React.useEffect(() => {
        showToast({ message: 'Hello!', type: 'success' });
      }, [showToast]);
      return <div>{toasts.length > 0 ? toasts[0].message : 'No toast'}</div>;
    }
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );
    expect(screen.getByText('Hello!')).toBeInTheDocument();
  });
});
