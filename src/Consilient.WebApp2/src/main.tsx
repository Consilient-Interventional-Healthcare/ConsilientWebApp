import { StrictMode, Suspense } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from "react-router-dom"
import { QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { AuthProvider } from '@/features/auth/services/AuthProvider'
import { ToastProvider, ToastContainer } from '@/shared/components/Toast'
import ErrorBoundary from '@/shared/components/ErrorBoundary'
import Loading from '@/shared/components/Loading'
import { router } from "@/shared/routes/Router"
import { queryClient } from '@/shared/core/query/QueryClient'
import { config } from '@/config'
import { logger } from '@/shared/core/logging/logger'
import './index.css'

const rootElement = document.getElementById('root');
if (!rootElement) throw new Error('Root element not found');

/**
 * Validate environment configuration before rendering the app
 * Shows user-friendly error if configuration is invalid
 */
function validateEnvironment(element: HTMLElement): void {
  try {
    // Additional runtime checks beyond what env.ts validates
    if (!config.api.baseUrl) {
      throw new Error('API Base URL is not configured');
    }
    
    if (!config.api.baseUrl.startsWith('http://') && !config.api.baseUrl.startsWith('https://')) {
      throw new Error('API Base URL must start with http:// or https://');
    }
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : 'Unknown configuration error';
    
    // Display user-friendly error in the DOM
    element.innerHTML = `
      <div style="display: flex; align-items: center; justify-content: center; min-height: 100vh; padding: 20px; background: #f9fafb; font-family: system-ui, -apple-system, sans-serif;">
        <div style="max-width: 600px; padding: 32px; background: white; border: 2px solid #ef4444; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);">
          <div style="display: flex; align-items: center; margin-bottom: 16px;">
            <svg style="width: 32px; height: 32px; color: #ef4444; margin-right: 12px;" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
            <h1 style="color: #dc2626; font-size: 24px; font-weight: 700; margin: 0;">Configuration Error</h1>
          </div>
          
          <p style="color: #991b1b; margin-bottom: 16px; line-height: 1.6;">
            ${errorMessage}
          </p>
          
          <div style="background: #fef2f2; border-left: 4px solid #ef4444; padding: 16px; margin-bottom: 16px; border-radius: 4px;">
            <p style="color: #7f1d1d; font-size: 14px; margin: 0; line-height: 1.6;">
              <strong>For Developers:</strong><br/>
              Please check your <code style="background: #fee2e2; padding: 2px 6px; border-radius: 3px; font-family: monospace;">.env</code> file and ensure all required environment variables are set.<br/>
              See <code style="background: #fee2e2; padding: 2px 6px; border-radius: 3px; font-family: monospace;">.env.example</code> for reference.
            </p>
          </div>
          
          <details style="cursor: pointer;">
            <summary style="color: #7f1d1d; font-size: 14px; font-weight: 600; user-select: none;">
              View Required Variables
            </summary>
            <ul style="color: #991b1b; font-size: 14px; margin-top: 12px; line-height: 1.8; font-family: monospace;">
              <li>VITE_API_BASE_URL (required)</li>
              <li>VITE_APP_ENV (optional, defaults to 'development')</li>
              <li>VITE_MSAL_CLIENT_ID (required for authentication)</li>
              <li>VITE_MSAL_TENANT_ID (required for authentication)</li>
              <li>VITE_MSAL_AUTHORITY (required for authentication)</li>
              <li>VITE_MSAL_REDIRECT_URI (required for authentication)</li>
            </ul>
          </details>
          
          <div style="margin-top: 24px; padding-top: 16px; border-top: 1px solid #fecaca;">
            <button onclick="location.reload()" style="background: #ef4444; color: white; padding: 10px 20px; border: none; border-radius: 6px; font-weight: 600; cursor: pointer; font-size: 14px;">
              Retry
            </button>
          </div>
        </div>
      </div>
    `;
    
    // Also throw to stop execution and show in console
    throw error;
  }
}

// Validate environment before rendering
validateEnvironment(rootElement);

/**
 * Global error handlers for uncaught errors and unhandled promise rejections
 * Ensures all errors are logged even if they escape error boundaries
 */
window.addEventListener('error', (event) => {
  const error = event.error instanceof Error ? event.error : new Error(event.message);
  
  logger.error('Uncaught error', error, {
    component: 'GlobalErrorHandler',
    filename: event.filename,
    lineno: event.lineno,
    colno: event.colno,
  });
  
  // Don't prevent default - let error boundaries handle UI
});

window.addEventListener('unhandledrejection', (event) => {
  logger.error('Unhandled promise rejection', 
    event.reason instanceof Error ? event.reason : new Error(String(event.reason)), 
    {
      component: 'GlobalErrorHandler',
      reason: event.reason,
    }
  );
  
  // Prevent default to avoid "Uncaught (in promise)" console errors
  // since we're already logging them
  event.preventDefault();
});

createRoot(rootElement).render(
  <StrictMode>
    <ErrorBoundary>
      <AuthProvider>
        <QueryClientProvider client={queryClient}>
          <ToastProvider>
            <ErrorBoundary fallbackMessage="An error occurred while loading the page.">
              <Suspense fallback={<Loading message="Loading application..." />}>
                <RouterProvider router={router} />
              </Suspense>
            </ErrorBoundary>
            <ToastContainer />
            <ReactQueryDevtools initialIsOpen={false} />
          </ToastProvider>
        </QueryClientProvider>
      </AuthProvider>
    </ErrorBoundary>
  </StrictMode>,
)
