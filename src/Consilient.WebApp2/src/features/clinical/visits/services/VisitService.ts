import api from '@/shared/core/api/ApiClient';
import type { Visit, VisitService } from '../types/visit.types';

export class VisitServiceImpl implements VisitService {
  private readonly baseUrl = '/visits';

  async getVisits(date: string, facilityId: number): Promise<Visit[]> {
    const response = await api.get<Visit[]>(this.baseUrl, {
      params: { date, facilityId }
    });
    return response.data;
  }
}

export const visitService = new VisitServiceImpl();
