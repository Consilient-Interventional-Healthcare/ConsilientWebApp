import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { referenceQueryOptions } from '@/shared/core/query/referenceQueryOptions';
import api from '@/shared/core/api/ApiClient';
import type { Assignments } from '@/types/api.generated';

class ProviderAssignmentBatchStatusStore {
  readonly keys = {
    all: ['providerAssignmentBatchStatuses'] as const,
    list: () => [...this.keys.all, 'list'] as const,
  };

  useProviderAssignmentBatchStatuses(): UseQueryResult<Assignments.ProviderAssignmentBatchStatusDto[], Error> {
    return useQuery({
      queryKey: this.keys.list(),
      queryFn: async () => {
        const response = await api.get<Assignments.ProviderAssignmentBatchStatusDto[]>('/assignments/batch-statuses');
        return response.data;
      },
      ...referenceQueryOptions,
    });
  }
}

export const providerAssignmentBatchStatusStore = new ProviderAssignmentBatchStatusStore();
export const useProviderAssignmentBatchStatuses = () => providerAssignmentBatchStatusStore.useProviderAssignmentBatchStatuses();
