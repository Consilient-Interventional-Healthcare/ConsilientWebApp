import type {
  IDailyLogServiceV2,
  DailyLogVisitsResponse,
  StatusChangeEvent,
  DailyLogLogEntryV2,
} from '../dailylog.types';
import api from '@/shared/core/api/ApiClient';
import type { GraphQL, GraphQl } from '@/types/api.generated';
import { enrichEventType } from '@/shared/config/visitEventTypeConfig';

export class DailyLogService implements IDailyLogServiceV2 {

  getPatientTimelineData(_hospitalizationId: number): Promise<StatusChangeEvent[]> {
    // TODO: Implement real API call
    return Promise.resolve([]);
  }
  async getVisitsByDateV2(date: string, facilityId: number): Promise<DailyLogVisitsResponse> {
    const query = `{
      dailyLogVisits(dateServiced: "${date}", facilityId: ${facilityId}) {
        date
        facilityId
        providers { id firstName lastName type providerType { id code name } }
        visits {
          id
          room
          bed
          hospitalization { id hospitalizationStatusId admissionDate caseId }
          patient { id firstName lastName mrn birthDate }
          providerIds
        }
      }
    }`;

    const response = await api.post<GraphQl.Consilient_Data_GraphQL_QueryResult>('/graphql', { query });
    const data = response.data.data as Pick<GraphQL.Query, 'dailyLogVisits'> | null;
    const result = data?.dailyLogVisits;

    return {
      result: result ?? { date, facilityId, providers: [], visits: [] },
      providers: result?.providers ?? []
    };
  }

  async getLogEntriesByVisitIdV2(visitId: number): Promise<DailyLogLogEntryV2[]> {
    const query = `{
      getLogEntriesByVisitIdV2(visitId: ${visitId}) {
        event { id description enteredByUserId eventOccurredAt eventTypeId visitId }
        eventType { id code name }
        user { firstName lastName role }
      }
    }`;

    const response = await api.post<GraphQl.Consilient_Data_GraphQL_QueryResult>('/graphql', { query });
    const data = response.data.data as Pick<GraphQL.Query, 'getLogEntriesByVisitIdV2'> | null;
    const entries = data?.getLogEntriesByVisitIdV2 ?? [];

    return entries
      .filter((entry): entry is NonNullable<typeof entry> => entry != null && entry.event != null)
      .map(entry => this.mapGraphQLEntryToLocal(entry));
  }

  /**
   * Maps a GraphQL DailyLogLogEntryV2 to the local type
   */
  private mapGraphQLEntryToLocal(entry: GraphQL.DailyLogLogEntryV2): DailyLogLogEntryV2 {
    const event = entry.event!;
    return {
      event: {
        id: event.id,
        visitId: event.visitId,
        eventTypeId: event.eventTypeId,
        enteredByUserId: event.enteredByUserId,
        eventOccurredAt: event.eventOccurredAt ?? '',
        description: event.description ?? '',
      },
      user: {
        firstName: entry.user?.firstName ?? '',
        lastName: entry.user?.lastName ?? '',
        role: entry.user?.role ?? '',
      },
      eventType: enrichEventType(entry.eventType ? {
        id: entry.eventType.id,
        code: entry.eventType.code ?? '',
        name: entry.eventType.name ?? '',
      } : null),
    };
  }

  async insertLogEntryV2(
    visitId: number,
    content: string,
    _userId: number,
    eventTypeId: number
  ): Promise<DailyLogLogEntryV2> {
    const request = {
      visitId,
      eventTypeId,
      eventOccurredAt: new Date().toISOString(),
      description: content,
    };

    // Insert the event via REST
    await api.post(`/visit/${visitId}/event`, request);

    // Refetch entries to get complete data with user info
    // This ensures we have consistent data including user resolution
    const entries = await this.getLogEntriesByVisitIdV2(visitId);

    // Return the most recent entry (just inserted)
    const latestEntry = entries[entries.length - 1];
    if (!latestEntry) {
      throw new Error('Failed to retrieve inserted log entry');
    }

    return latestEntry;
  }
}
export const dailyLogService = new DailyLogService();