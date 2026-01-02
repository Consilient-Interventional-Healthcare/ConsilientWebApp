import { useMemo } from 'react';
import { useLocation } from 'react-router-dom';
import { type NavItem } from '@/shared/routes/router.types';
import { navItems } from '@/shared/routes/Router';

interface UseActiveNavItemReturn {
  activeNavItem: NavItem | undefined;
  subNavItems: { href: string; label: string; icon?: string }[];
}

/**
 * Hook to determine the active navigation item based on current location
 * @returns {UseActiveNavItemReturn} Active nav item and its subnav items
 */
export function useActiveNavItem(): UseActiveNavItemReturn {
  const location = useLocation();

  const activeNavItem = useMemo(() => {
    return navItems.find(item => {
      if (item.href === "/" && location.pathname === "/") return true;
      if (item.href !== "/" && location.pathname.startsWith(item.href)) return true;
      return false;
    });
  }, [location.pathname]);

  const subNavItems = activeNavItem?.subNav?.map(({ href, label, icon }) => ({ href, label, icon })) ?? [];

  return { activeNavItem, subNavItems };
}