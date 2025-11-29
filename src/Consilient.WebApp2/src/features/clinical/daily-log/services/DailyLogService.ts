import type { IDailyLogService, DailyLogVisit, LogEntry } from '../dailylog.types';

export class DailyLogService implements IDailyLogService {
  getVisitsByDate(_date: string): Promise<DailyLogVisit[]> {
    throw new Error('Not implemented yet');
  }

  getLogEntriesByVisitId(_visitId: number): Promise<LogEntry[]> {
    throw new Error('Not implemented yet');
  }

  insertLogEntry(_visitId: number, _content: string, _userId: number, _type: string): Promise<LogEntry> {
    throw new Error('Not implemented yet');
  }
}
export const dailyLogService = new DailyLogService();