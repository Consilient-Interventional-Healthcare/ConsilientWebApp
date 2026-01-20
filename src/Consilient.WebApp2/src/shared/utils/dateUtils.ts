/**
 * Date utility functions
 * Provides reusable date formatting, parsing, and comparison utilities
 */

/**
 * Format a Date object to YYYY-MM-DD string
 * @param date - Date to format
 * @returns Formatted date string in YYYY-MM-DD format
 * 
 * @example
 * ```typescript
 * formatDate(new Date('2025-11-14')) // '2025-11-14'
 * formatDate(new Date()) // Today's date as '2025-11-14'
 * ```
 */
export function formatDate(date: Date | string): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/**
 * Parse a YYYY-MM-DD string to Date object
 * Creates date at midnight in local timezone
 * 
 * @param dateString - Date string in YYYY-MM-DD format
 * @returns Date object
 * 
 * @example
 * ```typescript
 * parseDate('2025-11-14') // Date object for Nov 14, 2025 at 00:00:00
 * ```
 */
export function parseDate(dateString: string): Date {
  const parts = dateString.split('-').map(Number);
  const [year = 0, month = 1, day = 1] = parts;
  return new Date(year, month - 1, day);
}

/**
 * Check if a date is today
 * Compares only the date portion, ignoring time
 * 
 * @param date - Date to check (Date object or YYYY-MM-DD string)
 * @returns true if the date is today
 * 
 * @example
 * ```typescript
 * isToday(new Date()) // true
 * isToday('2025-11-14') // true if today is Nov 14, 2025
 * isToday('2025-11-13') // false if today is Nov 14, 2025
 * ```
 */
export function isToday(date: Date | string): boolean {
  const d = typeof date === 'string' ? parseDate(date) : date;
  const today = new Date();
  return formatDate(d) === formatDate(today);
}

/**
 * Check if a date is in the past
 * Compares only the date portion, ignoring time
 * 
 * @param date - Date to check (Date object or YYYY-MM-DD string)
 * @returns true if the date is before today
 * 
 * @example
 * ```typescript
 * isPast('2025-11-13') // true if today is Nov 14, 2025
 * isPast('2025-11-14') // false if today is Nov 14, 2025
 * ```
 */
export function isPast(date: Date | string): boolean {
  const d = typeof date === 'string' ? parseDate(date) : date;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  d.setHours(0, 0, 0, 0);
  return d < today;
}

/**
 * Check if a date is in the future
 * Compares only the date portion, ignoring time
 * 
 * @param date - Date to check (Date object or YYYY-MM-DD string)
 * @returns true if the date is after today
 * 
 * @example
 * ```typescript
 * isFuture('2025-11-15') // true if today is Nov 14, 2025
 * isFuture('2025-11-14') // false if today is Nov 14, 2025
 * ```
 */
export function isFuture(date: Date | string): boolean {
  const d = typeof date === 'string' ? parseDate(date) : date;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  d.setHours(0, 0, 0, 0);
  return d > today;
}

/**
 * Add days to a date
 * 
 * @param date - Starting date
 * @param days - Number of days to add (can be negative to subtract)
 * @returns New Date object
 * 
 * @example
 * ```typescript
 * addDays(new Date('2025-11-14'), 1) // Nov 15, 2025
 * addDays(new Date('2025-11-14'), -1) // Nov 13, 2025
 * addDays(new Date('2025-11-14'), 7) // Nov 21, 2025
 * ```
 */
export function addDays(date: Date | string, days: number): Date {
  const d = typeof date === 'string' ? parseDate(date) : new Date(date);
  d.setDate(d.getDate() + days);
  return d;
}

/**
 * Get the difference in days between two dates
 * 
 * @param date1 - First date
 * @param date2 - Second date
 * @returns Number of days between dates (positive if date1 is after date2)
 * 
 * @example
 * ```typescript
 * diffInDays(new Date('2025-11-15'), new Date('2025-11-14')) // 1
 * diffInDays(new Date('2025-11-14'), new Date('2025-11-15')) // -1
 * ```
 */
export function diffInDays(date1: Date | string, date2: Date | string): number {
  const d1 = typeof date1 === 'string' ? parseDate(date1) : new Date(date1);
  const d2 = typeof date2 === 'string' ? parseDate(date2) : new Date(date2);
  
  d1.setHours(0, 0, 0, 0);
  d2.setHours(0, 0, 0, 0);
  
  const diffMs = d1.getTime() - d2.getTime();
  return Math.floor(diffMs / (1000 * 60 * 60 * 24));
}

/**
 * Get the start of the week (Monday) for a given date
 * 
 * @param date - Date to get week start for
 * @returns Date object representing Monday of that week
 * 
 * @example
 * ```typescript
 * startOfWeek(new Date('2025-11-14')) // Monday of that week
 * ```
 */
export function startOfWeek(date: Date | string): Date {
  const d = typeof date === 'string' ? parseDate(date) : new Date(date);
  const day = d.getDay();
  const diff = (day === 0 ? -6 : 1) - day; // Adjust when day is Sunday
  return addDays(d, diff);
}

/**
 * Get the end of the week (Sunday) for a given date
 * 
 * @param date - Date to get week end for
 * @returns Date object representing Sunday of that week
 * 
 * @example
 * ```typescript
 * endOfWeek(new Date('2025-11-14')) // Sunday of that week
 * ```
 */
export function endOfWeek(date: Date | string): Date {
  const d = typeof date === 'string' ? parseDate(date) : new Date(date);
  const day = d.getDay();
  const diff = (day === 0 ? 0 : 7) - day;
  return addDays(d, diff);
}

/**
 * Check if two dates are the same day
 * 
 * @param date1 - First date
 * @param date2 - Second date
 * @returns true if dates represent the same day
 * 
 * @example
 * ```typescript
 * isSameDay('2025-11-14', new Date('2025-11-14')) // true
 * isSameDay('2025-11-14', '2025-11-15') // false
 * ```
 */
export function isSameDay(date1: Date | string, date2: Date | string): boolean {
  return formatDate(date1) === formatDate(date2);
}

/**
 * Format a date to a human-readable string
 * 
 * @param date - Date to format
 * @param options - Intl.DateTimeFormat options
 * @returns Formatted date string
 * 
 * @example
 * ```typescript
 * formatDateLong(new Date('2025-11-14')) // 'November 14, 2025'
 * formatDateLong('2025-11-14', { month: 'short' }) // 'Nov 14, 2025'
 * ```
 */
export function formatDateLong(
  date: Date | string,
  options: Intl.DateTimeFormatOptions = { year: 'numeric', month: 'long', day: 'numeric' }
): string {
  const d = typeof date === 'string' ? parseDate(date) : date;
  return new Intl.DateTimeFormat('en-US', options).format(d);
}

/**
 * Get today's date as YYYY-MM-DD string
 * Convenience function for getting current date
 * 
 * @returns Today's date in YYYY-MM-DD format
 * 
 * @example
 * ```typescript
 * getToday() // '2025-11-14' if today is Nov 14, 2025
 * ```
 */
export function getToday(): string {
  return formatDate(new Date());
}

/**
 * Convert a URL date format (YYYYMMDD) to ISO format (YYYY-MM-DD)
 *
 * @param urlDate - Date string in YYYYMMDD format
 * @returns Date string in YYYY-MM-DD format
 *
 * @example
 * ```typescript
 * formatDateFromUrl('20251114') // '2025-11-14'
 * ```
 */
export function formatDateFromUrl(urlDate: string): string {
  return `${urlDate.slice(0, 4)}-${urlDate.slice(4, 6)}-${urlDate.slice(6, 8)}`;
}

/**
 * Convert an ISO date format (YYYY-MM-DD) to URL format (YYYYMMDD)
 *
 * @param isoDate - Date string in YYYY-MM-DD format
 * @returns Date string in YYYYMMDD format
 *
 * @example
 * ```typescript
 * formatDateToUrl('2025-11-14') // '20251114'
 * ```
 */
export function formatDateToUrl(isoDate: string): string {
  return isoDate.replace(/-/g, '');
}

/**
 * Get today's date in URL format (YYYYMMDD)
 *
 * @returns Today's date in YYYYMMDD format
 *
 * @example
 * ```typescript
 * getTodayYYYYMMDD() // '20251114' if today is Nov 14, 2025
 * ```
 */
export function getTodayYYYYMMDD(): string {
  return getToday().replace(/-/g, '');
}
