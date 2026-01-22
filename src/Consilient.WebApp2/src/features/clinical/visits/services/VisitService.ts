import api from '@/shared/core/api/ApiClient';
import type { GraphQl, GraphQL } from '@/types/api.generated';
import type { IVisitService } from './IVisitService';

class VisitServiceImpl implements IVisitService {
  async getVisits(date: string, facilityId: number): Promise<GraphQL.Visit[]> {
    const query = `{
      visits(dateServiced: "${date}", facilityId: ${facilityId}) {
        id
        dateServiced
        room
        bed
        isScribeServiceOnly
        patient {
          id
          firstName
          lastName
          mrn
          birthDate
        }
        hospitalization {
          id
          hospitalizationStatusId
          admissionDate
          dischargeDate
          facilityId
          patientId
          caseId
          psychEvaluation
        }
        visitAttendants {
          id
          visitId
          providerId
          provider {
            id
            firstName
            lastName
            type
            email
            employeeId
            titleExtension
          }
        }
      }
    }`;

    const response = await api.post<GraphQl.Consilient_Data_GraphQL_QueryResult>(
      '/graphql',
      { query } as GraphQl.Consilient_Data_GraphQL_QueryRequest
    );

    const data = response.data.data as Pick<GraphQL.Query, 'visits'> | null;
    return data?.visits ?? [];
  }
}

export const visitService = new VisitServiceImpl();
