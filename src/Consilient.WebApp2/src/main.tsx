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
import './index.css';
import 'rsuite/Timeline/styles/index.css';

const rootElement = document.getElementById('root');
if (!rootElement) throw new Error('Root element not found');

createRoot(rootElement).render(
  <StrictMode>
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
  </StrictMode>,
);
