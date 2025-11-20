import type { ProviderAssignments } from '../types/dailylog.types';

interface DailyLogService {
  getAssignmentsByDate(_date: string): Promise<ProviderAssignments[]>;
}

class DailyLogServiceImpl implements DailyLogService {
  async getAssignmentsByDate(_date: string): Promise<ProviderAssignments[]> {
    // Return mock provider assignments data
    return Promise.resolve([
      {
        providerId: 'provider-1',
        providerFirstName: 'Alice',
        providerLastName: 'Smith',
        assignments: [
          {
            id: 'assignment-1',
            patient: {
              id: 'patient-1',
              firstName: 'John',
              lastName: 'Doe',
              dateOfBirth: '1980-01-01',
              gender: 'male',
            },
            hospitalization: {
              hospitalizationId: 'hosp-1',
              admissionDate: '2025-11-01',
              status: 'active',
            },
          },
          {
            id: 'assignment-2',
            patient: {
              id: 'patient-2',
              firstName: 'Jane',
              lastName: 'Roe',
              dateOfBirth: '1975-05-15',
              gender: 'female',
            },
            hospitalization: {
              hospitalizationId: 'hosp-2',
              admissionDate: '2025-10-20',
              status: 'pending',
            },
          },
        ],
      },
      {
        providerId: 'provider-2',
        providerFirstName: 'Bob',
        providerLastName: 'Lee',
        assignments: [
          {
            id: 'assignment-3',
            patient: {
              id: 'patient-3',
              firstName: 'Sam',
              lastName: 'Green',
              dateOfBirth: '1990-03-22',
              gender: 'other',
            },
            hospitalization: {
              hospitalizationId: 'hosp-3',
              admissionDate: '2025-11-10',
              status: 'completed',
            },
          },
        ],
      },
    ]);
  }
}

export const dailyLogService: DailyLogService = new DailyLogServiceImpl();