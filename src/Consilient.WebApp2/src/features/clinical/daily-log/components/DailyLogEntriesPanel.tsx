import React, { useEffect, useState } from "react";
import { DailyLogEntriesHeader } from "./DailyLogEntriesHeader";
import { DailyLogEntriesDisplay } from "./DailyLogEntriesDisplay";
import { DailyLogEntriesInput } from "./DailyLogEntriesInput";
import type { DailyLogLogEntry } from "../services/IDailyLogService";
import type { GraphQL } from "@/types/api.generated";
import { dailyLogService } from "../services/DailyLogService";
import { useAuth } from "@/shared/hooks/useAuth";
import { useVisitEventTypes } from "@/shared/stores/VisitEventTypeStore";

interface DailyLogEntriesPanelProps {
  visit: GraphQL.DailyLogVisit | null;
}

export const DailyLogEntriesPanel: React.FC<DailyLogEntriesPanelProps> = ({
  visit,
}) => {
  const { user } = useAuth();
  const { data: eventTypes = [] } = useVisitEventTypes();
  const [logEntries, setLogEntries] = useState<DailyLogLogEntry[]>([]);
  const [typeFilter, setTypeFilter] = useState<string>("all");

  useEffect(() => {
    if (visit?.id) {
      dailyLogService.getLogEntriesByVisitId(visit.id)
        .then(setLogEntries)
        .catch((error) => {
          console.error("Failed to fetch log entries:", error);
          setLogEntries([]);
        });
    } else {
      setLogEntries([]);
    }
  }, [visit?.id]);

  const handleAddLogEntry = async (content: string) => {
    if (!content.trim() || !visit || !user) return;

    // Find the event type ID from the selected filter
    // Default to first event type if "all" is selected
    let eventTypeId: number;
    if (typeFilter === "all") {
      eventTypeId = eventTypes[0]?.id ?? 1;
    } else {
      const selectedType = eventTypes.find(t => t.code === typeFilter);
      eventTypeId = selectedType?.id ?? 1;
    }

    try {
      await dailyLogService.insertVisitEvent(visit.id, {
        visitId: visit.id,
        eventTypeId,
        description: content.trim(),
      });
      // Refetch entries to get complete data with user info
      const entries = await dailyLogService.getLogEntriesByVisitId(visit.id);
      setLogEntries(entries);
    } catch (error) {
      console.error("Failed to add log entry:", error);
    }
  };

  return (
    <div className="flex flex-col h-full">
      <DailyLogEntriesHeader
        visit={visit}
        typeFilter={typeFilter}
        onTypeFilterChange={setTypeFilter}
      />
      <div className="flex-1 overflow-y-auto">
        <DailyLogEntriesDisplay entries={logEntries} typeFilter={typeFilter} />
      </div>
      <div className="mt-4">
        <DailyLogEntriesInput onSubmit={handleAddLogEntry} />
      </div>
    </div>
  );
};
