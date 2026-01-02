import { useMemo, useState, useEffect } from 'react';
import { formatDate } from '@/shared/utils/dateUtils';

/**
 * Hook to get today's date in YYYY-MM-DD format
 * Memoized to prevent unnecessary recalculations on re-renders
 * 
 * Note: Date is captured at component mount and won't update if app stays open past midnight.
 * For a date that automatically updates at midnight, use useReactiveTodayDate() instead.
 * 
 * @returns Current date as YYYY-MM-DD string
 * 
 * @example
 * ```tsx
 * function MyComponent() {
 *   const today = useTodayDate();
 *   return <div>Today is {today}</div>;
 * }
 * ```
 */
export function useTodayDate(): string {
  return useMemo(() => formatDate(new Date()), []);
}

/**
 * Hook to get today's date that automatically updates at midnight
 * Uses setTimeout to schedule update at exactly midnight local time
 * 
 * Useful for long-running applications like dashboards that may stay open 24/7.
 * The date will automatically update when the clock strikes midnight.
 * 
 * @returns Current date as YYYY-MM-DD string that updates at midnight
 * 
 * @example
 * ```tsx
 * function Dashboard() {
 *   const today = useReactiveTodayDate(); // Updates automatically at midnight
 *   return <div>Dashboard for {today}</div>;
 * }
 * ```
 */
export function useReactiveTodayDate(): string {
  const [date, setDate] = useState(() => formatDate(new Date()));

  useEffect(() => {
    // Calculate time until midnight
    const now = new Date();
    const tomorrow = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1);
    const timeUntilMidnight = tomorrow.getTime() - now.getTime();

    // Set timeout to update at midnight
    const timeout = setTimeout(() => {
      setDate(formatDate(new Date()));
    }, timeUntilMidnight);

    return () => clearTimeout(timeout);
  }, [date]); // Re-run when date changes to schedule next midnight update

  return date;
}