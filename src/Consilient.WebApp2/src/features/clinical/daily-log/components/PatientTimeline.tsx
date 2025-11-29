import React, { useMemo } from 'react';
import type { ReactNode } from 'react';
import { Activity, CheckCircle, AlertCircle, Info } from 'lucide-react';

// --- Types ---

interface EventItem {
  date: string;
  name: string;
  color: string;
}

interface PeriodItem {
  startDate: string;
  name: string;
  color: string;
}

interface PatientTimelineProps {
  events?: EventItem[];
  periods?: PeriodItem[];
}

interface ProcessedPeriod extends PeriodItem {
  dateObj: Date;
  left: number;
  width: number;
  duration: number;
  colorClass: string;
}

interface ProcessedEvent extends EventItem {
  dateObj: Date;
  left: number;
  colorClass: string;
}

// --- Sample Data ---

const SAMPLE_DATA: { events: EventItem[]; periods: PeriodItem[] } = {
  events: [
    {
      date: "2025-10-05",
      name: "Psych Eval",
      color: "yellow"
    },
    {
      date: "2025-10-25",
      name: "Discharge",
      color: "green"
    }
  ],
  periods: [
    {
      startDate: "2025-10-05",
      name: "Acute",
      color: "blue"
    },
    {
      startDate: "2025-10-15",
      name: "Acute",
      color: "black"
    },
    {
      startDate: "2025-10-23",
      name: "Pending Discharge",
      color: "gray"
    }
  ]
};

const PatientTimeline: React.FC<PatientTimelineProps> = ({ 
  events = SAMPLE_DATA.events, 
  periods = SAMPLE_DATA.periods 
}) => {

  // --- Helpers & Logic ---

  // 1. Color Palette Generator
  const resolveColor = (colorInput: string, name: string, type: 'period' | 'event'): string => {
    if (colorInput) {
      const c = colorInput.toLowerCase();
      const styleMap: Record<string, string> = {
        yellow: type === 'period' ? 'bg-yellow-100 text-yellow-800 border-yellow-200' : 'text-yellow-700 bg-yellow-100 border-yellow-300',
        green: type === 'period' ? 'bg-emerald-100 text-emerald-800 border-emerald-200' : 'text-emerald-700 bg-emerald-100 border-emerald-300',
        blue: type === 'period' ? 'bg-blue-100 text-blue-800 border-blue-200' : 'text-blue-700 bg-blue-100 border-blue-300',
        black: type === 'period' ? 'bg-gray-800 text-gray-100 border-gray-700' : 'text-gray-900 bg-gray-200 border-gray-400',
        gray: type === 'period' ? 'bg-gray-200 text-gray-800 border-gray-300' : 'text-gray-600 bg-gray-100 border-gray-300',
      };
      if (styleMap[c]) return styleMap[c];
      return colorInput; 
    }

    const n = (name || '').toLowerCase();
    if (type === 'period') {
      if (n.includes('acute')) return 'bg-red-100 text-red-700 border-red-200';
      if (n.includes('pending')) return 'bg-amber-100 text-amber-700 border-amber-200';
      if (n.includes('discharge')) return 'bg-emerald-100 text-emerald-700 border-emerald-200';
      return 'bg-blue-100 text-blue-700 border-blue-200';
    } else {
      if (n.includes('eval')) return 'text-purple-600 bg-purple-100 border-purple-200';
      if (n.includes('discharge')) return 'text-emerald-600 bg-emerald-100 border-emerald-200';
      return 'text-gray-600 bg-gray-100 border-gray-200';
    }
  };

  const getIcon = (name: string): ReactNode => {
    const n = (name || '').toLowerCase();
    if (n.includes('discharge')) return <CheckCircle size={16} />;
    if (n.includes('eval')) return <Activity size={16} />;
    if (n.includes('acute')) return <AlertCircle size={16} />;
    return <Info size={16} />;
  };

  // 2. Process Data for Timeline
  const processedData = useMemo(() => {
    // Safety check: Ensure we have arrays
    const safeEvents = events || [];
    const safePeriods = periods || [];

    if (!safeEvents.length && !safePeriods.length) {
        return { periods: [], events: [], minDate: new Date(), maxDate: new Date() };
    }

    const procEvents = safeEvents.map(e => ({ ...e, dateObj: new Date(e.date) }));
    const procPeriods = safePeriods.map(p => ({ ...p, dateObj: new Date(p.startDate) }));

    procEvents.sort((a, b) => a.dateObj.getTime() - b.dateObj.getTime());
    procPeriods.sort((a, b) => a.dateObj.getTime() - b.dateObj.getTime());

    // Calculate Strict Boundaries (No Buffers)
    const allDates = [...procEvents.map(e => e.dateObj), ...procPeriods.map(p => p.dateObj)];
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

    const calculatedPeriods: ProcessedPeriod[] = procPeriods.map((p, index) => {
      const startPos = getPos(p.dateObj);
      let endPos: number;
      let endDate: Date;
      
      if (index < procPeriods.length - 1 && procPeriods[index + 1]?.dateObj) {
        // Ends exactly where the next one starts
        endDate = procPeriods[index + 1].dateObj;
        endPos = getPos(endDate);
      } else {
        // Last period extends to the exact end of the timeline (100%)
        endDate = maxDate;
        endPos = 100;
      }

      // Calculate duration in days
      const diffTime = Math.abs(endDate.getTime() - p.dateObj.getTime());
      const days = Math.round(diffTime / (1000 * 60 * 60 * 24)); 

      return {
        ...p,
        left: startPos,
        width: Math.max(endPos - startPos, 0),
        duration: days,
        colorClass: resolveColor(p.color, p.name, 'period')
      };
    });

    const calculatedEvents: ProcessedEvent[] = procEvents.map(e => ({
      ...e,
      left: getPos(e.dateObj),
      colorClass: resolveColor(e.color, e.name, 'event')
    }));

    return { 
      periods: calculatedPeriods, 
      events: calculatedEvents, 
      minDate, 
      maxDate 
    };
  }, [events, periods]);

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
          {processedData.periods.map((period, idx) => (
            <div
              key={`period-${idx}`}
              className={`absolute top-0 bottom-0 border-r border-white/20 last:border-0 transition-all hover:opacity-90 cursor-pointer flex items-center justify-center overflow-hidden ${period.colorClass}`}
              style={{ 
                left: `${period.left}%`, 
                width: `${period.width}%` 
              }}
              title={`${period.name}: ${period.duration} days`}
            >
              {period.width > 5 && (
                 <span className="text-xs font-bold opacity-70 whitespace-nowrap px-1 select-none">
                   {period.duration}d
                 </span>
              )}
            </div>
          ))}
        </div>

        {/* 2. Period Start Markers (Status Changes) */}
        {processedData.periods.map((period, idx) => (
          <div
            key={`marker-${idx}`}
            className="absolute top-1/2 -translate-y-1/2 -translate-x-1/2 flex flex-col items-center pointer-events-none"
            style={{ left: `${period.left}%` }}
          >
            {/* Tick Mark */}
            <div className="h-10 w-px bg-white/60 absolute top-1/2 -translate-y-1/2 mix-blend-overlay"></div>
            
            {/* Status Date Label */}
            <div className="absolute top-5 mt-1 flex flex-col items-center">
              <span className="text-[10px] text-gray-400 font-mono">
                {formatDate(period.dateObj)}
              </span>
              <span className="text-[10px] font-semibold text-gray-600 whitespace-nowrap max-w-[100px] overflow-hidden text-ellipsis">
                {period.name}
              </span>
            </div>
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
            <div className={`relative w-8 h-8 rounded-full flex items-center justify-center border-2 border-white shadow-md hover:scale-110 transition-transform ${event.colorClass}`}>
              {getIcon(event.name)}
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