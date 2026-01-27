import { useNavigate, useLoaderData } from "react-router-dom";
import { useState, useEffect, useContext, useMemo } from "react";
import { DailyLogVisitFilters } from "./components/DailyLogVisitFilters";
import { DailyLogVisitFiltersV2 } from "./components/DailyLogVisitFiltersV2";
import { getDailyLogService } from "./services/DailyLogServiceFactory";
import LoadingBarContext from "@/shared/layouts/LoadingBarContext";
import DailyLogPatientDetails from "./components/DailyLogAdditionalInfo";
import type { DailyLogVisit, DailyLogVisitsResponse } from "./dailylog.types";
import { DailyLogEntriesPanel } from "./components/DailyLogEntriesPanel";
import { appSettings } from "@/config";

// At the top, outside the component:
const dailyLogService = getDailyLogService();

// Check if V2 Stage 1 is enabled
const useV2Stage1 = appSettings.features.dailyLogV2.stage1;

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

  // State for V2 response and legacy visits
  const [dailyLogData, setDailyLogData] = useState<DailyLogVisitsResponse | null>(null);
  const [visits, setVisits] = useState<DailyLogVisit[]>([]);

  const loadingBar = useContext(LoadingBarContext);

  useEffect(() => {
    if (!facilityId) {
      // No facilityId provided, can't fetch visits
      return;
    }

    loadingBar?.start();

    if (useV2Stage1) {
      // Use V2 service method
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
    } else {
      // Use legacy service method
      dailyLogService
        .getVisitsByDate(date)
        .then((data: DailyLogVisit[]) => {
          setVisits(data);
          console.log("Fetched visits:", data);
        })
        .catch((err: unknown) => {
          console.error("Failed to fetch visits", err);
        })
        .finally(() => {
          loadingBar?.complete();
        });
    }
  }, [date, facilityId, loadingBar]);

  // Helper to build URLs with facilityId
  const buildUrl = (parts: (string | number | null | undefined)[]) => {
    const filtered = parts.filter(p => p != null);
    return `/clinical/daily-log/${filtered.join('/')}`;
  };

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

  const visit = useMemo(() => {
    if (!selectedVisitId) return null;
    return visits.find((v) => v.id === selectedVisitId) ?? null;
  }, [selectedVisitId, visits]);

  return (
    <div className="flex h-full bg-gray-50 overflow-hidden">
      {/* Left Column - conditionally render V2 or legacy based on feature flag */}
      {useV2Stage1 ? (
        <DailyLogVisitFiltersV2
          visitId={selectedVisitId}
          onVisitIdChange={handleVisitSelect}
          date={date}
          {...(providerId && { providerId })}
          onProviderChange={handleProviderChange}
          visits={dailyLogData?.result.visits ?? []}
          providers={dailyLogData?.providers ?? []}
        />
      ) : (
        <DailyLogVisitFilters
          visitId={selectedVisitId}
          onVisitIdChange={handleVisitSelect}
          date={date}
          {...(providerId && { providerId })}
          onProviderChange={handleProviderChange}
          visits={visits}
        />
      )}
      <div className="flex-1 flex flex-col bg-white overflow-hidden">
        {selectedVisitId ? (
          <>
            <div className="flex flex-row h-full">
              {/* Main log entries panel */}
              <div className="w-3/5 flex flex-col h-full overflow-hidden">
                <DailyLogEntriesPanel visit={visit} />
              </div>
              {/* Fixed right panel */}
              <aside className="w-2/5 border-l border-gray-200 flex-shrink-0 flex flex-col items-center">
                <DailyLogPatientDetails visit={visit} />
              </aside>
            </div>
          </>
        ) : null}
      </div>
    </div>
  );
}
