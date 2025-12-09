import { useEffect, useRef } from 'react';
import { Timeline } from 'rsuite';
import { formatTime1 } from '@/shared/utils/utils';
import type { DailyLogLogEntry } from '../dailylog.types';
import { DynamicIcon } from '@/shared/components/DynamicIcon';
import { dataProvider } from '@/data/DataProvider';
import type { VisitEvents } from '@/types/api.generated';
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
    (a, b) => new Date(a.eventOccurredAt).getTime() - new Date(b.eventOccurredAt).getTime()
  );

  // Helper to get icon and color for a log entry type
  const getEventType = (eventTypeId: number) : VisitEvents.VisitEventTypeDto | undefined =>
    dataProvider.getTable<VisitEvents.VisitEventTypeDto>('visitEventTypes').find(t => t.id === eventTypeId);

  const filteredEntries =
    typeFilter === "all"
      ? sortedEntries
      : sortedEntries.filter((entry) => entry.eventTypeId === Number(typeFilter));

  return (
    <div ref={scrollRef} className="flex-1 overflow-y-auto px-4 py-4 w-full">
      <Timeline className="custom-timeline">
        {filteredEntries.map(entry => {
          const eventType = getEventType(entry.eventTypeId);
          return (
            <Timeline.Item
              key={entry.id}
              dot={
                eventType?.iconName ? (
                    <DynamicIcon iconName={eventType.iconName} className='rs-icon' style={{
                      background: eventType.color ?? '#e0e0e0',
                      color: '#fff' }} />
                ) : undefined
              }
            >
              <div className='entry.userRole'>
                <span className="text-xs text-gray-500 mr-2">{formatTime1(entry.eventOccurredAt)}</span>
                <span className="text-xs text-gray-700 font-medium">
                  {entry.userFirstName} {entry.userLastName} reported
                </span>
              </div>
              <div className="mt-1">
                <span className="text-sm text-gray-900">{entry.description}</span>
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
