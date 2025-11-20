import { describe, it, expect } from 'vitest';
import { formatDate, parseDate, isToday, addDays, diffInDays, isSameDay, getToday } from '../src/shared/utils/dateUtils';

describe('dateUtils', () => {
  it('formatDate returns YYYY-MM-DD', () => {
    expect(formatDate('2025-11-14')).toBe('2025-11-14');
  });

  it('parseDate returns Date object', () => {
    const d = parseDate('2025-11-14');
    expect(d).toBeInstanceOf(Date);
    expect(d.getFullYear()).toBe(2025);
    expect(d.getMonth()).toBe(10); // November is month 10 (0-based)
    expect(d.getDate()).toBe(14);
  });

  it('isToday works for today', () => {
    expect(isToday(getToday())).toBe(true);
  });

  it('addDays adds days correctly', () => {
    expect(formatDate(addDays('2025-11-14', 1))).toBe('2025-11-15');
    expect(formatDate(addDays('2025-11-14', -1))).toBe('2025-11-13');
  });

  it('diffInDays returns correct difference', () => {
    expect(diffInDays('2025-11-15', '2025-11-14')).toBe(1);
    expect(diffInDays('2025-11-14', '2025-11-15')).toBe(-1);
  });

  it('isSameDay returns true for same day', () => {
    expect(isSameDay('2025-11-14', parseDate('2025-11-14'))).toBe(true);
    expect(isSameDay('2025-11-14', '2025-11-15')).toBe(false);
  });
});
