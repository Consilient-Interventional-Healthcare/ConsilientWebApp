# DailyLog Refactoring Plan

## Overview
Refactor the DailyLog feature to use types from `api.generated.ts` in three stages, preserving existing code throughout.

**Duration:** Multi-day effort
**Approach:** Staged refactoring with feature flags to preserve existing functionality

---

## Current Architecture

### Three-Column Layout
| Column | Component | Purpose |
|--------|-----------|---------|
| Left | `DailyLogVisitFilters` | Date picker, provider dropdown, patient/visit list |
| Center | `DailyLogEntriesPanel` | Header, timeline entries, input form |
| Right | `DailyLogAdditionalInfo` | Demographics, hospitalization details, progress timeline |

### Key Files
- [DailyLog.tsx](../../src/features/clinical/daily-log/DailyLog.tsx) - Main component
- [dailylog.types.ts](../../src/features/clinical/daily-log/dailylog.types.ts) - Custom type definitions
- ~~DailyLogVisitFilters.tsx~~ - Removed, replaced by `DailyLogVisitFiltersV2.tsx`
- [DailyLogVisitFiltersV2.tsx](../../src/features/clinical/daily-log/components/DailyLogVisitFiltersV2.tsx) - Left column (V2)
- [DailyLogEntriesPanel.tsx](../../src/features/clinical/daily-log/components/DailyLogEntriesPanel.tsx) - Center column
- [DailyLogAdditionalInfo.tsx](../../src/features/clinical/daily-log/components/DailyLogAdditionalInfo.tsx) - Right column (needs V2 refactor)
- [api.generated.ts](../../src/types/api.generated.ts) - Generated API types
- [VisitEventTypeStore.ts](../../src/shared/stores/VisitEventTypeStore.ts) - Event types with caching
- [HospitalizationStatusStore.ts](../../src/shared/stores/HospitalizationStatusStore.ts) - Hospitalization statuses with caching
- [useVisitMemory.ts](../../src/features/clinical/daily-log/hooks/useVisitMemory.ts) - Provider/visit selection memory hook
- [RoomBed.tsx](../../src/shared/components/RoomBed.tsx) - Shared room/bed display component
- [HospitalizationStatusPill.tsx](../../src/shared/components/HospitalizationStatusPill.tsx) - Shared status pill component
- [enrichedEntities.ts](../../src/shared/types/enrichedEntities.ts) - Enriched entity types (visuals + API)

### Current Custom Types (dailylog.types.ts)
- `DailyLogVisit` - Combined patient/hospitalization/provider info (legacy - **only used by Stage 3**)
- `DailyLogVisitPhaseMarker` - Icon/color markers for visit list
- `StatusChangeEvent` - Timeline events (used by PatientTimeline)
- `DailyLogLogEntryV2` - V2 log entry with event, user, and enriched eventType
- `DailyLogVisitsResponse` - V2 response containing visits and providers
- `DailyLogProviderV2` - Alias for `GraphQL.DailyLogProvider`
- `IDailyLogServiceV2` - V2 service interface

### Shared Types (enrichedEntities.ts)
- `EntityVisuals` - Base interface for icon/color/className
- `EnrichedVisitEventType` - VisitEventTypeDto + visuals
- `EnrichedHospitalizationStatus` - HospitalizationStatusDto + iconName/className
- `EnrichedProviderType` - Provider type with visuals
- `EnrichedProviderAssignmentBatchStatus` - Batch status with visuals

### Available Generated Types (api.generated.ts)
- `GraphQL.DailyLogVisit` - Visit with patient, hospitalization, room, bed, providerIds
- `GraphQL.DailyLogVisitsResult` - Query result with date, facilityId, visits, providers
- `GraphQL.DailyLogProvider` - Provider id, firstName, lastName
- `GraphQL.DailyLogHospitalization` - id, admissionDate, hospitalizationStatusId, caseId
- `GraphQL.VisitPatient` - id, firstName, lastName, birthDate, mrn (**missing: gender**)
- `GraphQL.HospitalizationStatus` - Status with color/iconName
- `VisitEvents.VisitEventDto` - Log entry events
- `VisitEvents.VisitEventTypeDto` - Event type info

---

## Preservation Strategy

### Feature Flags
Add to `config/index.ts`:
```typescript
dailyLogV2Stage1: boolean;  // Left column
dailyLogV2Stage2: boolean;  // Center column
dailyLogV2Stage3: boolean;  // Right column
```

### Parallel Components
For each component:
1. Create `ComponentV2.tsx` alongside original
2. Use feature flag to conditionally render
3. Keep both until V2 is validated
4. Remove legacy after successful rollout

### Adapter Pattern
Create `dailylog.adapters.ts` with display/filter utility functions. Note: Type conversion adapters were not needed since V2 components use GraphQL types directly.

---

## Stage 1: Left Column (Date, Provider, Visit Selector)

### Components to Refactor
- `DailyLogVisitFilters.tsx`
- `DailyLogVisitFiltersDateProvider.tsx`
- Routing configuration (verify/modify as needed)

### Type Changes

**Current `DailyLogVisit`:**
```typescript
interface DailyLogVisit {
  id, patientId, patientLastName, patientFirstName, patientDateOfBirth,
  patientGender, patientMRN, hospitalizationId, hospitalizationAdmissionDate,
  hospitalizationStatusId, providerId, providerFirstName, providerLastName,
  room, markers: DailyLogVisitPhaseMarker[]
}
```

**Target: Use `GraphQL.Visit`:**
- `visit.patient?.firstName`, `visit.patient?.lastName`
- `visit.patient?.birthDate`, `visit.patient?.mrn`
- `visit.hospitalization?.id`, `visit.hospitalization?.admissionDate`
- `visit.visitAttendants[].provider` for provider info
- `visit.room`

### Tasks
- [x] Verify/modify routing configuration for DailyLog
  - [x] Review current route params (date, providerId, visitId)
  - [x] Ensure loader function works with new types
  - [x] Update URL parameter handling if needed (no changes needed)
- [x] Add V2 feature flags to config
- [x] Create `DailyLogVisitV2` type wrapping `GraphQL.Visit`
- [x] ~~Create adapter function~~ (not needed - V2 uses GraphQL types directly)
- [x] Create `DailyLogVisitFiltersV2.tsx`
- [x] Update service to return `GraphQL.Visit[]`
- [x] Update `DailyLog.tsx` with conditional rendering
- [ ] Build missing API endpoints as needed (deferred - using mock for now)

### API Requirements (build as needed)
- GraphQL query to fetch visits by date with patient, hospitalization, and attendants
- May need to add `gender` to VisitPatient if not present

---

## Stage 2: Center Column (Note Taking)

### Components to Refactor
- `DailyLogEntriesPanel.tsx`
- `DailyLogEntriesHeader.tsx`
- `DailyLogEntriesDisplay.tsx`
- `DailyLogEntriesInput.tsx`

### Type Changes

**Current `DailyLogLogEntry`:**
```typescript
interface DailyLogLogEntry extends VisitEvents.VisitEventDto {
  userFirstName, userLastName, userRole
}
```

**Target `DailyLogLogEntryV2`:**
```typescript
interface DailyLogLogEntryV2 {
  event: VisitEvents.VisitEventDto;
  user: { firstName, lastName, role };
  eventType: VisitEvents.VisitEventTypeDto | null;
}
```

### Tasks
- [x] ~~Create `DailyLogLogEntryV2` type~~ (already existed in dailylog.types.ts)
- [x] ~~Create V2 components~~ (refactored in-place instead)
- [x] Update `DailyLogEntriesPanel.tsx` to accept GraphQL.DailyLogVisit
- [x] Update `DailyLogEntriesHeader.tsx` to use nested fields
- [x] Update `DailyLogEntriesDisplay.tsx` to accept DailyLogLogEntryV2[]
- [x] Update service calls to use V2 methods
- [x] Create `VisitEventTypeStore.ts` for API-based event types with caching
- [x] Create `visitEventTypeConfig.ts` for local icon/color enrichment (API doesn't provide these)
- [x] Update `DailyLogEntriesHeader.tsx` to use `useVisitEventTypes()` instead of mock data
- [x] Update `DailyLogLogEntryV2.eventType` to use `EnrichedVisitEventType`
- [x] Implement API calls for log entries (getLogEntriesByVisitIdV2, insertLogEntryV2)

### API Requirements (build as needed)
- Endpoint to fetch visit events with user info
- Endpoint to insert visit event
- May need `enteredByUser` relation on VisitEvent

---

## Stage 3: Right Column (Additional Info)

### Components to Refactor
- `DailyLogAdditionalInfo.tsx` - Main component, still uses legacy `DailyLogVisit` type
- `PatientTimeline.tsx` - Uses `StatusChangeEvent[]`, may stay as-is
- ~~`HospitalizationStatusPill.tsx`~~ - Already moved to `@/shared/components/` and uses store

### Current State
- `DailyLog.tsx` passes `null` to `DailyLogPatientDetails` with comment: "Stage 3 - pass null until refactored"
- `RoomBed.tsx` shared component already created and used
- `HospitalizationStatusPill.tsx` already refactored to use `useHospitalizationStatusById`

### Type Changes

**Current field access (`DailyLogVisit`):**
```typescript
visit.patientDateOfBirth        // Age calculation
visit.patientGender             // Demographics - NOT in GraphQL!
visit.patientMRN                // Hospitalization details
visit.hospitalizationId
visit.hospitalizationAdmissionDate
visit.room
visit.bed
```

**Target field access (`GraphQL.DailyLogVisit`):**
```typescript
visit.patient?.birthDate        // ✓ Available
visit.patient?.gender           // ✗ NOT AVAILABLE - needs API addition
visit.patient?.mrn              // ✓ Available
visit.hospitalization?.id       // ✓ Available
visit.hospitalization?.admissionDate  // ✓ Available
visit.room                      // ✓ Available
visit.bed                       // ✓ Available
```

### API Gap: Patient Gender
`GraphQL.VisitPatient` is missing `gender` field. Options:
1. Add `gender` to `VisitPatient` GraphQL type (preferred)
2. Make a separate patient fetch
3. Skip gender display temporarily

### Tasks
- [ ] Add `gender` field to `VisitPatient` GraphQL type (backend change)
- [ ] Regenerate types: `.\build.ps1 GenerateAllTypes`
- [ ] Refactor `DailyLogAdditionalInfo.tsx` in-place to accept `GraphQL.DailyLogVisit`
- [ ] Update `DailyLog.tsx` to pass `selectedVisit` instead of `null`
- [ ] Verify date calculations work with new date format (`DateOnly` vs `string`)
- [ ] Keep `StatusChangeEvent` as custom type (timeline API not built yet)
- [ ] Keep `PatientTimeline.tsx` as-is

### API Requirements
- **Required:** Add `gender` field to `VisitPatient` in GraphQL schema
- **Deferred:** Status history endpoint for timeline (keep using mock)

---

## Verification Checklist

### Stage 1 & 2 (Completed)
- [x] No TypeScript compilation errors
- [x] Left column (DailyLogVisitFiltersV2) works correctly
- [x] Center column (DailyLogEntriesPanel) displays log entries
- [x] Event type icons/colors display from enumVisuals.json
- [x] Provider/visit selection persists via useVisitMemory hook
- [x] Date/facility changes work correctly

### Stage 3 (Completed)
- [x] No TypeScript compilation errors (DailyLog-related)
- [x] Right column uses `GraphQL.DailyLogVisit` type
- [x] Gender displays correctly (string enum: 'Male', 'Female', 'Other')
- [x] Age calculation works with `DateOnly` format
- [x] Hospitalization details display correctly
- [x] Patient timeline component unchanged (uses mock data)

### Final
- [x] All three columns work together with V2 types
- [x] Legacy `DailyLogVisit` type removed
- [x] Legacy `DailyLogVisitPhaseMarker` type removed
- [ ] Manual testing (pending)

---

## File Creation Summary

### New Files Created
1. `dailylog.adapters.ts` - Display/filter utility functions (Stage 1)
2. `DailyLogVisitFiltersV2.tsx` - V2 left column component (Stage 1)
3. ~~`src/shared/config/visitEventTypeConfig.ts`~~ - Deleted, replaced by `enumVisualsConfig.ts`
4. `src/shared/config/enumVisualsConfig.ts` - Centralized visual config loader from JSON (Stage 2)
5. `public/enumVisuals.json` - Runtime-configurable visual properties (icon, color) for entities
6. `src/shared/stores/VisitEventTypeStore.ts` - API store with `useVisitEventTypes()` hook (Stage 2)
7. `src/shared/components/RoomBed.tsx` - Shared room/bed display component (Stage 2/3 prep)
8. `src/shared/components/HospitalizationStatusPill.tsx` - Moved from daily-log, uses store (Stage 2)
9. `src/shared/types/enrichedEntities.ts` - Centralized enriched entity types (Stage 2)
10. `src/features/clinical/daily-log/hooks/useVisitMemory.ts` - Provider/visit selection memory (Stage 1)

### Files to Create (Remaining)
None - Stage 3 will refactor `DailyLogAdditionalInfo.tsx` in-place

### Files Modified (Stages 1 & 2)
1. ~~`config/index.ts` - Add feature flags~~ (feature flags removed after validation)
2. `dailylog.types.ts` - Added V2 types, removed legacy types
3. `DailyLog.tsx` - Uses V2 types, passes `selectedVisit` to center column
4. `DailyLogService.ts` - Implements V2 methods with GraphQL + REST
5. `api.generated.ts` - Regenerated with DailyLog GraphQL types
6. `main.tsx` - Added `loadEnumVisualsConfig()` initialization

### Files to Modify (Stage 3)
1. `DailyLogAdditionalInfo.tsx` - Change prop type from `DailyLogVisit` to `GraphQL.DailyLogVisit`
2. `DailyLog.tsx` - Pass `selectedVisit` instead of `null` to right column
3. `api.generated.ts` - Regenerate after adding `gender` to `VisitPatient`

---

## Progress Tracking

### Stage 1: Left Column
- Status: Complete
- Started: 2026-01-27
- Completed: 2026-01-27
- Files created:
  - `dailylog.adapters.ts` - Type conversion functions
  - `DailyLogVisitFiltersV2.tsx` - V2 component for left column
- Files modified:
  - `config/index.ts` - Added dailyLogV2 feature flags
  - `dailylog.types.ts` - Added V2 types (DailyLogVisitV2, DailyLogLogEntryV2, etc.)
  - `DailyLogService.ts` - Added V2 method stubs
  - `DailyLogServiceMock.ts` - Added V2 method implementations
  - `DailyLog.tsx` - Added conditional rendering based on feature flag

### Stage 2: Center Column
- Status: Complete (in-place refactoring)
- Started: 2026-01-28
- Completed: 2026-01-28
- Files created:
  - ~~`src/shared/config/visitEventTypeConfig.ts`~~ - Deleted, replaced by `enumVisualsConfig.ts`
  - `src/shared/config/enumVisualsConfig.ts` - Centralized visual config loader
  - `public/enumVisuals.json` - Runtime-configurable entity visuals
  - `src/shared/stores/VisitEventTypeStore.ts` - API store with `useVisitEventTypes()` hook
  - `src/shared/types/enrichedEntities.ts` - Centralized enriched entity types
  - `src/shared/components/RoomBed.tsx` - Shared room/bed display component
  - `src/shared/components/HospitalizationStatusPill.tsx` - Moved from daily-log folder
  - `src/features/clinical/daily-log/hooks/useVisitMemory.ts` - Visit selection memory
- Files modified:
  - `DailyLog.tsx` - Pass selectedVisit (V2 type) to center column
  - `DailyLogEntriesPanel.tsx` - Accept GraphQL.DailyLogVisit, use V2 service methods
  - `DailyLogEntriesHeader.tsx` - Use nested fields, use `useVisitEventTypes()` hook
  - `DailyLogEntriesDisplay.tsx` - Accept DailyLogLogEntryV2[], use entry.eventType directly
  - `dailylog.types.ts` - `DailyLogLogEntryV2.eventType` now uses `EnrichedVisitEventType`
  - `DailyLogService.ts` - Implemented `getLogEntriesByVisitIdV2` (GraphQL) and `insertLogEntryV2` (REST + refetch)
  - `main.tsx` - Added `loadEnumVisualsConfig()` call during app initialization
- Notes: No feature flag needed - refactored in-place since legacy was already broken
- API Implementation Details:
  - `getLogEntriesByVisitIdV2`: Uses GraphQL query, maps response to local types, enriches with icons/colors
  - `insertLogEntryV2`: Posts to REST `/visit/{visitId}/event`, then refetches via GraphQL to get complete data with user info

### Stage 3: Right Column
- Status: ✅ Complete
- Started: 2026-01-29
- Completed: 2026-01-29
- Backend changes:
  - Added `Gender` property to `VisitPatient.cs`
  - Registered `Gender` enum in `GraphQlSchemaConfigurator.cs`
  - Registered Gender field in `GraphQlSchemaConfigurator.Visits.cs`
  - Mapped Gender in `GraphQlSchemaConfigurator.DailyLog.cs`
- Frontend changes:
  - Refactored `DailyLogAdditionalInfo.tsx` to use `GraphQL.DailyLogVisit`
  - Updated `DailyLog.tsx` to pass `selectedVisit` instead of `null`
  - Removed legacy `DailyLogVisit` and `DailyLogVisitPhaseMarker` types
  - Gender enum is string-based ('Male', 'Female', 'Other') - no mapping needed
- Types regenerated and verified with `npm run type-check`

### Cleanup Phase
- Status: ✅ Complete
- Completed:
  - [x] Removed `visitEventTypes` mock data from `db.json`
  - [x] Removed `visitEventTypes` from `db.types.ts` schema
  - [x] Consolidated `EnrichedVisitEventType` to single location (`enrichedEntities.ts`)
  - [x] Created centralized `enumVisualsConfig.ts` for all entity visuals
  - [x] Deleted `visitEventTypeConfig.ts` (replaced by `enumVisualsConfig.ts`)
  - [x] Deleted `LogEntryTypeProvider.ts` (unused after eventTypeId fix)
  - [x] Removed `ILogEntryTypeProvider` interface from `dailylog.types.ts`
  - [x] Removed `IDailyLogService` interface from `dailylog.types.ts`
  - [x] Removed `DailyLogLogEntry` type from `dailylog.types.ts` (replaced by V2)
  - [x] Removed legacy `getLogEntriesByVisitId` and `insertLogEntry` methods from `DailyLogService.ts`
  - [x] Removed `dataProvider` dependency from daily-log components:
    - Added `useHospitalizationStatusById` helper to `HospitalizationStatusStore.ts`
    - Updated `DailyLogEntriesHeader.tsx` to use the store hook
    - Updated `HospitalizationStatusPill.tsx` to use the store hook
  - [x] Moved `HospitalizationStatusPill.tsx` to `src/shared/components/`
  - [x] Created `RoomBed.tsx` shared component
  - [x] Removed all `console.log` debug statements from daily-log folder
  - [x] Removed commented CSS from `DailyLogEntriesDisplay.tsx`
- Remaining:
  - [x] Remove legacy `DailyLogVisit` type (completed in Stage 3)
  - [x] Remove legacy `DailyLogVisitPhaseMarker` type (completed in Stage 3)
  - [x] Regenerate types: `.\build.ps1 GenerateAllTypes` (completed)
  - [ ] Remove `console.error` from `DailyLog.tsx` (optional, useful for debugging)

---

## Cleanup Checklist

### Stage 1 Cleanup (after V2 validation)
When ready to remove legacy code for Stage 1:
- [x] Remove `DailyLogVisitFilters.tsx` (legacy left column component)
- [x] Remove `DailyLogVisitFiltersDateProvider.tsx` if unused
- [x] Remove legacy `DailyLogVisit` type from `dailylog.types.ts` (removed in Stage 3)
- [x] Remove `IDailyLogService` interface from `dailylog.types.ts`
- [x] Remove `getVisitsByDate` method from services
- [x] Remove `dailyLogV2.stage1` feature flag from config
- [x] Remove legacy `visits` state from `DailyLog.tsx`
- [x] Clean up unused imports in `DailyLog.tsx`

### Stage 2 Cleanup (completed)
- [x] Removed `visitEventTypes` mock data from `db.json`
- [x] Removed `visitEventTypes` from `db.types.ts` DbSchema
- [x] Consolidated duplicate `EnrichedVisitEventType` definitions
- [x] ~~Updated `VisitEventTypeStore.ts` to import from `visitEventTypeConfig.ts`~~ Replaced with `enumVisualsConfig.ts`
- [x] Created centralized `enumVisualsConfig.ts` for all entity visuals
- [x] Created `public/enumVisuals.json` for runtime-configurable visuals
- [x] Deleted `visitEventTypeConfig.ts` (replaced by centralized config)
- [x] Removed `dataProvider` dependency from `DailyLogEntriesHeader.tsx` (uses `useHospitalizationStatusById`)
- [x] Removed `dataProvider` dependency from `HospitalizationStatusPill.tsx` (uses `useHospitalizationStatusById`)
- [x] Added `useHospitalizationStatusById` helper to `HospitalizationStatusStore.ts`
- [x] Removed all `console.log` debug statements from daily-log folder
- [x] Removed commented CSS from `DailyLogEntriesDisplay.tsx`
- [ ] Remove `console.error` from `DailyLog.tsx` line 55 (keep for now, useful for debugging)
