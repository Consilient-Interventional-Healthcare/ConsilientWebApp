import { Outlet } from "react-router-dom";
import TopNavBar from "@/components/TopNavBar";
import type { FC } from "react";

const MainLayout: FC = () => {
  return (
    <>
      <TopNavBar />
      <main>
        <Outlet />
      </main>
    </>
  );
};

export default MainLayout;
