import { useEffect, useState } from 'react';
import type { Visit } from '../types/visit.types';
import { VisitServiceImpl } from '../services/VisitService';
import { Table, TableHeader, TableBody, TableRow, TableCell, TableHead } from "@/shared/components/ui/table";
import { HospitalizationStatusPill } from "../../daily-log/components/HospitalizationStatusPill";

const visitService = new VisitServiceImpl();

export default function Visits() {
  const [visits, setVisits] = useState<Visit[]>([]);

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
        <button className="bg-blue-600 text-white px-5 py-2 rounded font-semibold shadow hover:bg-blue-700 transition">
          Import From Assignments
        </button>
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
