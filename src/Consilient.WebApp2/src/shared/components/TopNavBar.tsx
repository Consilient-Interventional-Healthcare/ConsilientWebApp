import { Button } from "@/shared/components/ui/button"
import { cn } from "@/shared/utils/utils"
import { Link, useLocation } from "react-router-dom"
import { navItems } from "@/shared/routes/Router"
import { useAuth } from "@/shared/hooks/useAuth"
import { useActiveNavItem } from "@/shared/hooks/useActiveNavItem"
import { useNavigate } from "react-router-dom"
import { LogOut } from "lucide-react"
import config from "@/config"

function TopNavBar() {
  const location = useLocation();
  const { subNavItems } = useActiveNavItem();
  const { logout, user } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    void navigate('/auth/login');
  };

  return (
    <header className="sticky top-0 z-50 w-full border-b bg-white shadow-sm">
      {/* First Level Navigation */}
      <div className="flex h-16 items-center px-4 lg:px-8 gap-4">
        {/* Logo */}
        <div className="flex items-center gap-2 mr-6">
          <h1 className="text-xl font-bold text-blue-600">{config.app.name}</h1>
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
          {user && (
            <span className="text-sm text-gray-700 hidden md:inline">
              {user.firstName} {user.lastName}
            </span>
          )}
          <Button variant="ghost" size="icon" onClick={handleLogout}>
            <LogOut className="h-5 w-5" />
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

export default TopNavBar;
