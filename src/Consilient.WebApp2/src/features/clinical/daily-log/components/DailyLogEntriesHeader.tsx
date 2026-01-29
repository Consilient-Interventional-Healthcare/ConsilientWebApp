"use client";
import { useState, useEffect } from 'react';
import { SegmentedControl } from '@/shared/components/ui/segmented-control';
import { StatusComboBox } from '@/shared/components/ui/status-combobox';
import type { GraphQL } from '@/types/api.generated';
import { useVisitEventTypes } from '@/shared/stores/VisitEventTypeStore';
import { useHospitalizationStatusById } from '@/shared/stores/HospitalizationStatusStore';

interface DailyLogEntriesHeaderProps {
  visit: GraphQL.DailyLogVisit | null;
  typeFilter?: string;
  onTypeFilterChange?: ((value: string) => void) | undefined;
}

export function DailyLogEntriesHeader({
  visit,
  typeFilter = "all",
  onTypeFilterChange,
}: DailyLogEntriesHeaderProps) {
  const [statusId, setStatusId] = useState<number | undefined>(visit?.hospitalization?.hospitalizationStatusId);
  const { data: eventTypes = [] } = useVisitEventTypes();
  const selectedStatus = useHospitalizationStatusById(statusId);

  // Sync statusId when visit changes
  useEffect(() => {
    setStatusId(visit?.hospitalization?.hospitalizationStatusId);
  }, [visit?.hospitalization?.hospitalizationStatusId]);

  // All hooks must be called before any early returns
  if (!visit) return null;

  const options = eventTypes.map(t => ({
    label: t.name,
    value: t.code,
    icon: t.iconName ?? '',
    color: t.color ?? ''
  }));
  
  return (
    <div
      className="flex-shrink-0 bg-white"
      style={{ borderBottom: `6px solid ${selectedStatus?.color ?? '#e0e0e0'}` }}
    >
      <div className="px-6 py-4">
        <div className="flex items-center justify-between gap-x-6">
          {/* Left: Patient Name */}
          <div className="w-48 flex items-center gap-x-2 flex-shrink-0">
            <h2 className="text-lg font-semibold text-gray-900 truncate">
              {visit.patient?.firstName} {visit.patient?.lastName}
            </h2>
          </div>
          {/* Center: Segmented Control */}
          <div className="flex justify-center">
            <SegmentedControl
              options={options}
              defaultValue={typeFilter}
              onChange={onTypeFilterChange}
            />
          </div>
          {/* Right: Hospitalization Status ComboBox */}
          <div className="w-48 flex items-center justify-end flex-shrink-0">
              <StatusComboBox value={statusId} onChange={setStatusId} />
          </div>
        </div>
      </div>
    </div>
  );
}
