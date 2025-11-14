import { lazy } from "react";
import type { RouteObject } from "react-router-dom";

const ClinicalAssignmentsImport = lazy(() => import("@/features/clinical/assignments/views/Import"));

export const clinicalRoutes: RouteObject[] = [
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
];
