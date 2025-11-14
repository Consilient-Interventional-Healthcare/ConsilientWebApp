import { QueryClient as TanStackQueryClient } from '@tanstack/react-query';
import { QUERY_STALE_TIME, QUERY_GC_TIME } from '@/constants';
import config from '@/config';
import { QueryClientManager } from './QueryClient';

/**
 * Factory class for creating and configuring QueryClientManager instances
 * 
 * Responsibilities:
 * - Creates TanStack QueryClient instances with proper configuration
 * - Configures default query options
 * - Provides a centralized place for query client instantiation
 */
export class QueryClientFactory {
  /**
   * Create a fully configured QueryClientManager instance
   * 
   * @returns Configured QueryClientManager instance
   */
  static create(): QueryClientManager {
    const tanstackClient = this.createTanStackQueryClient();
    return new QueryClientManager(tanstackClient);
  }

  /**
   * Create and configure a TanStack QueryClient instance
   */
  private static createTanStackQueryClient(): TanStackQueryClient {
    return new TanStackQueryClient({
      defaultOptions: {
        queries: {
          staleTime: QUERY_STALE_TIME,
          gcTime: QUERY_GC_TIME, // Renamed from cacheTime in React Query v5
          refetchOnWindowFocus: false,
          refetchOnReconnect: true,
          retry: config.api.retryAttempts,
          // Stale-while-revalidate pattern: show cached data immediately, refetch in background
          refetchOnMount: 'always',
          // Only attempt fetches when online to avoid unnecessary failed requests
          networkMode: 'online',
        },
      },
    });
  }
}
