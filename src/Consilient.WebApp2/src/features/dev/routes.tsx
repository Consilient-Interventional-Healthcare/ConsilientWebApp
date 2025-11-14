import { lazy } from "react";
import type { RouteObject } from "react-router-dom";
import { config } from "@/config";

const LoggingTest = lazy(() => import("@/features/dev/views/LoggingTest"));

/**
 * Development-only routes
 * These routes are only available in development mode and will not be included in production builds
 */
// Development-only routes (only included in development mode)
export const devRoutes: RouteObject[] = config.env.isDevelopment
  ? [
  {
    path: "dev",
    handle: { 
      title: "Development Tools",
      protected: false,
    },
    children: [
      {
        path: "logging",
        element: <LoggingTest />,
        handle: { 
          title: "Logging Test",
          protected: false,
        },
      },
    ],
  },
] : [];
