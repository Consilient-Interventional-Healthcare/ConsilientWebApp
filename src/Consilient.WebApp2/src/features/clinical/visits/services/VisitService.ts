import { z } from 'zod';
import { Visit, VisitSchema } from '../types/visit.types';
// @ts-expect-error - alasql does not have proper type definitions
import alasql from 'alasql'; // Note: alasql type issues persist.
// import db from '@/data/db.json'; // No longer used

const RawVisitRowSchema = z.object({
  visitId: z.string(),
  patientId: z.string(),
  patientFirstName: z.string(),
  patientLastName: z.string(),
  hospitalizationId: z.string(),
  admissionDate: z.string(),
  hospitalizationStatusId: z.number(),
  facilityId: z.string(),
  date: z.string(),
  providerId: z.string(),
  providerFirstName: z.string(),
  providerLastName: z.string(),
  role: z.string(),
});

export interface VisitService {
  getVisits(date: string, facilityId: string): Promise<Visit[]>;
}

export class VisitServiceImpl implements VisitService {
  async getVisits(date: string, facilityId: string): Promise<Visit[]> {
    // Flat query
    const rows = alasql(`
      SELECT
        a.id AS visitId,
        a.patientId,
        a.hospitalizationId,
        a.date,
        p.firstName AS patientFirstName,
        p.lastName AS patientLastName,
        h.admissionDate,
        h.hospitalizationStatusId,
        h.facilityId,
        ap.providerId,
        pr.firstName AS providerFirstName,
        pr.lastName AS providerLastName,
        ap.role
      FROM visits AS a
      JOIN patients AS p ON a.patientId = p.id
      JOIN hospitalizations AS h ON a.hospitalizationId = h.id
      JOIN assignedProfessionals AS ap ON ap.visitId = a.id
      JOIN providers AS pr ON ap.providerId = pr.id
      WHERE a.date = ? AND h.facilityId = ?
    `, [date, facilityId]);

    // Group by visitId in JS
    const parsedRows = RawVisitRowSchema.array().parse(rows);

    const visits = Object.values(
      parsedRows.reduce((acc: Record<string, Visit>, row) => {
        const currentVisit = (acc[row.visitId] ??= {
            visitId: row.visitId,
            patientId: row.patientId,
            patientFirstName: row.patientFirstName,
            patientLastName: row.patientLastName,
            patientMrn: undefined, // Optional in schema, can be undefined
            hospitalizationId: row.hospitalizationId,
            admissionDate: row.admissionDate,
            hospitalizationStatusId: row.hospitalizationStatusId,
            room: undefined, // Optional in schema, can be undefined
            date: row.date,
            assignedProfessionals: [],
          });
        currentVisit.assignedProfessionals.push({
          providerId: row.providerId,
          providerFirstName: row.providerFirstName,
          providerLastName: row.providerLastName,
          role: row.role,
        });
        return acc;
      }, {} as Record<string, Visit>)
    );

    return Promise.resolve(VisitSchema.array().parse(visits));
  }
}
