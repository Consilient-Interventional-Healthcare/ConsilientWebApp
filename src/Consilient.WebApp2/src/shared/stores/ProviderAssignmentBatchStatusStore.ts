import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { referenceQueryOptions } from '@/shared/core/query/referenceQueryOptions';
import api from '@/shared/core/api/ApiClient';
import type { Assignments } from '@/types/api.generated';
import { getEntityVisuals } from '@/shared/config/enumVisualsConfig';
import type { EnrichedProviderAssignmentBatchStatus } from '@/shared/types/enrichedEntities';

class ProviderAssignmentBatchStatusStore {
  readonly keys = {
    all: ['providerAssignmentBatchStatuses'] as const,
    list: () => [...this.keys.all, 'list'] as const,
  };

  useProviderAssignmentBatchStatuses(): UseQueryResult<EnrichedProviderAssignmentBatchStatus[], Error> {
    return useQuery({
      queryKey: this.keys.list(),
      queryFn: async () => {
        const response = await api.get<Assignments.ProviderAssignmentBatchStatusDto[]>('/assignments/batch-statuses');
        return response.data.map((dto): EnrichedProviderAssignmentBatchStatus => ({
          ...dto,
          ...getEntityVisuals('providerAssignmentBatchStatus', dto.value),
        }));
      },
      ...referenceQueryOptions,
    });
  }
}

export const providerAssignmentBatchStatusStore = new ProviderAssignmentBatchStatusStore();
export const useProviderAssignmentBatchStatuses = () => providerAssignmentBatchStatusStore.useProviderAssignmentBatchStatuses();
