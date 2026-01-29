import React, { useMemo } from 'react';
import { DynamicIcon } from '@/shared/components/DynamicIcon';
import type { StatusChangeEvent } from '../services/IDailyLogService';

interface PatientTimelineProps {
  statusChanges: StatusChangeEvent[];
}

interface ProcessedStatusChangeEvent extends StatusChangeEvent {
  dateObj: Date;
  left: number;
  duration: number;
  width: number;
}

const PatientTimeline: React.FC<PatientTimelineProps> = (props) => {

  // 2. Process Data for Timeline
  const processedData = useMemo(() => {
    const safeStatusChanges = props.statusChanges ?? [];
    if (!safeStatusChanges.length) {
        return { periods: [], events: [], minDate: new Date(), maxDate: new Date() };
    }

    safeStatusChanges.sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());

    // Calculate Strict Boundaries (No Buffers)
    const allDates = [...safeStatusChanges.map(e => new Date(e.date))];
    const minDate = new Date(Math.min(...allDates.map(d => d.getTime())));
    const maxDate = new Date(Math.max(...allDates.map(d => d.getTime())));
    
    // Handle single-day case to prevent division by zero
    if (maxDate.getTime() === minDate.getTime()) {
      maxDate.setDate(maxDate.getDate() + 1);
    }

    const totalDuration = maxDate.getTime() - minDate.getTime();

    const getPos = (date: Date): number => {
      const pos = ((date.getTime() - minDate.getTime()) / totalDuration) * 100;
      return Math.max(0, Math.min(100, pos));
    };

    const procStatusChanges : ProcessedStatusChangeEvent[] = safeStatusChanges.map((e, index) => {
      const dateObj = new Date(e.date);
      const startPos = getPos(dateObj);

      let endPos: number;
      let endDate: Date;
      const nextChange = safeStatusChanges[index + 1];

      if (nextChange) { // Only proceed if nextChange is not undefined
        endDate = new Date(nextChange.date);
        endPos = getPos(endDate);
      } else {
        endDate = maxDate;
        endPos = 100;
      }

      const diffTime = Math.abs(endDate.getTime() - dateObj.getTime());
      const days = Math.round(diffTime / (1000 * 60 * 60 * 24)); 

      return {
        ...e,
        dateObj,
        left: startPos,
        width: Math.max(endPos - startPos, 0),
        duration: days
      };
    });

    procStatusChanges.sort((a, b) => a.dateObj.getTime() - b.dateObj.getTime());

    return { 
      states: procStatusChanges.filter(e => e.type === 'state'), 
      events: procStatusChanges.filter(e => e.type === 'event'),
      minDate, 
      maxDate 
    };
  }, [props]);

  const formatDate = (dateObj: Date): string => {
    return dateObj.toLocaleDateString('en-US', { month: 'numeric', day: 'numeric' });
  };

  return (
    // Clean wrapper. overflow-hidden prevents scrollbars.
    <div className="w-full font-sans overflow-visible pb-8">
      
      {/* Main Timeline Container */}
      <div className="relative h-16 select-none group mx-4">
        
        {/* 1. The Base Track (Periods) */}
        <div className="absolute top-1/2 left-0 w-full h-8 bg-gray-100 -translate-y-1/2 overflow-hidden shadow-inner">
          {processedData.states?.map((state, idx) => (
            <div
              key={`state-${idx}`}
              className={`absolute top-0 bottom-0 border-r border-white/20 last:border-0 transition-all hover:opacity-90 cursor-pointer flex flex-col items-center justify-center overflow-hidden`}
              style={{ 
                left: `${state.left}%`, 
                width: `${state.width}%`,
                background: state.color ? state.color : undefined // Use color only for background
              }}
              title={`${state.name}: ${state.duration} days`}
            >
              {state.width > 5 && (
                <div className="flex flex-col items-center">
                  <span className="text-xs font-bold opacity-70 whitespace-nowrap px-1 select-none">
                    {state.duration}d
                  </span>
                </div>
              )}
            </div>
          ))}
        </div>

        {/* 1b. Period Names Under Track (centered, full width, same style as date, with ellipsis) */}
        {processedData.states?.map((state, idx) => (
          <div
            key={`period-name-${idx}`}
            className="absolute flex items-center justify-center text-[10px] text-gray-400 font-mono whitespace-nowrap px-2"
            style={{
              left: `${state.left}%`,
              width: `${state.width}%`,
              top: '90%', // closer to track
              pointerEvents: 'none',
            }}
            title={state.name}
          >
            <span className="w-full text-center overflow-hidden text-ellipsis max-w-full">{state.code}</span>
          </div>
        ))}

        {/* 1c. Period Start Date (absolutely at start, closer to track) */}
        {processedData.states?.map((state, idx) => (
          <div
            key={`period-date-${idx}`}
            className="absolute text-[10px] text-gray-400 font-mono"
            style={{
              left: `${state.left}%`,
              top: '90%', // closer to track
              transform: 'translateX(-50%)',
              pointerEvents: 'none',
            }}
          >
            {formatDate(state.dateObj)}
          </div>
        ))}

        {/* 2. Period Start Markers (Status Changes) */}
        {processedData.states?.map((state, idx) => (
          <div
            key={`marker-${idx}`}
            className="absolute top-1/2 -translate-y-1/2 -translate-x-1/2 flex flex-col items-center pointer-events-none"
            style={{ left: `${state.left}%` }}
          >
            {/* Tick Mark */}
            <div className="h-10 w-px bg-white/60 absolute top-1/2 -translate-y-1/2 mix-blend-overlay"></div>
          </div>
        ))}

        {/* 3. Event Markers (Overlapping the track) */}
        {processedData.events.map((event, idx) => (
          <div
            key={`event-${idx}`}
            className="absolute top-1/2 -translate-y-1/2 -translate-x-1/2 flex flex-col items-center group/marker z-20 cursor-pointer"
            style={{ left: `${event.left}%` }}
          >
            {/* The Icon Bubble */}
            <div className="relative w-8 h-8 rounded-full flex items-center justify-center border-2 border-white shadow-md hover:scale-110 transition-transform bg-white">
              {/* Use color for icon only */}
              <span style={{ backgroundColor: event.color ? event.color : undefined }}>
                 <DynamicIcon iconName={event.iconName} />
              </span>
            </div>
            {/* The Event Name Label */}
            <div className="absolute top-full mt-1 w-32 text-center pointer-events-none opacity-0 group-hover/marker:opacity-100 transition-opacity z-30">
              <div className="inline-block">
                <span className="text-[10px] font-bold text-gray-700 bg-white/95 backdrop-blur-sm px-2 py-1 rounded shadow-sm border border-gray-200">
                  {event.name} ({formatDate(event.dateObj)})
                </span>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default PatientTimeline;