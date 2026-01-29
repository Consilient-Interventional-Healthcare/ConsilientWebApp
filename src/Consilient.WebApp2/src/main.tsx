import { StrictMode, Suspense } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from "react-router-dom"
import { QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { ToastProvider, ToastContainer } from '@/shared/components/Toast'
import { ApiErrorToastListener } from '@/shared/components/ApiErrorToastListener'
import ErrorBoundary from '@/shared/components/ErrorBoundary'
import { router } from "@/shared/routes/Router"
import { queryClient } from '@/shared/core/query/QueryClient'
import { loadEnumVisualsConfig } from '@/shared/config/enumVisualsConfig'
import './index.css';
import 'rsuite/Timeline/styles/index.css';

const rootElement = document.getElementById('root');
if (!rootElement) throw new Error('Root element not found');

// Load enum visuals config before rendering the app
loadEnumVisualsConfig().then(() => {
  createRoot(rootElement).render(
    <StrictMode>
      <QueryClientProvider client={queryClient}>
        <ToastProvider>
          <ErrorBoundary fallbackMessage="An error occurred while loading the page.">
            <Suspense>
              <RouterProvider router={router} />
            </Suspense>
          </ErrorBoundary>
          <ToastContainer />
          <ApiErrorToastListener />
          <ReactQueryDevtools initialIsOpen={false} />
        </ToastProvider>
      </QueryClientProvider>
    </StrictMode>,
  );
});
