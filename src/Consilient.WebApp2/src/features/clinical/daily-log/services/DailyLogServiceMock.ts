import type { IDailyLogService, LogEntry, DailyLogVisit, DailyLogLogEntry, StatusChangeEvent } from '../dailylog.types';
import { dataProvider } from '@/data/DataProvider';

export class DailyLogServiceMock implements IDailyLogService {

  getPatientTimelineData(_hospitalizationId: number): Promise<StatusChangeEvent[]> {
    const rows = dataProvider.query<StatusChangeEvent>(`
      SELECT
        statusChanges.date AS date,
        hospitalizationStatuses.name AS name,
        hospitalizationStatuses.code AS code,
        hospitalizationStatuses.color AS color,
        hospitalizationStatuses.type AS type,
        hospitalizationStatuses.iconName AS iconName
      FROM statusChanges
      INNER JOIN hospitalizationStatuses ON statusChanges.hospitalizationStatusId = hospitalizationStatuses.id
      WHERE statusChanges.hospitalizationId = ?
    `, [_hospitalizationId]);
    return Promise.resolve(rows);
  }

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
      users.lastName AS userLastName,
      users.role AS userRole
    FROM logEntries
    JOIN users ON logEntries.userId = users.id
    WHERE visitId = ?`, [_visitId]);
    return Promise.resolve(rows);
  }

  insertLogEntry(_visitId: number, _content: string, _userId: number, _type: string): Promise<DailyLogLogEntry> {
    const newEntry: LogEntry = {
      id: Math.floor(Math.random() * 10000),
      timestamp: new Date().toISOString(),
      visitId: _visitId,
      userId: _userId,
      message: _content,
      type: _type
    };
    const p = dataProvider.insert('logEntries', newEntry);
    return Promise.resolve(p);
  }
}
