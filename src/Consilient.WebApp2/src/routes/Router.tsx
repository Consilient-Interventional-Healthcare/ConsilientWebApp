import { createBrowserRouter } from "react-router-dom";
import { lazy } from "react";
import RootLayout from "@/layouts/RootLayout";
import MainLayout from "@/layouts/MainLayout";
import AuthLayout from "@/layouts/AuthLayout";
import ProtectedRoute from "@/routes/ProtectedRoute";
import PublicRoute from "@/routes/PublicRoute";
import { isDevelopment } from "@/config/env";

// Lazy load page components
const Dashboard = lazy(() => import("@/views/dashboard/Dashboard"));
const ClinicalAssignmentsImport = lazy(() => import("@/views/clinical/assignments/Import"));
const LoggingTest = lazy(() => import("@/views/test/LoggingTest"));
const Login = lazy(() => import("@/views/auth/Login"));
const NotFound = lazy(() => import("@/views/NotFound"));

export interface RouteHandle {
  label?: string;
  title?: string;
  protected?: boolean;
}

interface SubNavItem {
  href: string;
  label: string;
}

export interface NavItem {
  href: string;
  label: string;
  subNav: SubNavItem[];
}

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
          {
            index: true,
            element: <Dashboard />,
            handle: { label: "Dashboard", title: "Dashboard", protected: true },
          },
          {
            path: "clinical",
            handle: { 
              label: "Clinical Management",
              title: "Clinical Management",
              protected: true,
            },
            children: [
              {
                index: true,
                element: <div>Clinical Management Overview</div>,
              },
              {
                path: "assignments",
                children: [
                  {
                    path: "import",
                    element: <ClinicalAssignmentsImport />,
                    handle: { 
                      label: "Import Assignments",
                      title: "Import Assignments",
                      protected: true,
                    },
                  },
                ],
              },
            ],
          },
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
      // Public test routes (development only)
      ...(isDevelopment ? [{
        path: "test/logging",
        element: <LoggingTest />,
        handle: { 
          title: "Logging Test",
          protected: false,
        },
      }] : []),
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
  .filter(route => (route.handle as RouteHandle)?.label)
  .map(route => {
    const subNav: SubNavItem[] = [];
    
    // Extract subnav from nested children
    if (route.children) {
      const extractSubNav = (children: typeof route.children, parentPath = '') => {
        children.forEach(child => {
          const routePath = route.path ?? '';
          const childPath = child.path ?? '';
          const fullPath = route.path ? `/${routePath}${parentPath}${childPath ? '/' + childPath : ''}` : '/';
          
          if ((child.handle as RouteHandle)?.label && child.path) {
            subNav.push({
              href: fullPath,
              label: (child.handle as RouteHandle).label ?? '',
            });
          }
          
          if (child.children) {
            extractSubNav(child.children, `${parentPath}${childPath ? '/' + childPath : ''}`);
          }
        });
      };
      
      extractSubNav(route.children);
    }
    
    const routePath = route.path ?? '';
    
    return {
      href: route.path ? `/${routePath}` : '/',
      label: (route.handle as RouteHandle).label ?? '',
      subNav,
    };
  });
