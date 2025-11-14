import { useMemo } from 'react';

/**
 * Hook to get today's date in YYYY-MM-DD format
 * @returns {string} Current date as YYYY-MM-DD string
 */
export function useTodayDate(): string {
  return useMemo(() => {
    const dateStr = new Date().toISOString().split('T')[0];
    return dateStr!;
  }, []);
}