import type {
  IDailyLogService,
  StatusChangeEvent,
  DailyLogLogEntry,
} from './IDailyLogService';
import api from '@/shared/core/api/ApiClient';
import type { GraphQL, GraphQl, VisitEvents } from '@/types/api.generated';
import { getEntityVisuals } from '@/shared/config/enumVisualsConfig';
import type { EnrichedVisitEventType } from '@/shared/types/enrichedEntities';

export class DailyLogService implements IDailyLogService {

  getPatientTimelineData(_hospitalizationId: number): Promise<StatusChangeEvent[]> {
    // TODO: Implement real API call
    return Promise.resolve([]);
  }

  async getVisitsByDate(date: string, facilityId: number): Promise<GraphQL.DailyLogVisitsResult> {
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
          patient { id firstName lastName mrn birthDate gender }
          providerIds
          serviceBillings {
            id
            serviceTypeId
            serviceTypeCode
            serviceTypeName
            billingCodeId
            billingCodeCode
            billingCodeDescription
          }
        }
      }
    }`;

    const response = await api.post<GraphQl.Consilient_Data_GraphQL_QueryResult>('/graphql', { query });
    const data = response.data.data as Pick<GraphQL.Query, 'dailyLogVisits'> | null;

    return data?.dailyLogVisits ?? { date, facilityId, providers: [], visits: [] };
  }

  async getLogEntriesByVisitId(visitId: number): Promise<DailyLogLogEntry[]> {
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
   * Maps a GraphQL DailyLogLogEntryV2 to the local DailyLogLogEntry type
   * Enriches eventType with visual properties (color, iconName)
   */
  private mapGraphQLEntryToLocal(entry: GraphQL.DailyLogLogEntryV2): DailyLogLogEntry {
    return {
      event: entry.event!,
      user: entry.user ?? { firstName: '', lastName: '', role: '' },
      eventType: entry.eventType ? {
        id: entry.eventType.id,
        code: entry.eventType.code ?? '',
        name: entry.eventType.name ?? '',
        ...getEntityVisuals('visitEventType', entry.eventType.id),
      } as EnrichedVisitEventType : null,
    };
  }

  async insertVisitEvent(
    visitId: number,
    request: VisitEvents.InsertVisitEventRequest
  ): Promise<number> {
    const response = await api.post<number>(`/visit/${visitId}/event`, request);
    return response.data;
  }
}
export const dailyLogService = new DailyLogService();
