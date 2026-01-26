import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { referenceQueryOptions } from '@/shared/core/query/referenceQueryOptions';
import { facilityService } from '@/features/clinical/visits/services/FacilityService';
import type { Facilities } from '@/types/api.generated';

class FacilityStore {
  readonly keys = {
    all: ['facilities'] as const,
    list: () => [...this.keys.all, 'list'] as const,
  };

  useFacilities(): UseQueryResult<Facilities.FacilityDto[], Error> {
    return useQuery({
      queryKey: this.keys.list(),
      queryFn: () => facilityService.getAll(),
      ...referenceQueryOptions,
    });
  }
}

export const facilityStore = new FacilityStore();
export const useFacilities = () => facilityStore.useFacilities();
