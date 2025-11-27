import React from 'react';
import { cn } from '@/shared/utils/utils';
import { DailyLogPatientSideBarHeader } from './DailyLogPatientSideBarHeader';
import type { ProviderAssignments, Assignment } from '@/features/clinical/daily-log/types/dailylog.types';
import { HospitalizationStatusPill } from './HospitalizationStatusPill';

interface PatientSidebarProps {
  selectedPatientId: string | null;
  onPatientSelect: (id: string) => void;
  date: string;
  providerId?: string;  
  onProviderChange?: (providerId: string) => void;
  patientId?: string;
  providerAssignments: ProviderAssignments[];
}

export function DailyLogPatientSideBar({ selectedPatientId, onPatientSelect, date, providerId, onProviderChange, providerAssignments }: PatientSidebarProps) {
  const initialProvider = providerId ?? ((Array.isArray(providerAssignments) && providerAssignments.length > 0 && providerAssignments[0]?.providerId) ? providerAssignments[0].providerId : '');
  const [selectedProvider, setSelectedProvider] = React.useState<string>(initialProvider);

  // Get patients for selected provider
  const filteredAssignments: Assignment[] = React.useMemo(() => {
    const providerAssignment = providerAssignments.find(a => a.providerId === selectedProvider);
    return providerAssignment ? providerAssignment.assignments : [];
  }, [providerAssignments, selectedProvider]);

  // Keyboard navigation for patient list
  React.useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.altKey && (e.key === 'ArrowUp' || e.key === 'ArrowDown')) {
        if (!filteredAssignments.length) return;
        e.preventDefault();
        if (!selectedPatientId) return;
        const idx = filteredAssignments.findIndex(p => p.patient.id === selectedPatientId);
        if (idx === -1) return;
        let nextIdx = idx;
        if (e.key === 'ArrowUp') {
          nextIdx = idx === 0 ? filteredAssignments.length - 1 : idx - 1;
        } else if (e.key === 'ArrowDown') {
          nextIdx = idx === filteredAssignments.length - 1 ? 0 : idx + 1;
        }
        const nextPatient = filteredAssignments[nextIdx];
        if (nextPatient) {
          onPatientSelect(nextPatient.patient.id);
        }
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [filteredAssignments, selectedPatientId, onPatientSelect]);

  const handleProviderChange = (newProviderId: string) => {
    setSelectedProvider(newProviderId);
    onProviderChange?.(newProviderId);
  };

  // Auto-select first patient when provider changes or on initial load
  React.useEffect(() => {
    if (filteredAssignments.length > 0 && !selectedPatientId && filteredAssignments[0]) {
      onPatientSelect(filteredAssignments[0].patient.id);
    }
  }, [filteredAssignments, selectedPatientId, onPatientSelect]);

  // Calculate min and max date for input
  const today = new Date();
  const maxDate: string = today.toISOString().split('T')[0] ?? '';
  const minDateObj = new Date(today);
  minDateObj.setDate(today.getDate() - 7);
  const minDate: string = minDateObj.toISOString().split('T')[0] ?? '';

  return (
    <div className="w-80 border-r border-gray-200 bg-white flex flex-col h-full relative">
      <DailyLogPatientSideBarHeader
        date={date}
        minDate={minDate}
        maxDate={maxDate}
        providerId={selectedProvider}
        selectedProvider={selectedProvider}
        onProviderChange={handleProviderChange}
        providerAssignments={providerAssignments}
      />
      {/* Patient List */}
      <div className="flex-1 overflow-y-auto">
        {!selectedProvider ? (
          <div className="p-8 text-center text-gray-500">
            <svg
              className="mx-auto h-12 w-12 text-gray-400 mb-2"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
              />
            </svg>
            <p className="text-sm">Please select a provider</p>
          </div>
        ) : filteredAssignments.length === 0 ? (
          <div className="p-8 text-center text-gray-500">
            <svg
              className="mx-auto h-12 w-12 text-gray-400 mb-2"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M12 12h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <p className="text-sm">No patients found</p>
          </div>
        ) : (
          filteredAssignments.map(assignment => (
            <button
              key={assignment.patient.id}
              onClick={() => onPatientSelect(assignment.patient.id)}
              className={cn(
                'w-full text-left px-4 py-3 hover:bg-gray-50 transition-colors border-l-4 border-b border-gray-100',
                selectedPatientId === assignment.patient.id
                  ? 'bg-blue-50 border-l-blue-600'
                  : 'border-l-transparent'
              )}
            >
              <div className="flex items-center justify-between">
                <div className="flex-1 min-w-0">
                  <p className={cn(
                    'text-sm font-medium truncate',
                    selectedPatientId === assignment.patient.id
                      ? 'text-blue-900'
                      : 'text-gray-900'
                  )}>
                    {assignment.patient.lastName}, {assignment.patient.firstName}
                  </p>
                </div>
                {assignment.hospitalization?.hospitalizationStatusId && (
                  <div className="ml-2">
                    <HospitalizationStatusPill statusId={assignment.hospitalization.hospitalizationStatusId} />
                  </div>
                )}
              </div>
            </button>
          ))
        )}
      </div>
      {/* Sticky footer for shortcut help */}
      <div className="sticky bottom-0 w-full bg-white border-t border-gray-200 px-4 py-2 text-xs text-gray-500 flex items-center gap-4 z-10">
        <span>
          <kbd className="px-1 py-0.5 bg-gray-200 rounded border text-gray-700">Alt</kbd> + <kbd className="px-1 py-0.5 bg-gray-200 rounded border text-gray-700">↑</kbd> Previous
        </span>
        <span>
          <kbd className="px-1 py-0.5 bg-gray-200 rounded border text-gray-700">Alt</kbd> + <kbd className="px-1 py-0.5 bg-gray-200 rounded border text-gray-700">↓</kbd> Next
        </span>
      </div>
    </div>
  );
}
