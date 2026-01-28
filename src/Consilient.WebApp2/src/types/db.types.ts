import type * as api from "@/types/api.generated";
export interface DbSchema {
  patients: api.Patients.PatientDto[];
  hospitalizations: api.Hospitalizations.HospitalizationDto[];
  visits: api.Visits.VisitDto[];
  assignedProfessionals: { visitId: number; providerId: number }[];
  providers: api.GraphQL.Provider[];
  visitEvents: api.VisitEvents.VisitEventDto[];
  hospitalizationStatuses: api.Hospitalizations.HospitalizationStatusDto[];
}