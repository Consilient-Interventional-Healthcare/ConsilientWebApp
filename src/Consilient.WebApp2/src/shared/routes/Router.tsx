import { createBrowserRouter } from "react-router-dom";
import { lazy } from "react";
import RootLayout from "@/shared/layouts/RootLayout";
import MainLayout from "@/shared/layouts/MainLayout";
import AuthLayout from "@/shared/layouts/AuthLayout";
import ProtectedRoute from "@/shared/routes/ProtectedRoute";
import PublicRoute from "@/shared/routes/PublicRoute";
import { dashboardRoutes } from "@/features/dashboard/routes";
import { clinicalRoutes } from "@/features/clinical/routes";
import { devRoutes } from "@/features/dev/routes";
import { type RouteHandle, type NavItem, type SubNavItem, isRouteHandle } from "./router.types";

// Lazy load views
const Login = lazy(() => import("@/features/auth/views/Login"));
const NotFound = lazy(() => import("@/shared/components/NotFound"));

export const router = createBrowserRouter([
  {
    path: "/",
    element: <RootLayout />,
    children: [
      // Protected routes (require authentication)
      {
        element: (
          <ProtectedRoute>
            <MainLayout />
          </ProtectedRoute>
        ),
        children: [
          ...dashboardRoutes,
          ...clinicalRoutes,
          {
            path: "billing",
            element: <div>Billing</div>,
            handle: { 
              label: "Billing",
              title: "Billing",
              protected: true,
            },
          },
        ],
      },
      // Public routes (redirect to dashboard if authenticated)
      {
        path: "auth",
        element: (
          <PublicRoute>
            <AuthLayout />
          </PublicRoute>
        ),
        children: [
          {
            path: "login",
            element: <Login />,
            handle: { title: "Sign In", protected: false },
          }
        ],
      },
      // Development-only routes (only included in development mode)
      ...devRoutes,
      // 404 catch-all route
      {
        path: "*",
        element: <NotFound />,
      },
    ],
  },
]);

// Generate navItems from router with subnav (only show items user has access to)
const firstRoute = router.routes[0];
const protectedRoute = firstRoute?.children?.[0];
const protectedChildren = protectedRoute?.children ?? [];

export const navItems: NavItem[] = protectedChildren
  .filter(route => {
    const handle = route.handle as unknown;
    return isRouteHandle(handle) && handle.label !== undefined;
  })
  .map(route => {
    // Safe to assert here since we filtered with type guard above
    const handle = route.handle as RouteHandle;
    const subNav: SubNavItem[] = [];
    
    // Extract subnav from nested children
    if (route.children) {
      const extractSubNav = (children: typeof route.children, parentPath = '') => {
        children.forEach(child => {
          const childHandle = child.handle as unknown;
          
          // Use type guard for safe checking
          if (isRouteHandle(childHandle) && childHandle.label && child.path) {
            const routePath = route.path ?? '';
            const childPath = child.path ?? '';
            const fullPath = route.path 
              ? `/${routePath}${parentPath}${childPath ? '/' + childPath : ''}` 
              : '/';
            
            subNav.push({
              href: fullPath,
              label: childHandle.label,
            });
          }
          
          if (child.children) {
            const childPath = child.path ?? '';
            extractSubNav(child.children, `${parentPath}${childPath ? '/' + childPath : ''}`);
          }
        });
      };
      
      extractSubNav(route.children);
    }
    
    const routePath = route.path ?? '';
    
    return {
      href: route.path ? `/${routePath}` : '/',
      label: handle.label ?? '',
      subNav,
    };
  });
