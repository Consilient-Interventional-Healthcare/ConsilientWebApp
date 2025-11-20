import { Outlet } from "react-router-dom";
import TopNavBar from "@/shared/components/TopNavBar";
import type { FC } from "react";

const MainLayout: FC = () => {
  return (
    <div className="h-screen flex flex-col overflow-hidden">
      <TopNavBar />
      <main className="flex-1 overflow-hidden">
        <Outlet />
      </main>
    </div>
  );
};

export default MainLayout;
