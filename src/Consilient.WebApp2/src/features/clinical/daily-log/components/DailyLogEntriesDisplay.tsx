import { useEffect, useRef } from 'react';
import { Timeline } from 'rsuite';
import { formatTime1 } from '@/shared/utils/utils';
import { logEntryTypes, type DailyLogLogEntry, type LogEntryType } from '../dailylog.types';
import { DynamicIcon } from '@/shared/components/DynamicIcon';

interface DailyLogEntriesDisplayProps {
  entries: DailyLogLogEntry[];
  typeFilter: string;
}

export function DailyLogEntriesDisplay({ entries, typeFilter }: DailyLogEntriesDisplayProps) {
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [entries]);

  // Sort entries by timestamp ascending
  const sortedEntries = [...entries].sort(
    (a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
  );

  // Helper to get icon and color for a log entry type
  const getTypeConfig = (type: string) : LogEntryType | undefined =>
    logEntryTypes.find(t => t.value === type);

  const filteredEntries =
    typeFilter === "all"
      ? sortedEntries
      : sortedEntries.filter((entry) => entry.type === typeFilter);

  return (
    <div ref={scrollRef} className="flex-1 overflow-y-auto px-4 py-4 w-full">
      <Timeline className="custom-timeline">
        {filteredEntries.map(entry => {
          const typeConfig = getTypeConfig(entry.type);
          return (
            <Timeline.Item
              key={entry.id}
              dot={
                typeConfig?.icon ? (
                    <DynamicIcon iconName={typeConfig.icon} className='rs-icon' style={{
                      background: typeConfig.color ?? '#e0e0e0',
                      color: '#fff' }} />
                ) : undefined
              }
            >
              <div>
                <span className="text-xs text-gray-500 mr-2">{formatTime1(entry.timestamp)}</span>
                <span className="text-xs text-gray-700 font-medium">
                  {entry.userFirstName} {entry.userLastName} reported
                </span>
              </div>
              <div className="mt-1">
                <span className="text-sm text-gray-900">{entry.message}</span>
              </div>
            </Timeline.Item>
          );
        })}
      </Timeline>
          <style>
      {`.custom-timeline {
        margin-left: 20px;

        .rs-timeline-item-content {
          padding-top:8px;
          margin-left: 8px !important;
        }
        .rs-timeline-item-custom-dot {
          .rs-icon {
            position: absolute;
            background: #fff;
            top: 0;
            left: -2px;
            /*border: 2px solid #ddd;*/
            width: 26px;
            height: 26px;
            border-radius: 50%;
            font-size: 18px;
            color: #999;
            margin-left: -7px;
            justify-content: center;
            padding: 5px;
          }
        }

        .rs-timeline-item-content {
          margin-left: 24px;
        }
      }
    `}
    </style>
    </div>
    
  );
}
