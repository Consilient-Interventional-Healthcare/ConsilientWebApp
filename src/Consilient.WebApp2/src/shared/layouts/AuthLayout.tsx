import { Outlet } from "react-router-dom";
import config from "@/config";
import type { FC } from "react";

const AuthLayout: FC = () => {
  return (
    <div className="flex min-h-screen items-center justify-center">
      <div className="w-full max-w-md p-8">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-blue-600">{config.app.name}</h1>
          <p className="text-gray-600 mt-2">Welcome back</p>
        </div>
        <div className="bg-white rounded-lg shadow-lg p-6">
          <Outlet />
        </div>
      </div>
    </div>
  );
};

export default AuthLayout;
