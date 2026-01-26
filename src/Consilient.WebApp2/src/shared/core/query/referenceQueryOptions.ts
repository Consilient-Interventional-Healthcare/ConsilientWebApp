/**
 * Query options for reference/lookup data that rarely changes.
 * Uses infinite staleTime to cache data until manually invalidated.
 */
export const referenceQueryOptions = {
  staleTime: Infinity,
  gcTime: 30 * 60 * 1000, // 30 min garbage collection
  refetchOnMount: false,
  refetchOnWindowFocus: false,
  refetchOnReconnect: false,
};
