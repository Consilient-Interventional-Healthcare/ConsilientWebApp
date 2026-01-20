
import { z } from 'zod';

export interface VisitService {
  getVisits(date: string, facilityId: number): Promise<Visit[]>;
}

export const AssignedProfessionalSchema = z.object({
  providerId: z.string(),
  providerFirstName: z.string(),
  providerLastName: z.string(),
  role: z.string(),
});

export const VisitSchema = z.object({
  visitId: z.string(),
  patientId: z.string(),
  patientLastName: z.string(),
  patientFirstName: z.string(),
  hospitalizationStatusId: z.number(),
  hospitalizationId: z.string(),
  admissionDate: z.string(),
  date: z.string(),
  patientMrn: z.string().optional(), // Added as optional since it's not in the alasql query
  room: z.string().optional(), // Added as optional since it's not in the alasql query
  assignedProfessionals: z.array(AssignedProfessionalSchema),
});

export type AssignedProfessional = z.infer<typeof AssignedProfessionalSchema>;
export type Visit = z.infer<typeof VisitSchema>;
