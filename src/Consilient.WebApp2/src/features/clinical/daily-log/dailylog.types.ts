import type { VisitEvents } from "@/types/api.generated";

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
  markers: DailyLogVisitPhaseMarker[];
}

export interface DailyLogVisitPhaseMarker {
  iconName: string;
  color: string;
  hasData: boolean
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
  insertLogEntry(_visitId: number, _content: string, _userId: number, _eventTypeId: number): Promise<DailyLogLogEntry>;
  getPatientTimelineData(_hospitalizationId: number): Promise<StatusChangeEvent[]>;
}

export interface ILogEntryTypeProvider {
  getLogEntryType(): string;
}
export interface DailyLogLogEntry extends VisitEvents.VisitEventDto {
  userFirstName: string;
  userLastName: string;
  userRole: string;
}
