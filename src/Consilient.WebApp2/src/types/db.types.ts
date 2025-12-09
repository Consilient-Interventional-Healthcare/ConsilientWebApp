// export interface HospitalizationStatus {
//   id: number;
//   code: string;
//   name: string;
//   billingCode?: string;
//   color: string;
//   order: number;
// }

// export interface LogEntryType {
//   label: string;
//   value: string;
//   color: string;
//   icon: string;
// }
export interface MockExternalProvider {
  providerName: string;
  providerKey: string;
}

export interface MockUser {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  role: string;
  externalProviders: MockExternalProvider[];
}

export interface CurrentUser {
  id: string;
  userName: string;
  email: string;
}

// export interface Visit {
//   id: number;
//   patientId: number;
//   hospitalizationId: number;
//   date: string;
//   room: string;
// }

// export interface AssignedProfessional {
//   visitId: number;
//   providerId: number;
// }

// export interface Provider {
//   id: number;
//   firstName: string;
//   lastName: string;
//   role: string;
// }


import type * as api from "@/types/api.generated";
export interface DbSchema {
  users: MockUser[];
  patients: api.Patients.PatientDto[];
  hospitalizations: api.Hospitalizations.HospitalizationDto[];
  visits: api.Visits.VisitDto[];
  assignedProfessionals: api.AssignedProfessionals.AssignedProfessionalDto[];
  providers: api.Providers.ProviderDto[];
  visitEvents: api.VisitEvents.VisitEventDto[];
  hospitalizationStatuses: api.Hospitalizations.HospitalizationStatusDto[];
  visitEventTypes: api.VisitEvents.VisitEventTypeDto[];
}