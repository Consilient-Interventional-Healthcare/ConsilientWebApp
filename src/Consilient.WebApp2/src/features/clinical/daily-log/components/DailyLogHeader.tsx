"use client";
import { useState } from 'react';
import type { Assignment, HospitalizationStatus } from '../types/dailylog.types';
import { HOSPITALIZATION_STATUSES } from '../types/dailylog.types';
import { SegmentedControl } from '@/shared/components/ui/segmented-control';
import { StatusComboBox } from '@/shared/components/ui/status-combobox';

interface DailyLogHeaderProps {
  assignment: Assignment | null;
}

export function DailyLogHeader({ assignment }: DailyLogHeaderProps) {
  const [statusId, setStatusId] = useState<number>(assignment?.hospitalization.hospitalizationStatusId ?? 0);
  if (!assignment) return null;
  const selectedStatus: HospitalizationStatus | undefined = HOSPITALIZATION_STATUSES.find(s => s.id === statusId);
  return (
    <div
      className="flex-shrink-0 bg-white"
      style={{ borderBottom: `6px solid ${selectedStatus?.color ?? '#e0e0e0'}` }}
    >
      <div className="px-6 py-4">
        <div className="flex items-center justify-between gap-x-6">
          {/* Left: Patient Name */}
          <div className="flex items-center gap-x-2 min-w-0">
            <h2 className="text-lg font-semibold text-gray-900 truncate">
              {assignment.patient.firstName} {assignment.patient.lastName}
            </h2>
          </div>
          {/* Center: Segmented Control */}
          <div className="flex-1 flex justify-center">
            <SegmentedControl
              options={[
                { label: 'All', value: 'all' },
                { label: 'Interval', value: 'interval' },
                { label: 'Interview', value: 'interview' },
                { label: 'Plan', value: 'plan' },
              ]}
              defaultValue="all"
            />
          </div>
          {/* Right: Hospitalization Status ComboBox */}
          <div className="flex items-center justify-end min-w-0">
            <StatusComboBox value={statusId} onChange={setStatusId} />
          </div>
        </div>
      </div>
    </div>
  );
}
