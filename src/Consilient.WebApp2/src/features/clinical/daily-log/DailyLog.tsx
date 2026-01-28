import { useNavigate, useLoaderData } from "react-router-dom";
import { useState, useEffect, useContext, useMemo } from "react";
import { DailyLogVisitFiltersV2 } from "./components/DailyLogVisitFiltersV2";
import { getDailyLogService } from "./services/DailyLogServiceFactory";
import LoadingBarContext from "@/shared/layouts/LoadingBarContext";
import DailyLogPatientDetails from "./components/DailyLogAdditionalInfo";
import type { DailyLogVisitsResponse } from "./dailylog.types";
import { DailyLogEntriesPanel } from "./components/DailyLogEntriesPanel";
import { facilityService } from "@/features/clinical/visits/services/FacilityService";
import type { Facilities } from "@/types/api.generated";

// At the top, outside the component:
const dailyLogService = getDailyLogService();

export default function DailyLog() {
  const { date, facilityId, providerId, visitId } = useLoaderData<{
    date: string;
    facilityId: number | null;
    providerId?: number;
    visitId?: number;
  }>();
  const navigate = useNavigate();
  const [selectedVisitId, setSelectedVisitId] = useState<number | null>(
    visitId ?? null
  );

  // State for V2 response
  const [dailyLogData, setDailyLogData] = useState<DailyLogVisitsResponse | null>(null);
  const [facilities, setFacilities] = useState<Facilities.FacilityDto[]>([]);

  const loadingBar = useContext(LoadingBarContext);

  // Helper to build URLs with facilityId
  const buildUrl = (parts: (string | number | null | undefined)[]) => {
    const filtered = parts.filter(p => p != null);
    return `/clinical/daily-log/${filtered.join('/')}`;
  };

  useEffect(() => {
    console.log("Visits useEffect triggered - date:", date, "facilityId:", facilityId);
    if (!facilityId) {
      // No facilityId provided, can't fetch visits
      return;
    }

    loadingBar?.start();

    console.log("Fetching V2 visits for date:", date, "facilityId:", facilityId);
    dailyLogService
      .getVisitsByDateV2(date, facilityId)
      .then((data: DailyLogVisitsResponse) => {
        setDailyLogData(data);
        console.log("Fetched V2 visits:", data);
      })
      .catch((err: unknown) => {
        console.error("Failed to fetch V2 visits", err);
      })
      .finally(() => {
        loadingBar?.complete();
      });
  }, [date, facilityId, loadingBar]);

  // Fetch facilities on mount and auto-select first if none selected
  useEffect(() => {
    facilityService.getAll().then((fetchedFacilities) => {
      setFacilities(fetchedFacilities);
      // Auto-select the first facility if none is selected
      const firstFacility = fetchedFacilities[0];
      if (!facilityId && firstFacility?.id) {
        void navigate(buildUrl([date, firstFacility.id]), { replace: true });
      }
    }).catch(console.error);
  }, [facilityId, date, navigate]);

  const handleProviderChange = (newProviderId: number | null) => {
    console.log(
      "handleProviderChange called with newProviderId:",
      newProviderId
    );
    if (!newProviderId) {
      // If no provider selected, clear visit selection as well
      setSelectedVisitId(null);
      void navigate(buildUrl([date, facilityId]), { replace: true });
      return;
    } else {
      // Filter visits for the selected provider using V2 data
      const visitsData = dailyLogData?.result.visits ?? [];
      const filteredVisits = visitsData.filter((v) => v.providerIds?.includes(newProviderId));

      // If selectedVisitId is not in filteredVisits, set to first visit for provider
      const hasSelectedVisit = filteredVisits.some(
        (v) => v.id === selectedVisitId
      );
      const firstVisitId = filteredVisits.length > 0 && filteredVisits[0] ? filteredVisits[0].id : null;
      const nextVisitId = hasSelectedVisit ? selectedVisitId : firstVisitId;
      setSelectedVisitId(nextVisitId);
      if (nextVisitId) {
        void navigate(buildUrl([date, facilityId, newProviderId, nextVisitId]), { replace: true });
      } else {
        void navigate(buildUrl([date, facilityId, newProviderId]), { replace: true });
      }
    }
  };

  const handleVisitSelect = (visitId: number | null) => {
    console.log("handleVisitSelect called with visitId:", visitId);
    setSelectedVisitId(visitId);
    // Find the providerId for the selected visit
    const visitsData = dailyLogData?.result.visits ?? [];
    const selectedVisit = visitsData.find((v) => v.id === visitId);
    const newProviderId = selectedVisit?.providerIds?.[0] ?? providerId;

    if (newProviderId) {
      void navigate(buildUrl([date, facilityId, newProviderId, visitId]), { replace: true });
    } else {
      void navigate(buildUrl([date, facilityId, visitId]), { replace: true });
    }
  };

  const handleFacilityChange = (newFacilityId: number | null) => {
    // Clear data first to prevent auto-select from navigating back with stale facilityId
    setDailyLogData(null);
    setSelectedVisitId(null);
    const url = `/clinical/daily-log/${date}${newFacilityId ? `/${newFacilityId}` : ''}`;
    console.log("handleFacilityChange - newFacilityId:", newFacilityId, "url:", url);
    void navigate(url);
  };

  const handleDateChange = (newDate: string) => {
    // Clear data first to prevent auto-select from navigating back with stale values
    setDailyLogData(null);
    setSelectedVisitId(null);
    const url = `/clinical/daily-log/${newDate}${facilityId ? `/${facilityId}` : ''}`;
    console.log("handleDateChange - newDate:", newDate, "facilityId:", facilityId, "url:", url);
    void navigate(url, { replace: true });
  };

  // Find the selected visit from V2 data
  const selectedVisit = useMemo(() => {
    if (!selectedVisitId) return null;
    return dailyLogData?.result.visits?.find(v => v.id === selectedVisitId) ?? null;
  }, [selectedVisitId, dailyLogData]);

  return (
    <div className="flex h-full bg-gray-50 overflow-hidden">
      {/* Left Column */}
      <DailyLogVisitFiltersV2
        visitId={selectedVisitId}
        onVisitIdChange={handleVisitSelect}
        date={date}
        onDateChange={handleDateChange}
        {...(providerId && { providerId })}
        onProviderChange={handleProviderChange}
        visits={dailyLogData?.result.visits ?? []}
        providers={dailyLogData?.providers ?? []}
        facilities={facilities}
        selectedFacilityId={facilityId}
        onFacilityChange={handleFacilityChange}
      />
      <div className="flex-1 flex flex-col bg-white overflow-hidden">
        {selectedVisitId ? (
          <>
            <div className="flex flex-row h-full">
              {/* Main log entries panel */}
              <div className="w-3/5 flex flex-col h-full overflow-hidden">
                <DailyLogEntriesPanel visit={selectedVisit} />
              </div>
              {/* Fixed right panel */}
              <aside className="w-2/5 border-l border-gray-200 flex-shrink-0 flex flex-col items-center">
                {/* Stage 3 - pass null until refactored to use V2 types */}
                <DailyLogPatientDetails visit={null} />
              </aside>
            </div>
          </>
        ) : null}
      </div>
    </div>
  );
}
