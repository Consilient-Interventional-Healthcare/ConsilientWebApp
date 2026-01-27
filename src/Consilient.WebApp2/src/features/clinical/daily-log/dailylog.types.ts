import type { VisitEvents, GraphQL } from "@/types/api.generated";

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

// ============================================================================
// V2 Types - Using api.generated.ts types
// These types wrap the generated GraphQL types for the DailyLog refactoring
// ============================================================================

/**
 * Response from getVisitsByDateV2 containing visits and providers
 */
export interface DailyLogVisitsResponse {
  result: GraphQL.DailyLogVisitsResult;
  providers: GraphQL.DailyLogProvider[];
}

/**
 * Helper type for provider from the new DailyLog query
 */
export type DailyLogProviderV2 = GraphQL.DailyLogProvider;

/**
 * V2 Service interface using generated types
 */
export interface IDailyLogServiceV2 {
  getVisitsByDateV2(date: string, facilityId: number): Promise<DailyLogVisitsResponse>;
  getLogEntriesByVisitIdV2(visitId: number): Promise<DailyLogLogEntryV2[]>;
  insertLogEntryV2(
    visitId: number,
    content: string,
    userId: number,
    eventTypeId: number
  ): Promise<DailyLogLogEntryV2>;
  getPatientTimelineData(hospitalizationId: number): Promise<StatusChangeEvent[]>;
}

/**
 * V2 Log Entry type - composition instead of extension
 * Used in Stage 2 refactoring (center column)
 */
export interface DailyLogLogEntryV2 {
  event: VisitEvents.VisitEventDto;
  user: {
    firstName: string;
    lastName: string;
    role: string;
  };
  eventType: VisitEvents.VisitEventTypeDto | null;
}
