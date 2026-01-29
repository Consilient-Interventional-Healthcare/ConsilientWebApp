import type { GraphQL, VisitEvents } from "@/types/api.generated";
import type { EnrichedVisitEventType } from "@/shared/types/enrichedEntities";

/**
 * Timeline event for patient status changes (used by PatientTimeline component)
 * Custom domain type - no GraphQL equivalent
 */
export interface StatusChangeEvent {
  date: string;
  name: string;
  code: string;
  color: string;
  type: string;
  iconName: string;
}

/**
 * Log entry with enriched event type for visuals
 */
export interface DailyLogLogEntry {
  event: GraphQL.DailyLogEvent;
  user: GraphQL.DailyLogUser;
  eventType: EnrichedVisitEventType | null;
}

/**
 * Service interface aligned with backend endpoints
 */
export interface IDailyLogService {
  getVisitsByDate(date: string, facilityId: number): Promise<GraphQL.DailyLogVisitsResult>;
  getLogEntriesByVisitId(visitId: number): Promise<DailyLogLogEntry[]>;
  insertVisitEvent(visitId: number, request: VisitEvents.InsertVisitEventRequest): Promise<number>;
  getPatientTimelineData(hospitalizationId: number): Promise<StatusChangeEvent[]>;
}
