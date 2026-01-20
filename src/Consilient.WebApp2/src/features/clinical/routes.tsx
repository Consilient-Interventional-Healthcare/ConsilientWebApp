import React, { lazy } from "react";
import type { RouteObject } from "react-router-dom";
import { redirect } from "react-router-dom";
import { getTodayYYYYMMDD, formatDateFromUrl, isFuture } from '@/shared/utils/dateUtils';

function getTodayDate() {
  return new Date().toISOString().split('T')[0];
}

const Visits = lazy(() => import("@/features/clinical/visits/views/Visits"));
const Assignments = lazy(() => import("@/features/clinical/assignments/views/Assignments"));

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
        loader: () => redirect("/clinical/visits"),
      },
      {
        path: "visits/:date?/:facilityId?",
        element: <Visits />,
        loader: ({ params }) => {
          const today = getTodayYYYYMMDD();
          const date = params['date'];

          // If missing or future date, redirect to today
          if (!date || isFuture(formatDateFromUrl(date))) {
            const facilityPart = params['facilityId'] ? `/${params['facilityId']}` : '';
            return redirect(`/clinical/visits/${today}${facilityPart}`);
          }
          return {
            date,
            facilityId: params['facilityId'] ? Number(params['facilityId']) : null
          };
        },
        handle: {
          label: "Visits",
          title: "Visits",
          protected: true,
        }
      },
      {
        path: "assignments/:id",
        element: <Assignments />,
        handle: {
          title: "Assignment",
          protected: true,
        }
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
            path: ":date?/:providerId?/:visitId?",
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
