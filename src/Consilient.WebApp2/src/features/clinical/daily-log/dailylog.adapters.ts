import { GraphQL } from "@/types/api.generated";
import type { DailyLogProviderV2 } from "./dailylog.types";

/**
 * Filters visits by provider ID using the new DailyLogVisit structure
 * A visit matches if its providerIds array contains the specified provider
 */
export function filterVisitsByProviderV2(
  visits: GraphQL.DailyLogVisit[],
  providerId: number | null
): GraphQL.DailyLogVisit[] {
  if (!providerId) return visits;

  return visits.filter((visit) => visit.providerIds?.includes(providerId));
}

/**
 * Gets display name for a patient from DailyLogVisit
 */
export function getPatientDisplayNameV2(visit: GraphQL.DailyLogVisit): string {
  const firstName = visit.patient?.firstName ?? "";
  const lastName = visit.patient?.lastName ?? "";
  return `${lastName}, ${firstName}`.trim().replace(/^,\s*|,\s*$/g, "");
}

/**
 * Gets display name for a provider
 */
export function getProviderDisplayName(provider: DailyLogProviderV2 | null): string {
  if (!provider) return "";
  const firstName = provider.firstName ?? "";
  const lastName = provider.lastName ?? "";
  return `${lastName}, ${firstName}`.trim().replace(/^,\s*|,\s*$/g, "");
}
