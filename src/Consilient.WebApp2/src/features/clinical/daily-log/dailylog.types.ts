import type { Patient, Provider, Hospitalization, LogEntry } from "@/types/db.types";

export { Patient, Provider, Hospitalization, LogEntry };

export interface DailyLogVisit {
  id: number;
  patientId: number;
  patientLastName: string;
  patientFirstName: string;
  patientDateOfBirth: string;
  patientGender: string;
  patientMRN: string;
  hospitalizationId: number;
  hospitalizationAdmissionDate: string;
  hospitalizationStatusId: number;
  providerId: number;
  providerFirstName: string;
  providerLastName: string;
}

export interface IDailyLogService {
  getVisitsByDate(_date: string): Promise<DailyLogVisit[]>;
  getLogEntriesByVisitId(_visitId: number): Promise<LogEntry[]>;
  insertLogEntry(_visitId: number, _content: string, _userId: number, _type: string): Promise<LogEntry>
}
export interface LogEntryType {
  label: string;
  value: string;
  icon: string;
  color: string;
}
export interface ILogEntryTypeProvider {
  getLogEntryType(): string;
}
export interface DailyLogLogEntry extends LogEntry {
  userFirstName: string;
  userLastName: string;
}
export const logEntryTypes: LogEntryType[] = [
  { label: 'Interval', value: 'interval', icon: 'fa-bed', color: '#1976d2' },      // Represents overnight events
  { label: 'Interview', value: 'interview', icon: 'fa-user-md', color: '#43a047' }, // Patient-doctor interaction
  { label: 'Plan', value: 'plan', icon: 'fa-notes-medical', color: '#fbc02d' },     // Decisions made by doctor
];