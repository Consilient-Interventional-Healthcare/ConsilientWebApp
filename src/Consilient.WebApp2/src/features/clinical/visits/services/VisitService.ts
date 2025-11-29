import type { Visit, VisitService } from '../types/visit.types';
import { dataProvider } from '@/data/DataProvider';

// const RawVisitRowSchema = z.object({
//   visitId: z.string(),
//   patientId: z.string(),
//   patientFirstName: z.string(),
//   patientLastName: z.string(),
//   hospitalizationId: z.string(),
//   admissionDate: z.string(),
//   hospitalizationStatusId: z.number(),
//   facilityId: z.string(),
//   date: z.string(),
//   providerId: z.string(),
//   providerFirstName: z.string(),
//   providerLastName: z.string(),
//   role: z.string(),
// });

export class VisitServiceImpl implements VisitService {
  async getVisits(date: string, facilityId: string): Promise<Visit[]> {
    const rows = dataProvider.query<Visit>(`
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


    return Promise.resolve(rows);
  }
}
