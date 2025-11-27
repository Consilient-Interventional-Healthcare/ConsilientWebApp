import * as React from "react";
import { ToggleGroup, ToggleGroupItem } from "@/shared/components/ui/toggle-group";

interface SegmentedControlProps {
  options: { label: string; value: string }[];
  defaultValue?: string;
  onChange?: (value: string) => void;
}

export function SegmentedControl({ options, defaultValue, onChange }: SegmentedControlProps) {
  const [value, setValue] = React.useState(defaultValue ?? options[0]?.value ?? "");

  const handleChange = (val: string) => {
    setValue(val);
    onChange?.(val);
  };

  return (
    <ToggleGroup type="single" value={value} onValueChange={handleChange} className="rounded-lg p-1">
      {options.map((opt) => (
        <ToggleGroupItem
          key={opt.value}
          value={opt.value}
          className="px-4 py-2 text-sm font-medium bg-white data-[state=on]:bg-accent data-[state=on]:text-white rounded-md border border-gray-300"
        >
          {opt.label}
        </ToggleGroupItem>
      ))}
    </ToggleGroup>
  );
}
