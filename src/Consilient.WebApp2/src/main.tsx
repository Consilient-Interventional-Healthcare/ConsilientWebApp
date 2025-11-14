import { StrictMode, Suspense } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from "react-router-dom"
import { QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { AuthProvider } from '@/services/auth/AuthProvider'
import ErrorBoundary from '@/components/common/ErrorBoundary'
import Loading from '@/components/common/Loading'
import { router } from "@/routes/Router"
import { queryClient } from '@/services/query/QueryClient'
import './index.css'

const rootElement = document.getElementById('root');
if (!rootElement) throw new Error('Root element not found');

createRoot(rootElement).render(
  <StrictMode>
    <ErrorBoundary>
      <AuthProvider>
        <QueryClientProvider client={queryClient}>
          <Suspense fallback={<Loading message="Loading application..." />}>
            <RouterProvider router={router} />
          </Suspense>
          <ReactQueryDevtools initialIsOpen={false} />
        </QueryClientProvider>
      </AuthProvider>
    </ErrorBoundary>
  </StrictMode>,
)
