/**
 * Local configuration for VisitEventType visual properties.
 * Icons and colors are not provided by the API, so we define them here.
 * Keyed by event type id.
 */
export interface VisitEventTypeVisuals {
  iconName: string;
  color: string;
}

export const visitEventTypeVisuals: Record<number, VisitEventTypeVisuals> = {
  1: { iconName: 'fa-bed', color: '#1976d2' },       // Interval
  2: { iconName: 'fa-user-md', color: '#43a047' },  // Interview
  3: { iconName: 'fa-notes-medical', color: '#fbc02d' }, // Plan
};

// Default visuals for unknown event types
export const defaultVisuals: VisitEventTypeVisuals = {
  iconName: 'fa-circle',
  color: '#9e9e9e',
};

/**
 * Enriched VisitEventType with guaranteed icon and color values
 */
export interface EnrichedVisitEventType {
  id: number;
  code: string;
  name: string;
  iconName: string;
  color: string;
}

/**
 * Enriches an event type from the API with local icon/color config.
 * Returns null if input is null/undefined.
 */
export function enrichEventType(
  dto: { id: number; code: string; name: string; iconName?: string | null; color?: string | null } | null | undefined
): EnrichedVisitEventType | null {
  if (!dto) return null;

  const visuals = visitEventTypeVisuals[dto.id] ?? defaultVisuals;
  return {
    id: dto.id,
    code: dto.code,
    name: dto.name,
    iconName: visuals.iconName,
    color: visuals.color,
  };
}
