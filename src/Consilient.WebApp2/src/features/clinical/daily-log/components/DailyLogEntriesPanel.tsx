import React, { useEffect, useState } from "react";
import { DailyLogEntriesHeader } from "./DailyLogEntriesHeader";
import { DailyLogEntriesDisplay } from "./DailyLogEntriesDisplay";
import { DailyLogEntriesInput } from "./DailyLogEntriesInput";
import type { DailyLogLogEntry, DailyLogVisit } from "../dailylog.types";
import { getDailyLogService } from "../services/DailyLogServiceFactory";
import { logEntryTypeProvider } from "../services/LogEntryTypeProvider";
import { useAuth } from "@/shared/hooks/useAuth";

interface DailyLogEntriesPanelProps {
  visit: DailyLogVisit | null;
}

const dailyLogService = getDailyLogService();

export const DailyLogEntriesPanel: React.FC<DailyLogEntriesPanelProps> = ({
  visit,
}) => {
  const { user } = useAuth();
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
    const type = logEntryTypeProvider.getLogEntryType();
    try {
      const newEntry = await dailyLogService.insertLogEntry(
        visit.id,
        content.trim(),
        user.id,
        type
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
