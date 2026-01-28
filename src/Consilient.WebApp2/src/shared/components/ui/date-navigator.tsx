import { ChevronLeft, ChevronRight } from 'lucide-react';
import { addDays, formatDate, getToday } from '@/shared/utils/dateUtils';
import { cn } from '@/shared/utils/utils';

interface DateNavigatorProps {
  /** Current date value in ISO format (YYYY-MM-DD) */
  value: string;
  /** Callback when date changes */
  onChange: (date: string) => void;
  /** Minimum selectable date in ISO format */
  min?: string;
  /** Maximum selectable date in ISO format (defaults to today) */
  max?: string;
  /** ID for the date input element (for label association) */
  id?: string;
  /** Additional CSS classes */
  className?: string;
}

export function DateNavigator({
  value,
  onChange,
  min,
  max = getToday(),
  id,
  className,
}: DateNavigatorProps) {
  const handlePrevious = () => {
    const newDate = addDays(value, -1);
    onChange(formatDate(newDate));
  };

  const handleNext = () => {
    const newDate = addDays(value, 1);
    onChange(formatDate(newDate));
  };

  const isPrevDisabled = min ? value <= min : false;
  const isNextDisabled = value >= max;

  return (
    <div className={cn('flex', className)}>
      <button
        type="button"
        className="h-9 w-9 inline-flex items-center justify-center rounded-l-md border border-r-0 border-gray-300 bg-white text-sm hover:bg-gray-50 focus:outline-none focus:z-10 focus:ring-2 focus:ring-blue-500 disabled:text-gray-300 disabled:hover:bg-white disabled:pointer-events-none"
        disabled={isPrevDisabled}
        onClick={handlePrevious}
        aria-label="Previous day"
      >
        <ChevronLeft className="h-4 w-4" />
      </button>

      <input
        type="date"
        id={id}
        value={value}
        min={min}
        max={max}
        onChange={(e) => {
          const selected = e.target.value;
          const withinMin = !min || selected >= min;
          const withinMax = selected <= max;
          if (withinMin && withinMax) {
            onChange(selected);
          }
        }}
        className="h-9 px-3 flex-1 min-w-0 border-y border-gray-300 text-sm bg-white focus:outline-none focus:z-10 focus:ring-2 focus:ring-blue-500"
      />

      <button
        type="button"
        className="h-9 w-9 inline-flex items-center justify-center rounded-r-md border border-l-0 border-gray-300 bg-white text-sm hover:bg-gray-50 focus:outline-none focus:z-10 focus:ring-2 focus:ring-blue-500 disabled:text-gray-300 disabled:hover:bg-white disabled:pointer-events-none"
        disabled={isNextDisabled}
        onClick={handleNext}
        aria-label="Next day"
      >
        <ChevronRight className="h-4 w-4" />
      </button>
    </div>
  );
}
