import type { IDailyLogService, LogEntry, DailyLogVisit, DailyLogLogEntry } from '../dailylog.types';
import { dataProvider } from '@/data/DataProvider';

export class DailyLogServiceMock implements IDailyLogService {

  getVisitsByDate(date: string): Promise<DailyLogVisit[]> {
    const rows = dataProvider.query<DailyLogVisit>(`
      SELECT 
        visits.id AS id,
        visits.room AS room,
        patients.id AS patientId,
        patients.lastName AS patientLastName,
        patients.firstName AS patientFirstName,
        patients.dateOfBirth AS patientDateOfBirth,
        patients.gender AS patientGender,
        patients.mrn AS patientMRN,
        hospitalizations.id AS hospitalizationId,
        hospitalizations.admissionDate AS hospitalizationAdmissionDate,
        hospitalizations.hospitalizationStatusId AS hospitalizationStatusId,
        providers.id AS providerId,
        providers.firstName AS providerFirstName,
        providers.lastName AS providerLastName
      FROM visits
        JOIN patients ON visits.patientId = patients.id
        JOIN hospitalizations ON visits.hospitalizationId = hospitalizations.id
        JOIN assignedProfessionals ON assignedProfessionals.visitId = visits.id
        JOIN providers ON assignedProfessionals.providerId = providers.id
      WHERE providers.role = 'Physician'
    `, [date]);
    return Promise.resolve(rows);
  }

  getLogEntriesByVisitId(_visitId: number): Promise<DailyLogLogEntry[]> {
    const rows = dataProvider.query<DailyLogLogEntry>(`SELECT 
      id,
      timestamp,
      visitId,
      providerId,
      userId,
      message,
      type,
      users.firstName AS userFirstName,
      users.lastName AS userLastName
    FROM logEntries
    JOIN users ON logEntries.userId = users.id
    WHERE visitId = ?`, [_visitId]);
    return Promise.resolve(rows);
  }

  insertLogEntry(_visitId: number, _content: string, _userId: number, _type: string): Promise<LogEntry> {
    const newEntry: LogEntry = {
      id: Math.floor(Math.random() * 10000),
      timestamp: new Date().toISOString(),
      visitId: _visitId,
      userId: _userId,
      message: _content,
      type: _type
    };
    dataProvider.insert('logEntries', newEntry);
    return Promise.resolve(newEntry);
  }
}
