import { useNavigate, useLoaderData } from "react-router-dom";
import { useState, useEffect, useContext, useMemo } from "react";
import { DailyLogVisitFilters } from "./components/DailyLogVisitFilters";
import { getDailyLogService } from "./services/DailyLogServiceFactory";
import LoadingBarContext from "@/shared/layouts/LoadingBarContext";
import DailyLogPatientDetails from "./components/DailyLogAdditionalInfo";
import type { DailyLogVisit } from "./dailylog.types";
import { useAuth } from "@/shared/hooks/useAuth";
import { DailyLogEntriesPanel } from "./components/DailyLogEntriesPanel";

// At the top, outside the component:
const dailyLogService = getDailyLogService();

export default function DailyLog() {
  const { date, providerId, visitId } = useLoaderData<{
    date: string;
    providerId?: number;
    visitId?: number;
  }>();
  const navigate = useNavigate();
  const [selectedVisitId, setSelectedVisitId] = useState<number | null>(
    visitId ?? null
  );
  const [visits, setVisits] = useState<DailyLogVisit[]>([]);
  const loadingBar = useContext(LoadingBarContext);
  const { user } = useAuth();
  if (!user) {
    throw new Error("User must be authenticated to access Daily Log");
  }
  useEffect(() => {
    loadingBar?.start();
    dailyLogService
      .getVisitsByDate(date)
      .then((data: DailyLogVisit[]) => {
        setVisits(data);
        console.log("Fetched visits:", data);
      })
      .catch((err) => {
        console.error("Failed to fetch visits", err);
      })
      .finally(() => {
        loadingBar?.complete();
      });
  }, [date, loadingBar]);

  const handleProviderChange = (newProviderId: number | null) => {
    console.log(
      "handleProviderChange called with newProviderId:",
      newProviderId
    );
    if (!newProviderId) {
      // If no provider selected, clear visit selection as well
      setSelectedVisitId(null);
      void navigate(`/clinical/daily-log/${date}`, { replace: true });
      return;
    } else {
      // Filter visits for the selected provider
      const filteredVisits = visits.filter((v) => v.providerId === newProviderId);

      // If selectedVisitId is not in filteredVisits, set to first visit for provider
      const hasSelectedVisit = filteredVisits.some(
        (v) => v.id === selectedVisitId
      );
      const firstVisitId = filteredVisits.length > 0 && filteredVisits[0] ? filteredVisits[0].id : null;
      const nextVisitId = hasSelectedVisit ? selectedVisitId : firstVisitId;
      setSelectedVisitId(nextVisitId);
      if (nextVisitId) {
        void navigate(
          `/clinical/daily-log/${date}/${newProviderId}/${nextVisitId}`,
          { replace: true }
        );
      } else {
        void navigate(`/clinical/daily-log/${date}/${newProviderId}`, {
          replace: true,
        });
      }
    }
  };

  const handleVisitSelect = (visitId: number | null) => {
    console.log("handleVisitSelect called with visitId:", visitId);
    setSelectedVisitId(visitId);
    // Find the providerId for the selected visit
    const selectedVisit = visits.find((v) => v.id === visitId);
    const newProviderId = selectedVisit?.providerId ?? providerId; // Use the visit's providerId, or existing providerId if not found

    if (newProviderId) {
      void navigate(`/clinical/daily-log/${date}/${newProviderId}/${visitId}`, {
        replace: true,
      });
    } else {
      // If no providerId is found for the selected visit and no existing providerId, navigate without it
      void navigate(`/clinical/daily-log/${date}/${visitId}`, {
        replace: true,
      });
    }
  };

  const visit = useMemo(() => {
    if (!selectedVisitId) return null;
    return visits.find((v) => v.id === selectedVisitId) ?? null;
  }, [selectedVisitId, visits]);

  return (
    <div className="flex h-full bg-gray-50 overflow-hidden">
      <DailyLogVisitFilters
        visitId={selectedVisitId}
        onVisitIdChange={handleVisitSelect}
        date={date}
        {...(providerId && { providerId })}
        onProviderChange={handleProviderChange}
        visits={visits}
      />
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
