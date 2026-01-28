import { useEffect, useRef } from 'react';
import { Timeline } from 'rsuite';
import { formatTime1 } from '@/shared/utils/utils';
import type { DailyLogLogEntryV2 } from '../dailylog.types';
import { DynamicIcon } from '@/shared/components/DynamicIcon';
interface DailyLogEntriesDisplayProps {
  entries: DailyLogLogEntryV2[];
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
    (a, b) => new Date(a.event.eventOccurredAt).getTime() - new Date(b.event.eventOccurredAt).getTime()
  );

  const filteredEntries =
    typeFilter === "all"
      ? sortedEntries
      : sortedEntries.filter((entry) => entry.event.eventTypeId === Number(typeFilter));

  return (
    <div ref={scrollRef} className="flex-1 overflow-y-auto px-4 py-4 w-full">
      <Timeline className="custom-timeline">
        {filteredEntries.map(entry => (
          <Timeline.Item
            key={entry.event.id}
            dot={
              entry.eventType?.iconName ? (
                  <DynamicIcon iconName={entry.eventType.iconName} className='rs-icon' style={{
                    background: entry.eventType.color ?? '#e0e0e0',
                    color: '#fff' }} />
              ) : undefined
            }
          >
            <div className='entry.userRole'>
              <span className="text-xs text-gray-500 mr-2">{formatTime1(entry.event.eventOccurredAt)}</span>
              <span className="text-xs text-gray-700 font-medium">
                {entry.user.firstName} {entry.user.lastName} reported
              </span>
            </div>
            <div className="mt-1">
              <span className="text-sm text-gray-900">{entry.event.description}</span>
            </div>
          </Timeline.Item>
        ))}
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
