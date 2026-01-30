import api from '@/shared/core/api/ApiClient';
import type { VisitServiceBillings } from '@/types/api.generated';

class VisitServiceBillingService {
  async create(visitId: number, request: Omit<VisitServiceBillings.CreateVisitServiceBillingRequest, 'visitId'>): Promise<number> {
    const response = await api.post<number>(`/visit/${visitId}/servicebilling`, request);
    return response.data;
  }

  async delete(visitId: number, id: number): Promise<void> {
    await api.delete(`/visit/${visitId}/servicebilling/${id}`);
  }
}

export const visitServiceBillingService = new VisitServiceBillingService();
