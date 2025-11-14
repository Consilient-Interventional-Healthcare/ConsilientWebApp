import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

/**
 * Utility function for merging Tailwind CSS classes
 * Combines clsx and tailwind-merge for conflict-free class merging
 */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs))
}

// Re-export date utilities for convenience
export * from './dateUtils';
