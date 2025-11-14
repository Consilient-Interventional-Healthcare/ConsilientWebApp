import { QueryClient } from '@tanstack/react-query';
import { QUERY_STALE_TIME, QUERY_GC_TIME } from '@/constants';
import config from '@/config';

export const queryClient: QueryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: QUERY_STALE_TIME,
      gcTime: QUERY_GC_TIME, // Renamed from cacheTime in React Query v5
      refetchOnWindowFocus: false,
      refetchOnReconnect: true,
      retry: config.api.retryAttempts,
    },
  },
});