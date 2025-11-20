import React, { lazy } from "react";
import type { RouteObject } from "react-router-dom";
import { redirect } from "react-router-dom";
function getTodayDate() {
  return new Date().toISOString().split('T')[0];
}

const ClinicalAssignmentsImport = lazy(() => import("@/features/clinical/assignments/views/Import"));

export const clinicalRoutes: RouteObject[] = [
  {
    path: "clinical",
    handle: { 
      label: "Clinical Management",
      title: "Clinical Management",
      icon: "hospital-user",
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
              icon: "file-import",
              protected: true,
            },
          },
        ],
      },
      {
        path: "daily-log",
        handle: {
          label: "Daily Log",
          title: "Daily Log",
          icon: "notes-medical",
          protected: true,
        },
        children: [
          {
            path: ":date?/:providerId?/:patientId?",
            element: React.createElement(lazy(() => import("@/features/clinical/daily-log/DailyLog"))),
            loader: ({ params }) => {
              if (!params['date']) {
                return redirect(`/clinical/daily-log/${getTodayDate()}`);
              }
              return params;
            },
            handle: {
              // No label so it doesn't show as a subnav
              title: "Daily Log",
              protected: true,
            },
          },
        ],
      },
    ],
  },
];
