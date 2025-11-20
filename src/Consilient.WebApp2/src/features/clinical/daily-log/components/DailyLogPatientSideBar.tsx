import React from 'react';
import { cn } from '@/shared/utils/utils';
import { DailyLogPatientSideBarHeader } from './DailyLogPatientSideBarHeader';

interface Patient {
  id: string;
  name: string;
  room?: string;
  status?: 'active' | 'pending' | 'completed';
  providerId: string;
}

interface Provider {
  id: string;
  name: string;
}

interface PatientSidebarProps {
  selectedPatientId: string | null;
  onPatientSelect: (id: string) => void;
  date: string;
  providerId?: string;
  onProviderChange?: (providerId: string) => void;
  patientId?: string;
}

// Mock providers
const mockProviders: Provider[] = [
  { id: 'PR001', name: 'Dr. Smith' },
  { id: 'PR002', name: 'Dr. Johnson' },
  { id: 'PR003', name: 'Dr. Williams' },
];

// Mock data - replace with actual data fetching
const mockPatients: Patient[] = [
  { id: 'P001', name: 'Anderson, Jane', room: '101', status: 'active', providerId: 'PR001' },
  { id: 'P002', name: 'Brown, Michael', room: '102', status: 'pending', providerId: 'PR001' },
  { id: 'P003', name: 'Chen, Lisa', room: '103', status: 'active', providerId: 'PR001' },
  { id: 'P004', name: 'Davis, Robert', room: '104', status: 'completed', providerId: 'PR002' },
  { id: 'P005', name: 'Evans, Sarah', room: '105', status: 'active', providerId: 'PR002' },
  { id: 'P006', name: 'Foster, James', room: '106', status: 'pending', providerId: 'PR002' },
  { id: 'P007', name: 'Garcia, Maria', room: '107', status: 'active', providerId: 'PR002' },
  { id: 'P008', name: 'Harris, David', room: '108', status: 'active', providerId: 'PR003' },
  { id: 'P009', name: 'Johnson, Emma', room: '109', status: 'completed', providerId: 'PR003' },
  { id: 'P010', name: 'Kim, Daniel', room: '110', status: 'active', providerId: 'PR003' },
];

export function DailyLogPatientSideBar({ selectedPatientId, onPatientSelect, date, providerId, onProviderChange }: PatientSidebarProps) {
  const [selectedProvider, setSelectedProvider] = React.useState<string>(providerId ?? '');

  const filteredPatients = React.useMemo(() => (
    selectedProvider
      ? mockPatients.filter(p => p.providerId === selectedProvider)
      : []
  ), [selectedProvider]);

  // Keyboard navigation for patient list
  React.useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.altKey && (e.key === 'ArrowUp' || e.key === 'ArrowDown')) {
        if (!filteredPatients.length) return;
        e.preventDefault();
        if (!selectedPatientId) return;
        const idx = filteredPatients.findIndex(p => p.id === selectedPatientId);
        if (idx === -1) return;
        let nextIdx = idx;
        if (e.key === 'ArrowUp') {
          nextIdx = idx === 0 ? filteredPatients.length - 1 : idx - 1;
        } else if (e.key === 'ArrowDown') {
          nextIdx = idx === filteredPatients.length - 1 ? 0 : idx + 1;
        }
        const nextPatient = filteredPatients[nextIdx];
        if (nextPatient) {
          onPatientSelect(nextPatient.id);
        }
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [filteredPatients, selectedPatientId, onPatientSelect]);

  const handleProviderChange = (newProviderId: string) => {
    setSelectedProvider(newProviderId);
    onProviderChange?.(newProviderId);
  };

  // Auto-select first patient when provider changes or on initial load
  React.useEffect(() => {
    if (filteredPatients.length > 0 && !selectedPatientId && filteredPatients[0]) {
      onPatientSelect(filteredPatients[0].id);
    }
  }, [filteredPatients, selectedPatientId, onPatientSelect]);

  // Calculate min and max date for input
  const today = new Date();
  const maxDate: string = today.toISOString().split('T')[0] ?? '';
  const minDateObj = new Date(today);
  minDateObj.setDate(today.getDate() - 7);
  const minDate: string = minDateObj.toISOString().split('T')[0] ?? '';

  return (
    <div className="w-80 border-r border-gray-200 bg-white flex flex-col h-full relative">
      {/* Header */}
      <DailyLogPatientSideBarHeader
        date={date}
        minDate={minDate}
        maxDate={maxDate}
        providerId={selectedProvider}
        selectedProvider={selectedProvider}
        onProviderChange={handleProviderChange}
        mockProviders={mockProviders}
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
        ) : filteredPatients.length === 0 ? (
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
          filteredPatients.map(patient => (
            <button
              key={patient.id}
              onClick={() => onPatientSelect(patient.id)}
              className={cn(
                'w-full text-left px-4 py-3 hover:bg-gray-50 transition-colors border-l-4 border-b border-gray-100',
                selectedPatientId === patient.id
                  ? 'bg-blue-50 border-l-blue-600'
                  : 'border-l-transparent'
              )}
            >
              <div className="flex items-center justify-between">
                <div className="flex-1 min-w-0">
                  <p className={cn(
                    'text-sm font-medium truncate',
                    selectedPatientId === patient.id
                      ? 'text-blue-900'
                      : 'text-gray-900'
                  )}>
                    {patient.name}
                  </p>
                  <div className="flex items-center gap-2 mt-1">
                    <span className="text-xs text-gray-500">{patient.id}</span>
                    {patient.room && (
                      <>
                        <span className="text-gray-300">•</span>
                        <span className="text-xs text-gray-500">Room {patient.room}</span>
                      </>
                    )}
                  </div>
                </div>
                {/* Status indicator */}
                {patient.status && (
                  <div className="ml-2">
                    <span className={cn(
                      'inline-block w-2 h-2 rounded-full',
                      patient.status === 'active' && 'bg-green-500',
                      patient.status === 'pending' && 'bg-yellow-500',
                      patient.status === 'completed' && 'bg-gray-400'
                    )} />
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
