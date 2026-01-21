import api from '@/shared/core/api/ApiClient';
import type { GraphQl } from '@/types/api.generated';
import type { IVisitService, VisitDto } from './IVisitService';

interface GraphQLVisitResponse {
  visits: Array<{
    id: number;
    dateServiced: string;
    room: string;
    bed: string;
    patient: {
      id: number;
      firstName: string;
      lastName: string;
      mrn: string;
    };
    hospitalization: {
      id: number;
      hospitalizationStatusId: number;
      admissionDate: string;
    };
    visitAttendants: Array<{
      provider: {
        id: number;
        firstName: string;
        lastName: string;
        type: 'Physician' | 'NursePractitioner';
      };
    }>;
  }>;
}

class VisitServiceImpl implements IVisitService {
  async getVisits(date: string, facilityId: number): Promise<VisitDto[]> {
    const query = `{
      visits(dateServiced: "${date}", facilityId: ${facilityId}) {
        id
        dateServiced
        room
        bed
        patient {
          id
          firstName
          lastName
          mrn
        }
        hospitalization {
          id
          hospitalizationStatusId
          admissionDate
        }
        visitAttendants {
          provider {
            id
            firstName
            lastName
            type
          }
        }
      }
    }`;

    const response = await api.post<GraphQl.Consilient_Data_GraphQL_QueryResult>(
      '/graphql',
      { query } as GraphQl.Consilient_Data_GraphQL_QueryRequest
    );

    const data = response.data.data as GraphQLVisitResponse | null;
    if (!data?.visits) {
      return [];
    }

    return data.visits.map((visit) => ({
      id: visit.id,
      dateServiced: visit.dateServiced,
      room: visit.room ?? '',
      bed: visit.bed ?? '',
      patient: {
        id: visit.patient.id,
        firstName: visit.patient.firstName,
        lastName: visit.patient.lastName,
        mrn: visit.patient.mrn ?? '',
      },
      hospitalization: {
        id: visit.hospitalization.id,
        hospitalizationStatusId: visit.hospitalization.hospitalizationStatusId,
        admissionDate: visit.hospitalization.admissionDate,
      },
      assignedProfessionals: (visit.visitAttendants ?? []).map((attendant) => ({
        id: attendant.provider.id,
        firstName: attendant.provider.firstName,
        lastName: attendant.provider.lastName,
        role: attendant.provider.type === 'NursePractitioner' ? 'Nurse' : 'Physician',
      })),
    }));
  }
}

export const visitService = new VisitServiceImpl();
