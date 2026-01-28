import * as React from "react"
import { cn } from "@/shared/utils/utils"
import { ChevronDownIcon } from "lucide-react"

interface NativeSelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  size?: "sm" | "default"
}

const NativeSelect = React.forwardRef<HTMLSelectElement, NativeSelectProps>(
  ({ className, size = "default", children, ...props }, ref) => {
    return (
      <div className="relative">
        <select
          ref={ref}
          className={cn(
            // Match Radix SelectTrigger styling
            "appearance-none w-full",
            "border-input rounded-md border bg-transparent px-3 py-2 pr-8 text-sm shadow-xs",
            "focus-visible:outline-none focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px]",
            "disabled:cursor-not-allowed disabled:opacity-50",
            size === "default" && "h-9",
            size === "sm" && "h-8",
            className
          )}
          {...props}
        >
          {children}
        </select>
        <ChevronDownIcon className="absolute right-2 top-1/2 -translate-y-1/2 size-4 opacity-50 pointer-events-none" />
      </div>
    )
  }
)
NativeSelect.displayName = "NativeSelect"

export { NativeSelect }
