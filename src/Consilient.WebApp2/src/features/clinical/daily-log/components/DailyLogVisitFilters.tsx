import React from "react";
import { cn } from "@/shared/utils/utils";
import {
  DailyLogVisitFiltersDateProvider,
  type ProviderOption,
} from "./DailyLogVisitFiltersDateProvider";
import type { DailyLogVisit } from "@/features/clinical/daily-log/dailylog.types";
import { HospitalizationStatusPill } from "./HospitalizationStatusPill";
import { DynamicIcon } from "@/shared/components/DynamicIcon";

interface DailyLogVisitFiltersProps {
  date: string;
  providerId?: number | null;
  visitId?: number | null;
  onProviderChange?: (providerId: number | null) => void;
  onVisitIdChange?: (visitId: number | null) => void;
  visits: DailyLogVisit[];
}

export function DailyLogVisitFilters(props: DailyLogVisitFiltersProps) {
  // Ensure visits is always an array
  const safeVisits = React.useMemo(
    () => (Array.isArray(props.visits) ? props.visits : []),
    [props.visits]
  );

  // Group providers by unique id from visits
  const providers = React.useMemo(() => {
    const seen = new Map<number, ProviderOption>();
    safeVisits.forEach((visit) => {
      if (
        typeof visit.providerId === "number" &&
        visit.providerFirstName &&
        visit.providerLastName &&
        !seen.has(visit.providerId)
      ) {
        seen.set(visit.providerId, {
          id: visit.providerId,
          firstName: visit.providerFirstName,
          lastName: visit.providerLastName,
        });
      }
    });
    return Array.from(seen.values());
  }, [safeVisits]);

  // Allow selectedProviderId to be null
  const initialProviderId =
    props.providerId !== undefined
      ? props.providerId
      : (safeVisits.length > 0 && typeof safeVisits[0]?.providerId === "number"
          ? safeVisits[0].providerId
          : null);

  const [selectedProviderId, setSelectedProvider] =
    React.useState<number | null>(initialProviderId);

  // Get visits for selected provider (if null, show no visits)
  const filteredVisits: DailyLogVisit[] = React.useMemo(() => {
    if (selectedProviderId === null) return [];
    return safeVisits.filter((visit) => visit.providerId === selectedProviderId);
  }, [safeVisits, selectedProviderId]);

  // Keyboard navigation for visit list
  React.useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.altKey && (e.key === "ArrowUp" || e.key === "ArrowDown")) {
        if (!filteredVisits.length) return;
        e.preventDefault();
        if (!props.visitId) return;
        const idx = filteredVisits.findIndex((p) => p.id === props.visitId);
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

  const handleProviderChange = (newProviderId: number | null) => {
    setSelectedProvider(newProviderId);
    props.onProviderChange?.(newProviderId);
  };

  // Auto-select first visit when provider changes or on initial load
  React.useEffect(() => {
    if (filteredVisits.length > 0 && !props.visitId && filteredVisits[0]) {
      props.onVisitIdChange?.(filteredVisits[0].id);
    }
  }, [filteredVisits, props]);

  // Sync selectedProviderId with providers when visits change
  React.useEffect(() => {
    if (
      providers.length > 0 &&
      (selectedProviderId === null ||
        !providers.some((p) => p.id === selectedProviderId))
    ) {
      setSelectedProvider(providers[0]?.id ?? null);
      props.onProviderChange?.(providers[0]?.id ?? null);
    }
  }, [providers, props, selectedProviderId]);

  // Calculate min and max date for input
  const today = new Date();
  const maxDate: string = today.toISOString().split("T")[0] ?? "";
  const minDateObj = new Date(today);
  minDateObj.setDate(today.getDate() - 7);
  const minDate: string = minDateObj.toISOString().split("T")[0] ?? "";

  return (
    <div className="w-80 border-r border-gray-200 bg-white flex flex-col h-full relative">
      <DailyLogVisitFiltersDateProvider
        date={props.date}
        minDate={minDate}
        maxDate={maxDate}
        providerId={selectedProviderId}
        selectedProvider={selectedProviderId}
        onProviderChange={handleProviderChange}
        providers={providers}
      />
      {/* Patient List */}
      <div className="flex-1 overflow-y-auto">
        {!selectedProviderId ? (
          <div className="p-8 text-center text-gray-500">
            <svg
              className="mx-auto h-12 w-12 text-gray-400 mb-2"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
              />
            </svg>
            <p className="text-sm">Please select a provider</p>
          </div>
        ) : filteredVisits.length === 0 ? (
          <div className="p-8 text-center text-gray-500">
            <svg
              className="mx-auto h-12 w-12 text-gray-400 mb-2"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M12 12h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <p className="text-sm">No visits found</p>
          </div>
        ) : (
          filteredVisits.map((visit) => (
            <button
              key={visit.id}
              onClick={() => props.onVisitIdChange?.(visit.id)}
              className={cn(
                "w-full text-left px-4 py-3 hover:bg-gray-50 transition-colors border-l-4 border-b border-gray-100",
                props.visitId === visit.id
                  ? "bg-blue-50 border-l-blue-600"
                  : "border-l-transparent"
              )}
            >
              <div className="flex items-center justify-between">
                <div className="flex-1 min-w-0 flex items-center">
                  <p
                    className={cn(
                      "text-sm font-medium truncate",
                      props.visitId === visit.id
                        ? "text-blue-900"
                        : "text-gray-900"
                    )}
                  >
                    {visit.patientLastName}, {visit.patientFirstName}
                  </p>
                </div>
                {/* Icons float right, to the left of the status */}
                <div className="flex items-center">
                  <div className="flex items-center mr-2">
                    {visit.markers.map((marker) => (
                      <span
                        key={marker.iconName}
                        className="inline-flex items-center justify-center rounded-full mx-1"
                        style={{
                          width: 21,
                          height: 21,
                          padding: 4,
                          margin: 0,
                          backgroundColor: marker.hasData ? marker.color : "#bdbdbd"
                        }}
                      >
                        <DynamicIcon
                          iconName={marker.iconName}
                          style={{ color: "white", fontSize: 12 }}
                        />
                      </span>
                    ))}
                  </div>
                  {visit.hospitalizationStatusId && (
                    <HospitalizationStatusPill
                      statusId={visit.hospitalizationStatusId}
                    />
                  )}
                </div>
              </div>
            </button>
          ))
        )}
      </div>
      {/* Sticky footer for shortcut help */}
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
    </div>
  );
}
