import { Outlet, useMatches } from "react-router-dom";
import { useEffect, Suspense, type FC, useRef } from "react";
import LoadingBar from "react-top-loading-bar";
import LoadingBarContext from "./LoadingBarContext";
import type { LoadingBarApi } from "./LoadingBarContext";
import type { LoadingBarRef } from "react-top-loading-bar";
import { appSettings } from '@/config/index';
import { AuthProvider } from '@/features/auth/services/AuthProvider';

// Context moved to LoadingBarContext.ts

interface RouteHandle {
  title?: string;
}

const RootLayout: FC = () => {
  const matches = useMatches();
  const loadingBarRef = useRef<LoadingBarRef | null>(null);

  useEffect(() => {
    const currentMatch = matches[matches.length - 1];
    const handle = currentMatch?.handle as RouteHandle | undefined;
    const title = handle?.title;

    if (title) {
      document.title = `${title} - ${appSettings.app.name}`;
    } else {
      document.title = appSettings.app.name;
    }
  }, [matches]);

  const loadingBarApi: LoadingBarApi = {
    start: () => {
      if (loadingBarRef.current) loadingBarRef.current.continuousStart();
    },
    complete: () => {
      if (loadingBarRef.current) loadingBarRef.current.complete();
    },
  };

  return (
    <AuthProvider>
      <LoadingBarContext.Provider value={loadingBarApi}>
        <LoadingBar color="#2563eb" ref={loadingBarRef} height={3} shadow={true} />
        <div className="h-screen bg-gray-50 overflow-hidden">
          <Suspense fallback={
            <div className="flex items-center justify-center h-screen">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900"></div>
            </div>
          }>
            <Outlet />
          </Suspense>
        </div>
      </LoadingBarContext.Provider>
    </AuthProvider>
  );
};

export default RootLayout;
