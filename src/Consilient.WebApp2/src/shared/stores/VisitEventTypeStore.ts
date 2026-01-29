import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { referenceQueryOptions } from '@/shared/core/query/referenceQueryOptions';
import api from '@/shared/core/api/ApiClient';
import type { VisitEvents } from '@/types/api.generated';
import { getEntityVisuals } from '@/shared/config/enumVisualsConfig';
import type { EnrichedVisitEventType } from '@/shared/types/enrichedEntities';

class VisitEventTypeStore {
  readonly keys = {
    all: ['visitEventTypes'] as const,
    list: () => [...this.keys.all, 'list'] as const,
  };

  useVisitEventTypes(): UseQueryResult<EnrichedVisitEventType[], Error> {
    return useQuery({
      queryKey: this.keys.list(),
      queryFn: async () => {
        const response = await api.get<VisitEvents.VisitEventTypeDto[]>('/visit/event/types');
        return response.data.map((dto): EnrichedVisitEventType => ({
          ...dto,
          ...getEntityVisuals('visitEventType', dto.id),
        }));
      },
      ...referenceQueryOptions,
    });
  }
}

export const visitEventTypeStore = new VisitEventTypeStore();
export const useVisitEventTypes = () => visitEventTypeStore.useVisitEventTypes();
