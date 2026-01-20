import { useEffect, useState, useRef, type ChangeEvent } from 'react';
import { useNavigate, useLoaderData } from 'react-router-dom';
import { useToast } from '@/shared/hooks/useToast';
import { assignmentsService } from '../../assignments/services/AssignmentsService';
import { visitService } from '../services/VisitService';
import { facilityService } from '../services/FacilityService';
import { formatDateFromUrl, formatDateToUrl, getToday } from '@/shared/utils/dateUtils';
import type { Visit } from '../types/visit.types';
import type { Facilities } from '@/types/api.generated';
import { Table, TableHeader, TableBody, TableRow, TableCell, TableHead } from "@/shared/components/ui/table";
import { HospitalizationStatusPill } from "../../daily-log/components/HospitalizationStatusPill";

export default function Visits() {
  const { date: urlDate, facilityId } = useLoaderData<{ date: string; facilityId: number | null }>();
  const navigate = useNavigate();
  const { success, error } = useToast();

  const [visits, setVisits] = useState<Visit[]>([]);
  const [facilities, setFacilities] = useState<Facilities.FacilityDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const dateISO = formatDateFromUrl(urlDate);
  const today = getToday();

  // Fetch facilities on mount
  useEffect(() => {
    facilityService.getAll().then(setFacilities).catch(console.error);
  }, []);

  // Fetch visits when date and facilityId change
  useEffect(() => {
    if (!facilityId) {
      setVisits([]);
      return;
    }
    setIsLoading(true);
    visitService.getVisits(dateISO, facilityId)
      .then(setVisits)
      .catch(console.error)
      .finally(() => setIsLoading(false));
  }, [dateISO, facilityId]);

  const handleDateChange = (newDateISO: string) => {
    const newUrlDate = formatDateToUrl(newDateISO);
    navigate(`/clinical/visits/${newUrlDate}${facilityId ? `/${facilityId}` : ''}`);
  };

  const handleFacilityChange = (newFacilityId: number | null) => {
    navigate(`/clinical/visits/${urlDate}${newFacilityId ? `/${newFacilityId}` : ''}`);
  };

  const handleImportClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!facilityId) {
      error('Please select a facility before importing.');
      return;
    }

    setIsUploading(true);
    try {
      const result = await assignmentsService.uploadFile(file, dateISO, facilityId);
      success(result.message ?? 'File uploaded successfully');
      navigate(`/clinical/assignments/${result.batchId}`);
    } catch {
      error('Failed to upload file. Please try again.');
    } finally {
      setIsUploading(false);
      event.target.value = '';
    }
  };

  return (
    <div className="bg-white min-h-screen p-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Visits</h1>
        </div>
        <div className="flex items-center gap-4">
          <input
            type="date"
            value={dateISO}
            max={today}
            onChange={(e) => handleDateChange(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <select
            value={facilityId ?? ''}
            onChange={(e) => handleFacilityChange(e.target.value ? Number(e.target.value) : null)}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">Select a facility...</option>
            {facilities.map((f) => (
              <option key={f.id} value={f.id}>{f.name}</option>
            ))}
          </select>
          <button
            onClick={handleImportClick}
            disabled={isUploading}
            className="bg-blue-600 text-white px-5 py-2 rounded font-semibold shadow hover:bg-blue-700 transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isUploading ? 'Uploading...' : 'Import Provider Assignment'}
          </button>
        </div>
        <input
          ref={fileInputRef}
          type="file"
          accept=".xlsx,.xls,.xlsm"
          onChange={handleFileChange}
          className="hidden"
        />
      </div>
      <div className="p-6">
        {isLoading ? (
          <div className="text-center py-8 text-gray-500">Loading visits...</div>
        ) : !facilityId ? (
          <div className="text-center py-8 text-gray-500">Please select a facility to view visits.</div>
        ) : visits.length === 0 ? (
          <div className="text-center py-8 text-gray-500">No visits found for the selected date and facility.</div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Patient</TableHead>
                <TableHead>Hospitalization Status</TableHead>
                <TableHead>Room</TableHead>
                <TableHead>Admission Date</TableHead>
                <TableHead>Physician</TableHead>
                <TableHead>Nurse</TableHead>
                <TableHead>Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {visits.map((visit) => (
                <TableRow key={visit.visitId}>
                  <TableCell>
                    <span className="font-semibold">{visit.patientFirstName} {visit.patientLastName}</span>
                    <span className="text-xs text-gray-500 ml-2">MRN: {visit.patientMrn}</span>
                    <span className="text-xs text-gray-500 ml-2">Hosp ID: {visit.hospitalizationId}</span>
                  </TableCell>
                  <TableCell>
                    <HospitalizationStatusPill statusId={visit.hospitalizationStatusId} />
                  </TableCell>
                  <TableCell>{visit.room}</TableCell>
                  <TableCell>{visit.admissionDate}</TableCell>
                  <TableCell>
                    {visit.assignedProfessionals
                      .filter((prof) => prof.role === "Physician")
                      .map((prof) => (
                        <div key={prof.providerId} className="text-xs">
                          {prof.providerFirstName} {prof.providerLastName}
                        </div>
                      ))}
                  </TableCell>
                  <TableCell>
                    {visit.assignedProfessionals
                      .filter((prof) => prof.role === "Nurse")
                      .map((prof) => (
                        <div key={prof.providerId} className="text-xs">
                          {prof.providerFirstName} {prof.providerLastName}
                        </div>
                      ))}
                  </TableCell>
                  <TableCell>{/* Placeholder for actions */}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </div>
    </div>
  );
}
