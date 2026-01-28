import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { referenceQueryOptions } from '@/shared/core/query/referenceQueryOptions';
import api from '@/shared/core/api/ApiClient';
import type { Hospitalizations } from '@/types/api.generated';

class HospitalizationStatusStore {
  readonly keys = {
    all: ['hospitalizationStatuses'] as const,
    list: () => [...this.keys.all, 'list'] as const,
  };

  useHospitalizationStatuses(): UseQueryResult<Hospitalizations.HospitalizationStatusDto[], Error> {
    return useQuery({
      queryKey: this.keys.list(),
      queryFn: async () => {
        const response = await api.get<Hospitalizations.HospitalizationStatusDto[]>('/hospitalizations/statuses');
        return response.data;
      },
      ...referenceQueryOptions,
    });
  }
}

export const hospitalizationStatusStore = new HospitalizationStatusStore();
export const useHospitalizationStatuses = () => hospitalizationStatusStore.useHospitalizationStatuses();

export const useHospitalizationStatusById = (id: number | undefined) => {
  const { data: statuses = [] } = useHospitalizationStatuses();
  return statuses.find(s => s.id === id) ?? null;
};
