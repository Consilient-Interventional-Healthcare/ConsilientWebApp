import * as React from "react";
import { ToggleGroup, ToggleGroupItem } from "@/shared/components/ui/toggle-group";
import { DynamicIcon } from "../DynamicIcon";
interface SegmentedControlOption {
  label: string;
  value: string;
  icon?: string;
  color?: string;
}
interface SegmentedControlProps {
  options: SegmentedControlOption[];
  defaultValue?: string;
  onChange?: ((value: string) => void) | undefined;
}

export function SegmentedControl({ options, defaultValue, onChange }: SegmentedControlProps) {
  const [value, setValue] = React.useState(defaultValue ?? options[0]?.value ?? "");

  const handleChange = (val: string) => {
    setValue(val);
    onChange?.(val);
  };

  return (
    <ToggleGroup type="single" value={value} onValueChange={handleChange}>
      {options.map((opt) => (
        <ToggleGroupItem
          key={opt.value}
          value={opt.value}
          className="px-4 py-2 text-sm font-medium bg-white data-[state=on]:bg-accent data-[state=on]:text-white rounded-md border border-gray-300 flex items-center gap-2"
        >
          {opt.icon && (
            <DynamicIcon
              iconName={opt.icon}
              style={{ color: opt.color ?? undefined }}
              className="mr-1"
            />
          )}
          {opt.label}
        </ToggleGroupItem>
      ))}
    </ToggleGroup>
  );
}
