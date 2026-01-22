import type { GraphQL } from '@/types/api.generated';

export interface IVisitService {
  getVisits(date: string, facilityId: number): Promise<GraphQL.Visit[]>;
}
