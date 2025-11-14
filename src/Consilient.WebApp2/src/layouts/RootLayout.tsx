import { Outlet, useMatches } from "react-router-dom";
import { useEffect, Suspense, type FC } from "react";
import config from "@/config";

interface RouteHandle {
  title?: string;
}

const RootLayout: FC = () => {
  const matches = useMatches();

  useEffect(() => {
    const currentMatch = matches[matches.length - 1];
    const handle = currentMatch?.handle as RouteHandle | undefined;
    const title = handle?.title;

    if (title) {
      document.title = `${title} - ${config.app.name}`;
    } else {
      document.title = config.app.name;
    }
  }, [matches]);

  return (
    <div className="min-h-screen bg-gray-50">
      <Suspense fallback={
        <div className="flex items-center justify-center min-h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900"></div>
        </div>
      }>
        <Outlet />
      </Suspense>
    </div>
  );
};

export default RootLayout;
