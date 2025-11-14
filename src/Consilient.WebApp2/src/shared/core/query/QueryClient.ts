import type { QueryClient as TanStackQueryClient } from '@tanstack/react-query';

// ============================================================================
// Query Client Manager Class
// ============================================================================

/**
 * Query Client Manager
 * Manages TanStack Query client configuration and instance
 * 
 * Features:
 * - Centralized query configuration
 * - Cache management
 * - Query invalidation
 * - Data access and mutation
 */
export class QueryClientManager {
  private readonly client: TanStackQueryClient;

  constructor(tanstackClient: TanStackQueryClient) {
    this.client = tanstackClient;
  }

  // ============================================================================
  // Instance Access
  // ============================================================================

  /**
   * Get the TanStack Query client instance
   */
  getInstance(): TanStackQueryClient {
    return this.client;
  }

  // ============================================================================
  // Cache Management
  // ============================================================================

  /**
   * Clear all queries from the cache
   */
  clear(): void {
    this.client.clear();
  }

  /**
   * Invalidate all queries
   */
  async invalidateAll(): Promise<void> {
    await this.client.invalidateQueries();
  }

  /**
   * Invalidate queries matching a specific query key
   */
  async invalidateQueries(queryKey: unknown[]): Promise<void> {
    await this.client.invalidateQueries({ queryKey });
  }

  /**
   * Remove queries matching a specific query key
   */
  removeQueries(queryKey: unknown[]): void {
    this.client.removeQueries({ queryKey });
  }

  // ============================================================================
  // Data Access & Mutation
  // ============================================================================

  /**
   * Get query data from cache
   */
  getQueryData<T = unknown>(queryKey: unknown[]): T | undefined {
    return this.client.getQueryData<T>(queryKey);
  }

  /**
   * Set query data in cache
   */
  setQueryData<T = unknown>(queryKey: unknown[], data: T | ((oldData: T | undefined) => T)): void {
    this.client.setQueryData<T>(queryKey, data);
  }

  /**
   * Check if a query exists in cache
   */
  hasQueryData(queryKey: unknown[]): boolean {
    return this.client.getQueryData(queryKey) !== undefined;
  }

  /**
   * Prefetch a query
   */
  async prefetchQuery<T = unknown>(
    queryKey: unknown[],
    queryFn: () => Promise<T>
  ): Promise<void> {
    await this.client.prefetchQuery({
      queryKey,
      queryFn,
    });
  }
}

// ============================================================================
// Singleton Export
// ============================================================================

// Export singleton instance created via factory
import { QueryClientFactory } from './QueryClientFactory';

const queryClientManager = QueryClientFactory.create();

// Export the QueryClient instance for use with QueryClientProvider
export const queryClient = queryClientManager.getInstance();

// Export the manager for advanced operations
export default queryClientManager;
