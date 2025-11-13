import { TopNavBar } from "@/components/TopNavBar"
import { cn } from "@/lib/utils"

export function FluidLayout({ children, className }) {
  return (
    <div className="min-h-screen flex flex-col">
      {/* Top Navigation */}
      <TopNavBar />
      
      {/* Main Content - Full Width */}
      <main className={cn(
        "flex-1 w-full",
        className
      )}>
        {children}
      </main>
    </div>
  )
}