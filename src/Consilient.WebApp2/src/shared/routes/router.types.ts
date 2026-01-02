/**
 * Route metadata for navigation and page titles
 */
export interface RouteHandle {
  label?: string;
  title?: string;
  protected?: boolean;
  icon?: string;
}

/**
 * Sub-navigation item
 */
export interface SubNavItem {
  href: string;
  label: string;
  icon?: string;
}


/**
 * Main navigation item with optional sub-navigation
 */
export interface NavItem {
  href: string;
  label: string;
  icon?: string | undefined;
  subNav: SubNavItem[];
}

/**
 * Type guard to safely check if a value is a RouteHandle
 * Prevents runtime errors from unsafe type assertions
 */
export function isRouteHandle(handle: unknown): handle is RouteHandle {
  return (
    typeof handle === 'object' &&
    handle !== null &&
    ('label' in handle || 'title' in handle || 'protected' in handle)
  );
}
