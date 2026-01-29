import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { referenceQueryOptions } from '@/shared/core/query/referenceQueryOptions';
import api from '@/shared/core/api/ApiClient';
import type { Hospitalizations } from '@/types/api.generated';
import { getEntityVisuals } from '@/shared/config/enumVisualsConfig';
import type { EnrichedHospitalizationStatus } from '@/shared/types/enrichedEntities';

class HospitalizationStatusStore {
  readonly keys = {
    all: ['hospitalizationStatuses'] as const,
    list: () => [...this.keys.all, 'list'] as const,
  };

  useHospitalizationStatuses(): UseQueryResult<EnrichedHospitalizationStatus[], Error> {
    return useQuery({
      queryKey: this.keys.list(),
      queryFn: async () => {
        const response = await api.get<Hospitalizations.HospitalizationStatusDto[]>('/hospitalizations/statuses');
        return response.data.map((dto): EnrichedHospitalizationStatus => {
          const visuals = getEntityVisuals('hospitalizationStatus', dto.id);
          return {
            ...dto,
            // Config overrides API values if present, otherwise use API values
            iconName: visuals.iconName ?? dto.iconName ?? '',
            color: visuals.color ?? dto.color,
            // Only include className if it has a value (for exactOptionalPropertyTypes)
            ...(visuals.className && { className: visuals.className }),
          };
        });
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
