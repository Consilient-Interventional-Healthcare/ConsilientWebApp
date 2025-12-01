import type { Patient, Provider, Hospitalization, LogEntry, LogEntryType } from "@/types/db.types";

export { Patient, Provider, Hospitalization, LogEntry, LogEntryType };

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
  room: string;
}

export interface StatusChangeEvent {
  date: string;
  name: string;
  code: string;
  color: string;
  type: string;
  iconName: string;
}


export interface IDailyLogService {
  getVisitsByDate(_date: string): Promise<DailyLogVisit[]>;
  getLogEntriesByVisitId(_visitId: number): Promise<DailyLogLogEntry[]>;
  insertLogEntry(_visitId: number, _content: string, _userId: number, _type: string): Promise<DailyLogLogEntry>;
  getPatientTimelineData(_hospitalizationId: number): Promise<StatusChangeEvent[]>;
}

export interface ILogEntryTypeProvider {
  getLogEntryType(): string;
}
export interface DailyLogLogEntry extends LogEntry {
  userFirstName: string;
  userLastName: string;
  userRole: string;
}
