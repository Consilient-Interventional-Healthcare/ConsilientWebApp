"use client";
import { useState } from 'react';
import { SegmentedControl } from '@/shared/components/ui/segmented-control';
import { StatusComboBox } from '@/shared/components/ui/status-combobox';
import { dataProvider } from '@/data/DataProvider';
import type { DailyLogVisit } from "../dailylog.types";
import type { Hospitalizations, VisitEvents } from '@/types/api.generated';
interface DailyLogEntriesHeaderProps {
  visit: DailyLogVisit | null;
  typeFilter?: string;
  onTypeFilterChange?: ((value: string) => void) | undefined;
}

export function DailyLogEntriesHeader({
  visit,
  typeFilter = "all",
  onTypeFilterChange,
}: DailyLogEntriesHeaderProps) {
  const [statusId, setStatusId] = useState<number | undefined>(visit?.hospitalizationStatusId);
  if (!visit) return null;
  const [selectedStatus = null] = dataProvider.query<Hospitalizations.HospitalizationStatusDto>(
    "SELECT * FROM hospitalizationStatuses WHERE id = ?",
    [statusId]
  );

  const options = [
    { label: "All", value: "all" },
    ...dataProvider.getTable<VisitEvents.VisitEventTypeDto>('visitEventTypes')
  ];
  
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
              {visit.patientFirstName} {visit.patientLastName}
            </h2>
          </div>
          {/* Center: Segmented Control */}
          <div className="flex-1 flex justify-center">
            <SegmentedControl
              options={options}
              defaultValue={typeFilter}
              onChange={onTypeFilterChange}
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
