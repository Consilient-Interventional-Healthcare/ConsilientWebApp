export interface HospitalizationStatus {
  id: number;
  code: string;
  name: string;
  billingCode?: string;
  color: string;
  order: number;
}

export interface LogEntryType {
  label: string;
  value: string;
  icon: string;
  color: string;
  icon: string;
}

export interface ExternalProvider {
  providerName: string;
  providerKey: string;
}

export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  externalProviders: ExternalProvider[];
}

export interface Patient {
  id: number;
  mrn: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
}

export interface Hospitalization {
  id: number;
  patientId: number;
  admissionDate: string;
  hospitalizationStatusId: number;
  facilityId: number;
}

export interface Visit {
  id: number;
  patientId: number;
  hospitalizationId: number;
  date: string;
  room: string;
}

export interface AssignedProfessional {
  visitId: number;
  providerId: number;
}

export interface Provider {
  id: number;
  firstName: string;
  lastName: string;
  role: string;
}

export interface LogEntry {
  id: number;
  timestamp: string;
  visitId: number;
  userId: number;
  message: string;
  type: string;
}

export interface DbSchema {
  users: User[];
  patients: Patient[];
  hospitalizations: Hospitalization[];
  visits: Visit[];
  assignedProfessionals: AssignedProfessional[];
  providers: Provider[];
  logEntries: LogEntry[];
  hospitalizationStatuses: HospitalizationStatus[];
  logEntryTypes: LogEntryType[];
}