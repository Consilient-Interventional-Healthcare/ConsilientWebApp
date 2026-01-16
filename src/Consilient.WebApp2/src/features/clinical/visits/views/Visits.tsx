import { useEffect, useState, useRef, type ChangeEvent } from 'react';
import { useToast } from '@/shared/hooks/useToast';
import { assignmentsService } from '../services/AssignmentsService';
import type { Visit } from '../types/visit.types';
import { VisitServiceImpl } from '../services/VisitService';
import { Table, TableHeader, TableBody, TableRow, TableCell, TableHead } from "@/shared/components/ui/table";
import { HospitalizationStatusPill } from "../../daily-log/components/HospitalizationStatusPill";

const visitService = new VisitServiceImpl();

export default function Visits() {
  const [visits, setVisits] = useState<Visit[]>([]);
  const [isUploading, setIsUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const { success, error } = useToast();

  const handleImportClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const today = new Date().toISOString().slice(0, 10);
    const facilityId = 1;

    setIsUploading(true);
    try {
      const result = await assignmentsService.uploadFile(file, today, facilityId);
      success(result.message ?? 'File uploaded successfully');
    } catch {
      error('Failed to upload file. Please try again.');
    } finally {
      setIsUploading(false);
      event.target.value = '';
    }
  };

  useEffect(() => {
    void visitService.getVisits('2025-11-26', 'facility-1').then((data) => {
      setVisits(data);
    });
  }, []);

  return (
    <div className="bg-white min-h-screen p-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Visits</h1>
        </div>
        <button
          onClick={handleImportClick}
          disabled={isUploading}
          className="bg-blue-600 text-white px-5 py-2 rounded font-semibold shadow hover:bg-blue-700 transition disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isUploading ? 'Uploading...' : 'Import From Assignments'}
        </button>
        <input
          ref={fileInputRef}
          type="file"
          accept=".xlsx,.xls,.xlsm"
          onChange={handleFileChange}
          className="hidden"
        />
      </div>
      <div className="p-6">
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
      </div>
    </div>
  );
}
