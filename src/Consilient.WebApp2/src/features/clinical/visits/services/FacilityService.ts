import api from '@/shared/core/api/ApiClient';
import type { Facilities } from '@/types/api.generated';

class FacilityService {
  private readonly baseUrl = '/facilities';

  async getAll(): Promise<Facilities.FacilityDto[]> {
    const response = await api.get<Facilities.FacilityDto[]>(this.baseUrl);
    return response.data;
  }
}

export const facilityService = new FacilityService();
