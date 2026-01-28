import React, { useEffect, useState } from "react";
import { DailyLogEntriesHeader } from "./DailyLogEntriesHeader";
import { DailyLogEntriesDisplay } from "./DailyLogEntriesDisplay";
import { DailyLogEntriesInput } from "./DailyLogEntriesInput";
import type { DailyLogLogEntryV2 } from "../dailylog.types";
import type { GraphQL } from "@/types/api.generated";
import { getDailyLogService } from "../services/DailyLogServiceFactory";
import { useAuth } from "@/shared/hooks/useAuth";
import { useVisitEventTypes } from "@/shared/stores/VisitEventTypeStore";

interface DailyLogEntriesPanelProps {
  visit: GraphQL.DailyLogVisit | null;
}

const dailyLogService = getDailyLogService();

export const DailyLogEntriesPanel: React.FC<DailyLogEntriesPanelProps> = ({
  visit,
}) => {
  const { user } = useAuth();
  const { data: eventTypes = [] } = useVisitEventTypes();
  const [logEntries, setLogEntries] = useState<DailyLogLogEntryV2[]>([]);
  const [typeFilter, setTypeFilter] = useState<string>("all");

  useEffect(() => {
    if (visit?.id) {
      dailyLogService.getLogEntriesByVisitIdV2(visit.id)
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
      const newEntry = await dailyLogService.insertLogEntryV2(
        visit.id,
        content.trim(),
        parseInt(user.id, 10),
        eventTypeId
      );
      setLogEntries((prev) => [...prev, newEntry]);
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
