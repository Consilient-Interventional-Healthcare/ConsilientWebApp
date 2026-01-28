import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { referenceQueryOptions } from '@/shared/core/query/referenceQueryOptions';
import api from '@/shared/core/api/ApiClient';
import type { VisitEvents } from '@/types/api.generated';
import {
  visitEventTypeVisuals,
  defaultVisuals,
  type EnrichedVisitEventType
} from '@/shared/config/visitEventTypeConfig';

class VisitEventTypeStore {
  readonly keys = {
    all: ['visitEventTypes'] as const,
    list: () => [...this.keys.all, 'list'] as const,
  };

  /**
   * Enriches API event types with local icon/color mappings
   */
  private enrichEventType(dto: VisitEvents.VisitEventTypeDto): EnrichedVisitEventType {
    const visuals = visitEventTypeVisuals[dto.id] ?? defaultVisuals;
    return {
      ...dto,
      iconName: visuals.iconName,
      color: visuals.color,
    };
  }

  useVisitEventTypes(): UseQueryResult<EnrichedVisitEventType[], Error> {
    return useQuery({
      queryKey: this.keys.list(),
      queryFn: async () => {
        const response = await api.get<VisitEvents.VisitEventTypeDto[]>('/visit/event/types');
        return response.data.map((dto) => this.enrichEventType(dto));
      },
      ...referenceQueryOptions,
    });
  }
}

export const visitEventTypeStore = new VisitEventTypeStore();
export const useVisitEventTypes = () => visitEventTypeStore.useVisitEventTypes();
