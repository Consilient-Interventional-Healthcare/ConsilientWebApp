import api from '@/shared/core/api/ApiClient';
import type { Assignments, GraphQl, GraphQL } from '@/types/api.generated';

export type ProviderAssignmentBatch = GraphQL.ProviderAssignmentBatch;
export type ProviderAssignment = GraphQL.ProviderAssignment;
export type BatchStatus = GraphQL.ProviderAssignmentBatchStatus;

class ProviderAssignmentsService {
  private readonly baseUrl = '/assignments';

  async uploadFile(file: File, serviceDate: string, facilityId: number): Promise<Assignments.ImportProviderAssignmentResult> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('serviceDate', serviceDate);
    formData.append('facilityId', facilityId.toString());

    // Remove Content-Type to let browser set multipart/form-data with boundary
    const response = await api.post<Assignments.ImportProviderAssignmentResult>(`${this.baseUrl}/upload`, formData, {
      headers: { 'Content-Type': null as unknown as string }
    });
    return response.data;
  }

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
          visit { room bed }
          resolvedPatientId
          resolvedPhysicianId
          resolvedNursePractitionerId
          resolvedHospitalizationId
          resolvedHospitalizationStatusId
          hospitalizationStatus { code }
          imported
          shouldImport
          validationErrorsJson
        }
      }
    }`;

    const response = await api.post<GraphQl.Consilient_Data_GraphQL_QueryResult>(
      '/graphql',
      { query } as GraphQl.Consilient_Data_GraphQL_QueryRequest
    );

    const data = response.data.data as Pick<GraphQL.Query, 'providerAssignmentBatch'> | null;
    return data?.providerAssignmentBatch ?? null;
  }

  async processBatch(batchId: string): Promise<string> {
    const response = await api.post<string>(`${this.baseUrl}/process/${batchId}`);
    return response.data;
  }
}

export const providerAssignmentsService = new ProviderAssignmentsService();
