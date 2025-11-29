import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"
import { isValid, differenceInDays, parseISO } from "date-fns";

/**
 * Utility function for merging Tailwind CSS classes
 * Combines clsx and tailwind-merge for conflict-free class merging
 */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs))
}

export function calculateAge(dateOfBirth: string): number | null {
  const ageInDays = diffInDays(dateOfBirth);
  if (ageInDays === null) {
    return null;
  }
  return Math.floor(ageInDays / 365.25);
}

export function diffInDays(date: string): number | null {
  const dateObj = parseISO(date);
  if (!isValid(dateObj)) {
    return null;
  }
  return differenceInDays(new Date(), dateObj);
}


  export const formatTime1 = (date: string) => {
    return new Intl.DateTimeFormat('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true,
    }).format(new Date(date));
  };

  export const formatDate1 = (date: string) => {
    const dateObj = parseISO(date);
    if (!isValid(dateObj)) {
      return date;
    }
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    const isToday = dateObj.toDateString() === today.toDateString();
    const isYesterday = dateObj.toDateString() === yesterday.toDateString();

    if (isToday) return 'Today';
    if (isYesterday) return 'Yesterday';

    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: dateObj.getFullYear() !== today.getFullYear() ? 'numeric' : undefined,
    }).format(dateObj);
  };

// Re-export date utilities for convenience
export * from './dateUtils';
