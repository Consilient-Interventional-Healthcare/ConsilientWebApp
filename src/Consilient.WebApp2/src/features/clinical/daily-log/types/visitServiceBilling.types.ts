// Types for VisitServiceBilling feature
// Some types are temporary until GraphQL backend (Phase 2) is implemented

import type { GraphQL } from '@/types/api.generated';

// Re-export generated types for convenience
export type { BillingCodes, VisitServiceBillings } from '@/types/api.generated';

// TEMPORARY: GraphQL type for service billing info (until Phase 2 backend adds to schema)
export interface VisitServiceBillingInfo {
  id: number;
  serviceTypeId: number;
  serviceTypeCode: string;
  serviceTypeName: string;
  billingCodeId: number;
  billingCodeCode: string;
  billingCodeDescription: string;
}

// TEMPORARY: Extend DailyLogVisit until GraphQL schema is updated in Phase 2
export interface DailyLogVisitWithBillings extends GraphQL.DailyLogVisit {
  serviceBillings?: VisitServiceBillingInfo[];
}
