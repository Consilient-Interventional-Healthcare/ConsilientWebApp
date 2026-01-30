import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { referenceQueryOptions } from '@/shared/core/query/referenceQueryOptions';
import api from '@/shared/core/api/ApiClient';
import type { BillingCodes } from '@/types/api.generated';

class BillingCodeStore {
  readonly keys = {
    all: ['billingCodes'] as const,
    list: () => [...this.keys.all, 'list'] as const,
  };

  useBillingCodes(): UseQueryResult<BillingCodes.BillingCodeDto[], Error> {
    return useQuery({
      queryKey: this.keys.list(),
      queryFn: async () => {
        const response = await api.get<BillingCodes.BillingCodeDto[]>('/billingcodes');
        return response.data;
      },
      ...referenceQueryOptions,
    });
  }
}

export const billingCodeStore = new BillingCodeStore();
export const useBillingCodes = () => billingCodeStore.useBillingCodes();
