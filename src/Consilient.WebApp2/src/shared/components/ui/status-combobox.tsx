import { useHospitalizationStatuses } from "@/shared/stores/HospitalizationStatusStore";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./select";

interface StatusComboBoxProps {
  value: number | undefined;
  onChange: (value: number) => void;
}

function StatusDot({ color }: { color: string }) {
  return (
    <span
      className="inline-block size-2.5 rounded-full shrink-0"
      style={{ backgroundColor: color }}
    />
  );
}

export function StatusComboBox({ value, onChange }: StatusComboBoxProps) {
  const { data: statuses = [] } = useHospitalizationStatuses();
  const selectedStatus = statuses.find((s) => s.id === value);

  return (
    <Select
      value={value !== undefined ? value.toString() : ""}
      onValueChange={(val) => onChange(Number(val))}
    >
      <SelectTrigger className="w-48">
        <SelectValue placeholder="Select status">
          {selectedStatus && (
            <>
              <StatusDot color={selectedStatus.color ?? "#ccc"} />
              <span>
                {selectedStatus.name} ({selectedStatus.code})
              </span>
            </>
          )}
        </SelectValue>
      </SelectTrigger>
      <SelectContent>
        {statuses.map((status) => (
          <SelectItem key={status.id} value={status.id.toString()}>
            <StatusDot color={status.color ?? "#ccc"} />
            <span>
              {status.name} ({status.code})
            </span>
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
