import { useNavigate, useLoaderData } from "react-router-dom";
import { useState, useEffect, useContext, useMemo } from "react";
import { DailyLogVisitFilters } from "./components/DailyLogVisitFilters";
import { dailyLogService } from "./services/DailyLogService";
import LoadingBarContext from "@/shared/layouts/LoadingBarContext";
import DailyLogPatientDetails from "./components/DailyLogAdditionalInfo";
import { DailyLogEntriesPanel } from "./components/DailyLogEntriesPanel";
import { facilityService } from "@/features/clinical/visits/services/FacilityService";
import type { Facilities, GraphQL } from "@/types/api.generated";
import { useVisitMemory } from "./hooks/useVisitMemory";


export default function DailyLog() {
  const { date, facilityId, providerId, visitId } = useLoaderData<{
    date: string;
    facilityId: number | null;
    providerId?: number;
    visitId?: number;
  }>();
  const navigate = useNavigate();
  const {
    getVisitId: getMemorizedVisitId,
    setVisitId: setMemorizedVisitId,
    getProviderId: getMemorizedProviderId,
    setProviderId: setMemorizedProviderId
  } = useVisitMemory();

  // State for daily log data and facilities
  const [dailyLogData, setDailyLogData] = useState<GraphQL.DailyLogVisitsResult | null>(null);
  const [facilities, setFacilities] = useState<Facilities.FacilityDto[]>([]);

  const loadingBar = useContext(LoadingBarContext);

  // Helper to build URLs
  const buildUrl = (parts: (string | number | null | undefined)[]) => {
    const filtered = parts.filter(p => p != null);
    return `/clinical/daily-log/${filtered.join('/')}`;
  };

  // Fetch daily log data when date/facility changes
  useEffect(() => {
    if (!facilityId) return;

    loadingBar?.start();

    dailyLogService
      .getVisitsByDate(date, facilityId)
      .then((data) => {
        setDailyLogData(data);
      })
      .catch((err: unknown) => {
        console.error("Failed to fetch visits", err);
      })
      .finally(() => {
        loadingBar?.complete();
      });
  }, [date, facilityId, loadingBar]);

  // Fetch facilities and auto-select first if none selected
  useEffect(() => {
    facilityService.getAll().then((fetchedFacilities) => {
      setFacilities(fetchedFacilities);
      const firstFacility = fetchedFacilities[0];
      if (!facilityId && firstFacility?.id) {
        void navigate(buildUrl([date, firstFacility.id]), { replace: true });
      }
    }).catch(console.error);
  }, [facilityId, date, navigate]);

  // DERIVED STATE: Compute resolved provider from URL > memory > first
  const resolvedProviderId = useMemo(() => {
    if (!dailyLogData) return providerId ?? null;

    const providers = dailyLogData.providers ?? [];
    if (providers.length === 0) return null;

    // Priority: URL (if valid) > memory (if valid) > first
    if (providerId && providers.some(p => p.id === providerId)) {
      return providerId;
    }

    const memorized = getMemorizedProviderId(date, facilityId);
    if (memorized && providers.some(p => p.id === memorized)) {
      return memorized;
    }

    return providers[0]?.id ?? null;
  }, [dailyLogData, providerId, date, facilityId, getMemorizedProviderId]);

  // DERIVED STATE: Compute resolved visit from URL > memory > first
  const resolvedVisitId = useMemo(() => {
    if (!dailyLogData || !resolvedProviderId) return visitId ?? null;

    const visits = dailyLogData.visits ?? [];
    const providerVisits = visits.filter(v => v.providerIds?.includes(resolvedProviderId));
    if (providerVisits.length === 0) return null;

    // Priority: URL (if valid for this provider) > memory (if valid) > first
    if (visitId && providerVisits.some(v => v.id === visitId)) {
      return visitId;
    }

    const memorized = getMemorizedVisitId(date, facilityId, resolvedProviderId);
    if (memorized && providerVisits.some(v => v.id === memorized)) {
      return memorized;
    }

    return providerVisits[0]?.id ?? null;
  }, [dailyLogData, resolvedProviderId, visitId, date, facilityId, getMemorizedVisitId]);

  // Sync URL when resolved values differ from URL values
  useEffect(() => {
    if (!dailyLogData) return;

    const needsUpdate =
      (resolvedProviderId !== (providerId ?? null)) ||
      (resolvedVisitId !== (visitId ?? null));

    if (needsUpdate && resolvedProviderId !== null) {
      void navigate(buildUrl([date, facilityId, resolvedProviderId, resolvedVisitId]), { replace: true });
    }
  }, [dailyLogData, resolvedProviderId, resolvedVisitId, providerId, visitId, date, facilityId, navigate]);

  // Persist to memory when URL has valid values
  useEffect(() => {
    if (providerId && facilityId) {
      setMemorizedProviderId(date, facilityId, providerId);
    }
    if (providerId && visitId && facilityId) {
      setMemorizedVisitId(date, facilityId, providerId, visitId);
    }
  }, [date, facilityId, providerId, visitId, setMemorizedProviderId, setMemorizedVisitId]);

  // Handler: User changes provider dropdown
  const handleProviderChange = (newProviderId: number | null) => {
    if (!newProviderId) {
      void navigate(buildUrl([date, facilityId]), { replace: true });
      return;
    }

    // Compute visit for new provider inline
    const visits = dailyLogData?.visits ?? [];
    const providerVisits = visits.filter(v => v.providerIds?.includes(newProviderId));
    const memorized = getMemorizedVisitId(date, facilityId, newProviderId);
    const visitForProvider = (memorized && providerVisits.some(v => v.id === memorized))
      ? memorized
      : providerVisits[0]?.id ?? null;

    void navigate(buildUrl([date, facilityId, newProviderId, visitForProvider]), { replace: true });
  };

  // Handler: User clicks a visit
  const handleVisitSelect = (newVisitId: number | null) => {
    void navigate(buildUrl([date, facilityId, resolvedProviderId, newVisitId]), { replace: true });
  };

  // Handler: User changes facility
  const handleFacilityChange = (newFacilityId: number | null) => {
    setDailyLogData(null);
    const url = `/clinical/daily-log/${date}${newFacilityId ? `/${newFacilityId}` : ''}`;
    void navigate(url);
  };

  // Handler: User changes date
  const handleDateChange = (newDate: string) => {
    setDailyLogData(null);
    const url = `/clinical/daily-log/${newDate}${facilityId ? `/${facilityId}` : ''}`;
    void navigate(url, { replace: true });
  };

  // Find the selected visit object for display
  const selectedVisit = useMemo(() => {
    if (!resolvedVisitId) return null;
    return dailyLogData?.visits?.find(v => v.id === resolvedVisitId) ?? null;
  }, [resolvedVisitId, dailyLogData]);

  return (
    <div className="flex h-full bg-gray-50 overflow-hidden">
      {/* Left Column */}
      <DailyLogVisitFilters
        providerId={resolvedProviderId}
        visitId={resolvedVisitId}
        onProviderChange={handleProviderChange}
        onVisitIdChange={handleVisitSelect}
        date={date}
        onDateChange={handleDateChange}
        visits={dailyLogData?.visits ?? []}
        providers={dailyLogData?.providers ?? []}
        facilities={facilities}
        selectedFacilityId={facilityId}
        onFacilityChange={handleFacilityChange}
      />
      <div className="flex-1 flex flex-col bg-white overflow-hidden">
        {resolvedVisitId ? (
          <>
            <div className="flex flex-row h-full">
              {/* Main log entries panel */}
              <div className="w-3/5 flex flex-col h-full overflow-hidden">
                <DailyLogEntriesPanel visit={selectedVisit} />
              </div>
              {/* Fixed right panel */}
              <aside className="w-2/5 border-l border-gray-200 flex-shrink-0 flex flex-col items-center">
                <DailyLogPatientDetails visit={selectedVisit} />
              </aside>
            </div>
          </>
        ) : null}
      </div>
    </div>
  );
}
