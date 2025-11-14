import { lazy } from "react";
import type { RouteObject } from "react-router-dom";

const Dashboard = lazy(() => import("@/features/dashboard/views/Dashboard"));

export const dashboardRoutes: RouteObject[] = [
  {
    index: true,
    element: <Dashboard />,
    handle: { label: "Dashboard", title: "Dashboard", protected: true },
  },
];
