import type { IDailyLogService, DailyLogVisit, StatusChangeEvent, DailyLogLogEntry } from '../dailylog.types';

export class DailyLogService implements IDailyLogService {

  getPatientTimelineData(_hospitalizationId: number): Promise<StatusChangeEvent[]> {
    throw new Error('Method not implemented.');
  }

  getVisitsByDate(_date: string): Promise<DailyLogVisit[]> {
    throw new Error('Not implemented yet');
  }

  getLogEntriesByVisitId(_visitId: number): Promise<DailyLogLogEntry[]> {
    throw new Error('Not implemented yet');
  }

  insertLogEntry(_visitId: number, _content: string, _userId: number, _type: string): Promise<DailyLogLogEntry> {
    throw new Error('Not implemented yet');
  }
}
export const dailyLogService = new DailyLogService();