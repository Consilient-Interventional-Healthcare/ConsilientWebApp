import { Button } from "@/shared/components/ui/button"
import { cn } from "@/shared/utils/utils"
import { Link, useLocation } from "react-router-dom"
import { navItems } from "@/shared/routes/Router"
import { useAuth } from "@/shared/hooks/useAuth"
import { useActiveNavItem } from "@/shared/hooks/useActiveNavItem"
import { LogOut } from "lucide-react"
import { Icon } from "@/shared/components/ui/icon";
import { appSettings } from '@/config/index';
import { ROUTES } from "@/constants";

function TopNavBar() {
  const location = useLocation();
  const { subNavItems } = useActiveNavItem();
  const { user } = useAuth(); // No need for logout or navigate here, as we will use a LinkWW

  return (
    <header className="sticky top-0 z-50 w-full border-b bg-white shadow-sm">
      {/* First Level Navigation */}
      <div className="flex h-16 items-center px-4 lg:px-8 gap-4">
        {/* Logo */}
        <div className="flex items-center gap-2 mr-6">
          <h1 className="text-xl font-bold text-blue-600 flex items-center gap-2">
            <img src="/logo.png" alt={appSettings.app.name} className="h-8 w-auto" />
          </h1>
        </div>
        
        {/* Navigation Links - Desktop */}
        <nav className="hidden md:flex items-center gap-1">
          {navItems.map((item) => {
            const isActive = item.href === "/" 
              ? location.pathname === "/" 
              : location.pathname.startsWith(item.href);

            let iconElement = null;
            if (item.icon) {
              iconElement = <Icon name={item.icon} className="w-5 h-5" aria-hidden="true" />;
            }

            return (
              <Link
                key={item.href}
                to={item.href}
                className={cn(
                  "px-4 py-2 rounded-md text-sm font-medium flex items-center gap-2",
                  "hover:bg-gray-100 transition-colors",
                  isActive
                    ? "text-blue-600 bg-blue-50"
                    : "text-gray-700 hover:text-gray-900"
                )}
              >
                {iconElement}
                {item.label}
              </Link>
            );
          })}
        </nav>
        
        <div className="flex-1" />
        
        {/* Actions */}
        <div className="flex items-center gap-2">
          {user && (
            <span className="text-sm text-gray-700 hidden md:inline">
              {user.userName}
            </span>
          )}
          <Link to={ROUTES.LOGOUT}>
            <Button variant="ghost" size="icon">
              <LogOut className="h-5 w-5" />
            </Button>
          </Link>
        </div>
      </div>

      {/* Second Level Navigation */}
      {subNavItems.length > 0 && (
        <div className="border-t bg-gray-50">
          <div className="flex items-center px-4 lg:px-8 gap-1 h-12 overflow-x-auto">
            {subNavItems.map((item) => {
              const isActive = location.pathname === item.href || location.pathname.startsWith(item.href + "/");
              let iconElement = null;
              if ('icon' in item && item.icon) {
                iconElement = <Icon name={item.icon} className="w-4 h-4" aria-hidden="true" />;
              }
              return (
                <Link
                  key={item.href}
                  to={item.href}
                  className={cn(
                    "px-4 py-2 rounded-md text-sm font-medium whitespace-nowrap flex items-center gap-2",
                    "hover:bg-gray-100 transition-colors",
                    isActive
                      ? "text-blue-600 bg-white shadow-sm"
                      : "text-gray-600 hover:text-gray-900"
                  )}
                >
                  {iconElement}
                  {item.label}
                </Link>
              );
            })}
          </div>
        </div>
      )}
    </header>
  )
}

export default TopNavBar;
