import type { EntityVisuals } from '@/shared/types/enrichedEntities';

// --- Types ---

/**
 * The entity types that can be configured with visuals.
 */
export type EntityType = 'visitEventType' | 'hospitalizationStatus' | 'providerType' | 'providerAssignmentBatchStatus';

/**
 * Structure of the enumVisuals.json configuration file.
 */
export interface EnumVisualsConfig {
  visitEventTypes: Record<string, EntityVisuals>;
  hospitalizationStatuses: Record<string, EntityVisuals>;
  providerTypes: Record<string, EntityVisuals>;
  providerAssignmentBatchStatuses: Record<string, EntityVisuals>;
  defaults: Record<EntityType, EntityVisuals>;
}

// --- Module State ---

let configCache: EnumVisualsConfig | null = null;

/**
 * Default configuration used when enumVisuals.json fails to load.
 */
const defaultConfig: EnumVisualsConfig = {
  visitEventTypes: {},
  hospitalizationStatuses: {},
  providerTypes: {},
  providerAssignmentBatchStatuses: {},
  defaults: {
    visitEventType: { iconName: 'fa-circle', color: '#9e9e9e' },
    hospitalizationStatus: { color: '#9e9e9e' },
    providerType: { color: '#9e9e9e' },
    providerAssignmentBatchStatus: { color: '#9e9e9e' },
  },
};

// --- Public API ---

/**
 * Loads the enum visuals configuration from /enumVisuals.json.
 * Call this once during app initialization (in main.tsx).
 * The config is cached for subsequent calls.
 *
 * @returns The loaded configuration, or defaults if loading fails.
 */
export async function loadEnumVisualsConfig(): Promise<EnumVisualsConfig> {
  if (configCache) return configCache;

  try {
    const response = await fetch('/enumVisuals.json');
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    configCache = await response.json() as EnumVisualsConfig;
  } catch (error) {
    console.warn('Failed to load enumVisuals.json, using defaults:', error);
    configCache = defaultConfig;
  }

  return configCache;
}

/**
 * Gets the cached configuration synchronously.
 * Returns defaults if config hasn't been loaded yet.
 *
 * Note: Call loadEnumVisualsConfig() during app initialization to ensure
 * the config is loaded before this function is called.
 */
export function getEnumVisualsConfig(): EnumVisualsConfig {
  return configCache ?? defaultConfig;
}

/**
 * Gets visual properties for an entity by type and ID.
 * Merges entity-specific visuals with defaults (entity visuals take precedence).
 *
 * @param entityType - The type of entity (e.g., 'visitEventType')
 * @param id - The entity's identifier (id or value, depending on entity type)
 * @returns Merged visual properties for the entity
 *
 * @example
 * ```typescript
 * const visuals = getEntityVisuals('visitEventType', 1);
 * // Returns: { iconName: 'fa-bed', color: '#1976d2' }
 *
 * const visuals = getEntityVisuals('visitEventType', 999);
 * // Returns defaults: { iconName: 'fa-circle', color: '#9e9e9e' }
 * ```
 */
export function getEntityVisuals(entityType: EntityType, id: string | number): EntityVisuals {
  const config = configCache ?? defaultConfig;
  const key = String(id);

  const collections: Record<EntityType, Record<string, EntityVisuals>> = {
    visitEventType: config.visitEventTypes,
    hospitalizationStatus: config.hospitalizationStatuses,
    providerType: config.providerTypes,
    providerAssignmentBatchStatus: config.providerAssignmentBatchStatuses,
  };

  const entityVisuals = collections[entityType][key] ?? {};
  const defaultVisuals = config.defaults[entityType] ?? {};

  // Merge defaults with entity-specific visuals (entity visuals take precedence)
  return { ...defaultVisuals, ...entityVisuals };
}
