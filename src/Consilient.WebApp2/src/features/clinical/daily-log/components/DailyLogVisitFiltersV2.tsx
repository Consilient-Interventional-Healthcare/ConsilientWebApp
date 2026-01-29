import React from "react";
import { cn } from "@/shared/utils/utils";
import { DateNavigator } from "@/shared/components/ui/date-navigator";
import { NativeSelect } from "@/shared/components/ui/native-select";
import type { GraphQL, Facilities } from "@/types/api.generated";
import {
  filterVisitsByProviderV2,
  getPatientDisplayNameV2,
  getProviderDisplayName,
} from "@/features/clinical/daily-log/dailylog.adapters";
import { HospitalizationStatusPill } from "./HospitalizationStatusPill";

interface DailyLogVisitFiltersV2Props {
  date: string;
  providerId?: number | null;
  visitId?: number | null;
  onProviderChange?: (providerId: number | null) => void;
  onVisitIdChange?: (visitId: number | null) => void;
  visits: GraphQL.DailyLogVisit[];
  providers: GraphQL.DailyLogProvider[];
  onDateChange?: (date: string) => void;
  facilities: Facilities.FacilityDto[];
  selectedFacilityId: number | null;
  onFacilityChange?: (facilityId: number | null) => void;
}

export function DailyLogVisitFiltersV2(props: DailyLogVisitFiltersV2Props) {
  // Refs for scroll-into-view behavior
  const scrollContainerRef = React.useRef<HTMLDivElement>(null);
  const visitItemRefs = React.useRef<Map<number, HTMLButtonElement>>(new Map());

  // Ensure visits and providers are always arrays
  const safeVisits = React.useMemo(
    () => (Array.isArray(props.visits) ? props.visits : []),
    [props.visits]
  );

  const safeProviders = React.useMemo(
    () => (Array.isArray(props.providers) ? props.providers : []),
    [props.providers]
  );

  const safeFacilities = React.useMemo(
    () => (Array.isArray(props.facilities) ? props.facilities : []),
    [props.facilities]
  );

  // Allow selectedProviderId to be null
  const initialProviderId = React.useMemo(() => {
    if (props.providerId !== undefined) return props.providerId;
    if (safeProviders.length > 0 && safeProviders[0]) {
      return safeProviders[0].id;
    }
    return null;
  }, [props.providerId, safeProviders]);

  const [selectedProviderId, setSelectedProvider] =
    React.useState<number | null>(initialProviderId);

  // Get visits for selected provider
  const filteredVisits = React.useMemo(() => {
    const filtered = filterVisitsByProviderV2(safeVisits, selectedProviderId);
    return filtered.sort((a, b) => {
      const lastNameA = (a.patient?.lastName ?? "").toLowerCase();
      const lastNameB = (b.patient?.lastName ?? "").toLowerCase();
      if (lastNameA !== lastNameB) return lastNameA.localeCompare(lastNameB);
      const firstNameA = (a.patient?.firstName ?? "").toLowerCase();
      const firstNameB = (b.patient?.firstName ?? "").toLowerCase();
      return firstNameA.localeCompare(firstNameB);
    });
  }, [safeVisits, selectedProviderId]);

  // Keyboard navigation for visit list
  React.useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.altKey && (e.key === "ArrowUp" || e.key === "ArrowDown")) {
        if (!filteredVisits.length) return;
        if (!props.visitId) return;
        e.preventDefault();
        const idx = filteredVisits.findIndex((v) => v.id === props.visitId);
        if (idx === -1) return;
        let nextIdx = idx;
        if (e.key === "ArrowUp") {
          nextIdx = idx === 0 ? filteredVisits.length - 1 : idx - 1;
        } else if (e.key === "ArrowDown") {
          nextIdx = idx === filteredVisits.length - 1 ? 0 : idx + 1;
        }
        const nextVisit = filteredVisits[nextIdx];
        if (nextVisit) {
          props.onVisitIdChange?.(nextVisit.id);
        }
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [filteredVisits, props]);

  // Scroll selected visit into view if outside visible area
  React.useEffect(() => {
    if (!props.visitId) return;

    // Use requestAnimationFrame to ensure refs are populated after render
    const frameId = requestAnimationFrame(() => {
      const visitElement = visitItemRefs.current.get(props.visitId!);
      const container = scrollContainerRef.current;

      if (visitElement && container) {
        const containerRect = container.getBoundingClientRect();
        const itemRect = visitElement.getBoundingClientRect();

        // Only scroll if item is outside visible area
        if (itemRect.top < containerRect.top || itemRect.bottom > containerRect.bottom) {
          visitElement.scrollIntoView({ block: 'start', behavior: 'smooth' });
        }
      }
    });

    return () => cancelAnimationFrame(frameId);
  }, [props.visitId, filteredVisits]);

  const handleProviderChange = (newProviderId: number | null) => {
    setSelectedProvider(newProviderId);
    props.onProviderChange?.(newProviderId);
  };

  // Auto-select first visit when provider changes or on initial load
  // Only auto-select if we have visits AND a valid provider selected
  // This prevents auto-selection during facility/date changes when data is being cleared
  React.useEffect(() => {
    if (
      filteredVisits.length > 0 &&
      !props.visitId &&
      filteredVisits[0] &&
      selectedProviderId !== null
    ) {
      props.onVisitIdChange?.(filteredVisits[0].id);
    }
  }, [filteredVisits, props, selectedProviderId]);

  // Sync selectedProviderId with providers when visits change
  // Only update local state, don't call parent callback to avoid navigation loops
  React.useEffect(() => {
    if (
      safeProviders.length > 0 &&
      (selectedProviderId === null ||
        !safeProviders.some((p) => p.id === selectedProviderId))
    ) {
      setSelectedProvider(safeProviders[0]?.id ?? null);
      // Don't call props.onProviderChange here - it causes navigation loops
    }
  }, [safeProviders, selectedProviderId]);

  // Calculate min and max date for input
  const today = new Date();
  const maxDate: string = today.toISOString().split("T")[0] ?? "";
  const minDateObj = new Date(today);
  minDateObj.setDate(today.getDate() - 7);
  const minDate: string = minDateObj.toISOString().split("T")[0] ?? "";

  return (
    <div className="w-80 border-r border-gray-200 bg-white flex flex-col h-full relative">
      {/* Facility, Date, and Provider Selection */}
      <div className="px-4 py-4 border-b border-gray-200 space-y-3">
        {/* Facility Dropdown */}
        <FacilityDropdown
          facilities={safeFacilities}
          selectedFacilityId={props.selectedFacilityId}
          onFacilityChange={props.onFacilityChange}
        />
        <div className="space-y-2">
          <label
            htmlFor="date-input"
            className="text-xs font-medium text-gray-700 uppercase tracking-wide"
          >
            Date
          </label>
          <DateNavigator
            id="date-input"
            value={props.date}
            min={minDate}
            max={maxDate}
            onChange={(date) => props.onDateChange?.(date)}
            className="w-full"
          />
        </div>
        {/* Provider Dropdown */}
        <ProviderDropdown
          date={props.date}
          selectedProviderId={selectedProviderId}
          providers={safeProviders}
          onProviderChange={handleProviderChange}
        />
      </div>

      {/* Visit List */}
      <div ref={scrollContainerRef} className="flex-1 overflow-y-auto">
        {!selectedProviderId ? (
          <EmptyState type="no-provider" />
        ) : filteredVisits.length === 0 ? (
          <EmptyState type="no-visits" />
        ) : (
          filteredVisits.map((visit) => (
            <VisitListItem
              key={visit.id}
              ref={(el) => {
                if (el) visitItemRefs.current.set(visit.id, el);
                else visitItemRefs.current.delete(visit.id);
              }}
              visit={visit}
              isSelected={props.visitId === visit.id}
              onSelect={() => props.onVisitIdChange?.(visit.id)}
              selectedProviderId={selectedProviderId}
              providers={safeProviders}
            />
          ))
        )}
      </div>

      {/* Keyboard shortcuts footer */}
      <KeyboardShortcutsFooter />
    </div>
  );
}

// Sub-components for better organization

interface ProviderDropdownProps {
  date: string;
  selectedProviderId: number | null;
  providers: GraphQL.DailyLogProvider[];
  onProviderChange: (providerId: number | null) => void;
}

function ProviderDropdown({
  date,
  selectedProviderId,
  providers,
  onProviderChange,
}: ProviderDropdownProps) {
  const isDisabled = !date || providers.length === 0;

  return (
    <div className="space-y-2">
      <label
        htmlFor="provider-select"
        className="text-xs font-medium text-gray-700 uppercase tracking-wide"
      >
        Provider
      </label>
      <NativeSelect
        id="provider-select"
        value={selectedProviderId ?? ""}
        onChange={(e) =>
          onProviderChange(e.target.value === "" ? null : Number(e.target.value))
        }
        disabled={isDisabled}
      >
        {providers.length === 0 ? (
          <option value="">No providers</option>
        ) : (
          providers.map((provider) => (
            <option key={provider.id} value={provider.id}>
              {getProviderDisplayName(provider)}
            </option>
          ))
        )}
      </NativeSelect>
    </div>
  );
}

interface FacilityDropdownProps {
  facilities: Facilities.FacilityDto[];
  selectedFacilityId: number | null;
  onFacilityChange: ((facilityId: number | null) => void) | undefined;
}

function FacilityDropdown({
  facilities,
  selectedFacilityId,
  onFacilityChange,
}: FacilityDropdownProps) {
  const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newValue = e.target.value === "" ? null : Number(e.target.value);
    onFacilityChange?.(newValue);
  };

  return (
    <div className="space-y-2">
      <label
        htmlFor="facility-select"
        className="text-xs font-medium text-gray-700 uppercase tracking-wide"
      >
        Facility
      </label>
      <NativeSelect
        id="facility-select"
        value={selectedFacilityId ?? ""}
        onChange={handleChange}
      >
        {facilities.map((facility) => (
          <option key={facility.id} value={facility.id}>
            {facility.name}
          </option>
        ))}
      </NativeSelect>
    </div>
  );
}

interface VisitListItemProps {
  visit: GraphQL.DailyLogVisit;
  isSelected: boolean;
  onSelect: () => void;
  selectedProviderId: number | null;
  providers: GraphQL.DailyLogProvider[];
}

const VisitListItem = React.forwardRef<HTMLButtonElement, VisitListItemProps>(
  function VisitListItem({ visit, isSelected, onSelect, selectedProviderId, providers }, ref) {
    const roomBed = [visit.room, visit.bed].filter(Boolean).join(" / ");

    const otherProviders = React.useMemo(() => {
      return (visit.providerIds ?? [])
        .filter((id) => id !== selectedProviderId)
        .map((id) => providers.find((p) => p.id === id))
        .filter((p): p is GraphQL.DailyLogProvider => p != null)
        .map((p) => {
          const abbrev = (p as { providerType?: { code?: string } }).providerType?.code ?? "";
          return abbrev ? `${p.lastName} (${abbrev})` : p.lastName ?? "";
        })
        .join(", ");
    }, [visit.providerIds, selectedProviderId, providers]);

    return (
      <button
        ref={ref}
        onClick={onSelect}
        className={cn(
          "w-full text-left px-4 py-3 hover:bg-gray-50 transition-colors border-l-4 border-b border-gray-100",
          isSelected ? "bg-blue-50 border-l-blue-600" : "border-l-transparent"
        )}
      >
        <div className="flex items-center justify-between">
          <div className="flex-1 min-w-0">
            <p
              className={cn(
                "text-sm font-medium truncate",
                isSelected ? "text-blue-900" : "text-gray-900"
              )}
            >
              {getPatientDisplayNameV2(visit)}
            </p>
            {roomBed && (
              <p className="text-xs text-gray-500 truncate">
                {roomBed}
              </p>
            )}
            {otherProviders && (
              <p className="text-xs text-gray-500 truncate">{otherProviders}</p>
            )}
          </div>
          <div className="flex items-center ml-2">
            {visit.hospitalization?.hospitalizationStatusId && (
              <HospitalizationStatusPill
                statusId={visit.hospitalization.hospitalizationStatusId}
              />
            )}
          </div>
        </div>
      </button>
    );
  }
);

interface EmptyStateProps {
  type: "no-provider" | "no-visits";
}

function EmptyState({ type }: EmptyStateProps) {
  const isNoProvider = type === "no-provider";

  return (
    <div className="p-8 text-center text-gray-500">
      <svg
        className="mx-auto h-12 w-12 text-gray-400 mb-2"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
      >
        {isNoProvider ? (
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
          />
        ) : (
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M12 12h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        )}
      </svg>
      <p className="text-sm">
        {isNoProvider ? "Please select a provider" : "No visits found"}
      </p>
    </div>
  );
}

function KeyboardShortcutsFooter() {
  return (
    <div className="sticky bottom-0 w-full bg-white border-t border-gray-200 px-4 py-2 text-xs text-gray-500 flex items-center gap-4 z-10">
      <span>
        <kbd className="px-1 py-0.5 bg-gray-200 rounded border text-gray-700">
          Alt
        </kbd>{" "}
        +{" "}
        <kbd className="px-1 py-0.5 bg-gray-200 rounded border text-gray-700">
          ↑
        </kbd>{" "}
        Previous
      </span>
      <span>
        <kbd className="px-1 py-0.5 bg-gray-200 rounded border text-gray-700">
          Alt
        </kbd>{" "}
        +{" "}
        <kbd className="px-1 py-0.5 bg-gray-200 rounded border text-gray-700">
          ↓
        </kbd>{" "}
        Next
      </span>
    </div>
  );
}
