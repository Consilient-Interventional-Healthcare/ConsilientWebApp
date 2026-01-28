import type {
  IDailyLogServiceV2,
  DailyLogVisitsResponse,
  StatusChangeEvent,
  DailyLogLogEntry,
  DailyLogLogEntryV2,
} from '../dailylog.types';
import api from '@/shared/core/api/ApiClient';
import type { GraphQL, GraphQl } from '@/types/api.generated';

export class DailyLogService implements IDailyLogServiceV2 {

  getPatientTimelineData(_hospitalizationId: number): Promise<StatusChangeEvent[]> {
    // TODO: Implement real API call
    return Promise.resolve([]);
  }

  getLogEntriesByVisitId(_visitId: number): Promise<DailyLogLogEntry[]> {
    // TODO: Implement real API call
    return Promise.resolve([]);
  }

  insertLogEntry(_visitId: number, _content: string, _userId: number, _eventTypeId: number): Promise<DailyLogLogEntry> {
    // TODO: Implement real API call
    return Promise.reject(new Error('insertLogEntry not yet implemented'));
  }

  // V2 Methods
  async getVisitsByDateV2(date: string, facilityId: number): Promise<DailyLogVisitsResponse> {
    const query = `{
      dailyLogVisits(dateServiced: "${date}", facilityId: ${facilityId}) {
        date
        facilityId
        providers { id firstName lastName type }
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

  getLogEntriesByVisitIdV2(_visitId: number): Promise<DailyLogLogEntryV2[]> {
    // TODO: Implement real API call
    return Promise.resolve([]);
  }

  insertLogEntryV2(_visitId: number, _content: string, _userId: number, _eventTypeId: number): Promise<DailyLogLogEntryV2> {
    // TODO: Implement real API call
    return Promise.reject(new Error('insertLogEntryV2 not yet implemented'));
  }
}
export const dailyLogService = new DailyLogService();