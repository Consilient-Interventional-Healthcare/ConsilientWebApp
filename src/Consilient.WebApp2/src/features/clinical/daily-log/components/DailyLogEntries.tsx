import { useEffect, useRef } from 'react';
import { cn } from '@/shared/utils/utils';

interface LogEntry {
  id: string;
  patientId: string;
  timestamp: Date;
  content: string;
  author: string;
}

interface DailyLogEntriesProps {
  entries: LogEntry[];
}

export function DailyLogEntries({ entries }: DailyLogEntriesProps) {
  const scrollRef = useRef<HTMLDivElement>(null);

  // Auto-scroll to bottom when new entries are added
  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [entries]);

  const formatTime = (date: Date) => {
    return new Intl.DateTimeFormat('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true,
    }).format(date);
  };

  const formatDate = (date: Date) => {
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    const isToday = date.toDateString() === today.toDateString();
    const isYesterday = date.toDateString() === yesterday.toDateString();

    if (isToday) return 'Today';
    if (isYesterday) return 'Yesterday';

    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: date.getFullYear() !== today.getFullYear() ? 'numeric' : undefined,
    }).format(date);
  };

  // Group entries by date
  const groupedEntries = entries.reduce((acc, entry) => {
    const dateKey = entry.timestamp.toDateString();
    acc[dateKey] ??= [];
    acc[dateKey].push(entry);
    return acc;
  }, {} as Record<string, LogEntry[]>);

  const sortedDates = Object.keys(groupedEntries).sort((a, b) => 
    new Date(a).getTime() - new Date(b).getTime()
  );

  return (
    <div 
      ref={scrollRef}
      className="flex-1 overflow-y-auto px-6 py-4 bg-gray-50"
    >
      {entries.length === 0 ? (
        <div className="flex items-center justify-center h-full">
          <div className="text-center text-gray-500">
            <svg
              className="mx-auto h-12 w-12 text-gray-400 mb-3"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
            <p className="text-sm font-medium">No log entries yet</p>
            <p className="text-xs text-gray-400 mt-1">Add your first entry below</p>
          </div>
        </div>
      ) : (
        <div className="space-y-6 max-w-4xl mx-auto">
          {sortedDates.map(dateKey => {
            const dateEntries = groupedEntries[dateKey];
            const date = new Date(dateKey);

            return (
              <div key={dateKey}>
                {/* Date Separator */}
                <div className="flex items-center gap-4 mb-4">
                  <div className="flex-1 h-px bg-gray-300" />
                  <span className="text-xs font-medium text-gray-500 uppercase tracking-wide">
                    {formatDate(date)}
                  </span>
                  <div className="flex-1 h-px bg-gray-300" />
                </div>

                {/* Entries for this date */}
                <div className="space-y-3">
                  {dateEntries?.map((entry, index) => {
                    const isFirstInGroup = index === 0 ||
                      dateEntries[index - 1]?.author !== entry.author;

                    return (
                      <div
                        key={entry.id}
                        className={cn(
                          'flex gap-3',
                          !isFirstInGroup && 'mt-1'
                        )}
                      >
                        {/* Avatar/Initial */}
                        {isFirstInGroup ? (
                          <div className="flex-shrink-0 w-8 h-8 rounded-full bg-blue-600 flex items-center justify-center text-white text-sm font-medium">
                            {entry.author.charAt(0).toUpperCase()}
                          </div>
                        ) : (
                          <div className="flex-shrink-0 w-8" />
                        )}

                        {/* Message Content */}
                        <div className="flex-1 min-w-0">
                          {isFirstInGroup && (
                            <div className="flex items-baseline gap-2 mb-1">
                              <span className="text-sm font-semibold text-gray-900">
                                {entry.author}
                              </span>
                              <span className="text-xs text-gray-500">
                                {formatTime(entry.timestamp)}
                              </span>
                            </div>
                          )}
                          
                          <div className="bg-white rounded-lg px-4 py-2.5 shadow-sm border border-gray-200">
                            <p className="text-sm text-gray-900 whitespace-pre-wrap break-words">
                              {entry.content}
                            </p>
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
