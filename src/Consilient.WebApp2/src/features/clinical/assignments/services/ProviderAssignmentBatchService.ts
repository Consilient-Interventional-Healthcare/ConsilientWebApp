import api from '@/shared/core/api/ApiClient';
import type { GraphQl } from '@/types/api.generated';

export type BatchStatus = 'Pending' | 'Processed' | 'Resolved' | 'Imported';

export interface ProviderAssignmentBatch {
  batchId: string;
  date: string;
  facilityId: number;
  status: BatchStatus;
  items: ProviderAssignment[];
}

export interface ProviderAssignment {
  id: number;
  patient: { firstName: string | null; lastName: string | null; mrn: string | null } | null;
  physician: { firstName: string | null; lastName: string | null } | null;
  nursePractitioner: { firstName: string | null; lastName: string | null } | null;
  hospitalization: { caseId: number | null } | null;
  visit: { room: string | null; bed: string | null; imported: boolean } | null;
  resolvedPatientId: number | null;
  resolvedPhysicianId: number | null;
  resolvedNursePractitionerId: number | null;
  resolvedHospitalizationId: number | null;
}

interface GraphQLBatchResponse {
  providerAssignmentBatch: ProviderAssignmentBatch | null;
}

class ProviderAssignmentBatchService {
  async getBatch(batchId: string): Promise<ProviderAssignmentBatch | null> {
    const query = `{
      providerAssignmentBatch(batchId: "${batchId}") {
        batchId
        date
        facilityId
        status
        items {
          id
          patient { firstName lastName mrn }
          physician { firstName lastName }
          nursePractitioner { firstName lastName }
          hospitalization { caseId }
          visit { room bed imported }
          resolvedPatientId
          resolvedPhysicianId
          resolvedNursePractitionerId
          resolvedHospitalizationId
        }
      }
    }`;

    const response = await api.post<GraphQl.Consilient_Data_GraphQL_QueryResult>(
      '/graphql',
      { query } as GraphQl.Consilient_Data_GraphQL_QueryRequest
    );

    const data = response.data.data as GraphQLBatchResponse | null;
    return data?.providerAssignmentBatch ?? null;
  }
}

export const providerAssignmentBatchService = new ProviderAssignmentBatchService();
