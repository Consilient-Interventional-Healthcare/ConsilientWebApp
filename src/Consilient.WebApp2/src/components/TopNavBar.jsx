import { Bell, User, Search, Menu } from "lucide-react"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"
import { Link, useLocation } from "react-router-dom"
import { navItems } from "../routes"
import { useMemo } from "react"

export function TopNavBar() {
  const location = useLocation();
  
  // Find active nav item and its subnav
  const activeNavItem = useMemo(() => {
    return navItems.find(item => {
      if (item.href === "/" && location.pathname === "/") return true;
      if (item.href !== "/" && location.pathname.startsWith(item.href)) return true;
      return false;
    });
  }, [location.pathname]);

  const subNavItems = activeNavItem?.subNav || [];
  
  return (
    <header className="sticky top-0 z-50 w-full border-b bg-white shadow-sm">
      {/* First Level Navigation */}
      <div className="flex h-16 items-center px-4 lg:px-8 gap-4">
        {/* Logo */}
        <div className="flex items-center gap-2 mr-6">
          <h1 className="text-xl font-bold text-blue-600">Consilient</h1>
        </div>
        
        {/* Navigation Links - Desktop */}
        <nav className="hidden md:flex items-center gap-1">
          {navItems.map((item) => {
            const isActive = item.href === "/" 
              ? location.pathname === "/" 
              : location.pathname.startsWith(item.href);
            
            return (
              <Link
                key={item.href}
                to={item.href}
                className={cn(
                  "px-4 py-2 rounded-md text-sm font-medium",
                  "hover:bg-gray-100 transition-colors",
                  isActive
                    ? "text-blue-600 bg-blue-50"
                    : "text-gray-700 hover:text-gray-900"
                )}
              >
                {item.label}
              </Link>
            );
          })}
        </nav>
        
        <div className="flex-1" />
        
        {/* Actions */}
        <div className="flex items-center gap-2">
          <Button variant="ghost" size="icon">
            <User className="h-5 w-5" />
          </Button>
          <Button variant="ghost" size="icon" className="md:hidden">
            <Menu className="h-5 w-5" />
          </Button>
        </div>
      </div>

      {/* Second Level Navigation */}
      {subNavItems.length > 0 && (
        <div className="border-t bg-gray-50">
          <div className="flex items-center px-4 lg:px-8 gap-1 h-12 overflow-x-auto">
            {subNavItems.map((item) => (
              <Link
                key={item.href}
                to={item.href}
                className={cn(
                  "px-4 py-2 rounded-md text-sm font-medium whitespace-nowrap",
                  "hover:bg-gray-100 transition-colors",
                  location.pathname === item.href
                    ? "text-blue-600 bg-white shadow-sm"
                    : "text-gray-600 hover:text-gray-900"
                )}
              >
                {item.label}
              </Link>
            ))}
          </div>
        </div>
      )}
    </header>
  )
}