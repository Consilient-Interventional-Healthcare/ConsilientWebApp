import * as React from "react";
import { HOSPITALIZATION_STATUSES } from "@/features/clinical/daily-log/types/dailylog.types";

interface StatusComboBoxProps {
  value: number;
  onChange: (value: number) => void;
}

export function StatusComboBox({ value, onChange }: StatusComboBoxProps) {
  const [open, setOpen] = React.useState(false);
  const selectedStatus = HOSPITALIZATION_STATUSES.find(s => s.id === value);

  return (
    <div className="relative w-48">
      <button
        type="button"
        className="w-full border border-gray-300 rounded-lg px-4 py-2 text-sm font-medium bg-white flex items-center gap-2 focus:outline-none focus:ring-2 focus:ring-primary data-[state=on]:bg-primary data-[state=on]:text-white"
        onClick={() => setOpen((o) => !o)}
        tabIndex={0}
      >
        <span
          className="inline-block w-3 h-3 rounded-full mr-2"
          style={{ backgroundColor: selectedStatus?.color ?? "#ccc" }}
        />
        {selectedStatus?.name ?? "Select status"}
        <span className="ml-auto">▼</span>
      </button>
      {open && (
        <ul className="absolute z-10 mt-1 w-full bg-white border border-gray-300 rounded-lg shadow-lg">
          {HOSPITALIZATION_STATUSES.map((status) => (
            <li
              key={status.id}
              className="flex items-center gap-2 px-4 py-2 text-sm font-medium cursor-pointer hover:bg-primary hover:text-white rounded-md"
              onClick={() => { onChange(status.id); setOpen(false); }}
            >
              <span
                className="inline-block w-3 h-3 rounded-full"
                style={{ backgroundColor: status.color }}
              />
              <span>{status.name}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
