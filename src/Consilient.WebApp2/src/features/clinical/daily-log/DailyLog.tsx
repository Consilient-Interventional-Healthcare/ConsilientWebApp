import { useNavigate, useLoaderData } from 'react-router-dom';
import { useState, useEffect, useContext } from 'react';
import { DailyLogPatientSideBar } from './components/DailyLogPatientSideBar';
import { DailyLogHeader } from './components/DailyLogHeader';
import { DailyLogEntries } from './components/DailyLogEntries';
import { DailyLogEntryInput } from './components/DailyLogEntryInput';
import { dailyLogService } from './services/DailyLogService';
import type { ProviderAssignments, Assignment } from './types/dailylog.types';
import LoadingBarContext from '@/shared/layouts/LoadingBarContext';

export default function DailyLog() {
  const { date, providerId, patientId } = useLoaderData<{ date: string; providerId?: string; patientId?: string }>();
  const navigate = useNavigate();
  const [selectedPatientId, setSelectedPatientId] = useState<string | null>(patientId ?? null);
  const [logEntries, setLogEntries] = useState<{
    id: string;
    patientId: string;
    timestamp: Date;
    content: string;
    author: string;
  }[]>([]);
  const [providerAssignments, setProviderAssignments] = useState<ProviderAssignments[]>([]);
  const loadingBar = useContext(LoadingBarContext);

  useEffect(() => {
    loadingBar?.start();
    dailyLogService.getAssignmentsByDate(date)
      .then((data: ProviderAssignments[]) => setProviderAssignments(data))
      .catch((err) => {
        // Optionally handle error, e.g. show toast or log
        console.error('Failed to fetch assignments', err);
      })
      .finally(() => {
        loadingBar?.complete();
      });
  }, [date, loadingBar]);

  const handleProviderChange = (newProviderId: string) => {
    if (newProviderId) {
      if (selectedPatientId) {
        void navigate(`/clinical/daily-log/${date}/${newProviderId}/${selectedPatientId}`, { replace: true });
      } else {
        void navigate(`/clinical/daily-log/${date}/${newProviderId}`, { replace: true });
      }
    } else {
      void navigate(`/clinical/daily-log/${date}`, { replace: true });
    }
  };

  const handlePatientSelect = (id: string) => {
    setSelectedPatientId(id);
    if (providerId) {
      void navigate(`/clinical/daily-log/${date}/${providerId}/${id}`, { replace: true });
    }
  };

  const handleAddLogEntry = (content: string) => {
    if (!selectedPatientId || !content.trim()) return;

    const newEntry = {
      id: `log-${Date.now()}`,
      patientId: selectedPatientId,
      timestamp: new Date(),
      content: content.trim(),
      author: 'Current User',
    };

    setLogEntries([...logEntries, newEntry]);
  };

  const selectedPatientEntries = logEntries.filter(
    entry => entry.patientId === selectedPatientId
  );

  let assignment: Assignment | null = null;
  if (selectedPatientId) {
    const providerAssignment = providerAssignments.find(p => p.providerId === providerId);
    const found = providerAssignment?.assignments.find(a => a.patient.id === selectedPatientId);
    assignment = found ?? null;
  }

  // console.log('providerId', providerId, 'selectedPatientId', selectedPatientId, 'assignment', assignment);

  return (
    <div className="flex h-full bg-gray-50 overflow-hidden">
      <DailyLogPatientSideBar
        selectedPatientId={selectedPatientId}
        onPatientSelect={handlePatientSelect}
        date={date}
        {...(providerId && { providerId })}
        onProviderChange={handleProviderChange}
        providerAssignments={providerAssignments}
      />
      <div className="flex-1 flex flex-col bg-white overflow-hidden">
        {selectedPatientId ? (
          <>
            <DailyLogHeader assignment={assignment} />
            <DailyLogEntries entries={selectedPatientEntries} />
            <div className="flex-shrink-0">
              <DailyLogEntryInput onSubmit={handleAddLogEntry} />
            </div>
          </>
        ) : null}
      </div>
    </div>
  );
}
