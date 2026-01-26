import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { referenceQueryOptions } from '@/shared/core/query/referenceQueryOptions';
import api from '@/shared/core/api/ApiClient';
import type { ServiceTypes } from '@/types/api.generated';

class ServiceTypeStore {
  readonly keys = {
    all: ['serviceTypes'] as const,
    list: () => [...this.keys.all, 'list'] as const,
  };

  useServiceTypes(): UseQueryResult<ServiceTypes.ServiceTypeDto[], Error> {
    return useQuery({
      queryKey: this.keys.list(),
      queryFn: async () => {
        const response = await api.get<ServiceTypes.ServiceTypeDto[]>('/servicetypes');
        return response.data;
      },
      ...referenceQueryOptions,
    });
  }
}

export const serviceTypeStore = new ServiceTypeStore();
export const useServiceTypes = () => serviceTypeStore.useServiceTypes();
