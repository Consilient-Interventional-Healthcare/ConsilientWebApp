
import { useNavigate, useLoaderData } from 'react-router-dom';
import { useState } from 'react';
import { DailyLogPatientSideBar } from './components/DailyLogPatientSideBar';
import { DailyLogHeader } from './components/DailyLogHeader';
import { DailyLogEntries } from './components/DailyLogEntries';
import { DailyLogEntryInput } from './components/DailyLogEntryInput';


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

  // Mock patients (should be replaced with real data)
  const mockPatients = [
    { id: 'P001', name: 'Anderson, Jane', dateOfBirth: '1991-05-12', gender: 'Female', admissionDate: '2025-11-10', status: 'active' },
    { id: 'P002', name: 'Brown, Michael', dateOfBirth: '1983-02-20', gender: 'Male', admissionDate: '2025-11-15', status: 'pending' },
    { id: 'P003', name: 'Chen, Lisa', dateOfBirth: '1996-08-03', gender: 'Female', admissionDate: '2025-11-18', status: 'active' },
    { id: 'P004', name: 'Davis, Robert', dateOfBirth: '1974-11-30', gender: 'Male', admissionDate: '2025-11-01', status: 'completed' },
    { id: 'P005', name: 'Evans, Sarah', dateOfBirth: '1988-03-15', gender: 'Female', admissionDate: '2025-11-05', status: 'active' },
    { id: 'P006', name: 'Foster, James', dateOfBirth: '1979-07-22', gender: 'Male', admissionDate: '2025-11-17', status: 'pending' },
    { id: 'P007', name: 'Garcia, Maria', dateOfBirth: '1968-09-10', gender: 'Female', admissionDate: '2025-11-12', status: 'active' },
    { id: 'P008', name: 'Harris, David', dateOfBirth: '1962-01-05', gender: 'Male', admissionDate: '2025-11-03', status: 'active' },
    { id: 'P009', name: 'Johnson, Emma', dateOfBirth: '1997-12-25', gender: 'Female', admissionDate: '2025-11-19', status: 'completed' },
    { id: 'P010', name: 'Kim, Daniel', dateOfBirth: '1985-06-18', gender: 'Male', admissionDate: '2025-11-08', status: 'active' },
  ];

  let patientName = '';
  let patientDateOfBirth: string | null = null;
  let patientGender: string | null = null;
  let admissionDate: string | null = null;
  let patientStatus: 'active' | 'pending' | 'completed' | null = null;
  if (selectedPatientId) {
    const patient = mockPatients.find(p => p.id === selectedPatientId);
    if (patient) {
      const [last, first] = patient.name.split(',').map(s => s.trim());
      patientName = `${last}, ${first}`;
      patientDateOfBirth = patient.dateOfBirth;
      patientGender = patient.gender;
      admissionDate = patient.admissionDate;
      patientStatus = (['active', 'pending', 'completed'].includes(patient.status) ? patient.status : null) as 'active' | 'pending' | 'completed' | null;
    }
  }

  return (
    <div className="flex h-full bg-gray-50 overflow-hidden">
      <DailyLogPatientSideBar
        selectedPatientId={selectedPatientId}
        onPatientSelect={handlePatientSelect}
        date={date}
        {...(providerId && { providerId })}
        onProviderChange={handleProviderChange}
      />
      <div className="flex-1 flex flex-col bg-white overflow-hidden">
        {selectedPatientId ? (
          <>
            <DailyLogHeader
              patientName={patientName}
              patientId={selectedPatientId ?? ''}
              patientDateOfBirth={patientDateOfBirth}
              patientGender={patientGender}
              admissionDate={admissionDate}
              patientStatus={patientStatus}
            />
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
