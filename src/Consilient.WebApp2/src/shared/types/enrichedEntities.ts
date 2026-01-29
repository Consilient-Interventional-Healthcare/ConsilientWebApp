import type { VisitEvents, Hospitalizations, Assignments } from '@/types/api.generated';

/**
 * Visual properties that can be configured at runtime for any entity.
 * These are loaded from public/enumVisuals.json and can be changed without rebuild.
 */
export interface EntityVisuals {
  iconName?: string;
  color?: string;
  className?: string;
}

/**
 * Enriched entity types - API DTO + visual properties
 * These types are used after entities are fetched from the API and enriched with visuals.
 */

/**
 * VisitEventType enriched with visual properties (icon, color, className)
 */
export interface EnrichedVisitEventType extends VisitEvents.VisitEventTypeDto, EntityVisuals {}

/**
 * HospitalizationStatus enriched with visual properties.
 * Note: API already provides color/iconName, but config can override them.
 */
export interface EnrichedHospitalizationStatus extends Hospitalizations.HospitalizationStatusDto {
  // Ensure iconName is always a string (not null/undefined)
  iconName: string;
  // className is optional, provided by config
  className?: string;
}

/**
 * ProviderType enriched with visual properties.
 * Note: ProviderType will become an API entity in the future.
 */
export interface EnrichedProviderType {
  id: number;
  code: string;
  name: string;
  iconName?: string;
  color?: string;
  className?: string;
}

/**
 * ProviderAssignmentBatchStatus enriched with visual properties.
 */
export interface EnrichedProviderAssignmentBatchStatus extends Assignments.ProviderAssignmentBatchStatusDto, EntityVisuals {}
