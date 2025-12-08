import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "@/shared/hooks/useAuth";
import { getAuthService } from "@/features/auth/services/AuthServiceFactory";
import { logger } from "@/shared/core/logging/Logger";
import { ROUTES } from "@/constants";

const authService = getAuthService();

export default function MicrosoftCallback() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { user } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const [isProcessing, setIsProcessing] = useState(true);

  useEffect(() => {
    const handleCallback = async () => {
      const code = searchParams.get("code");
      const state = searchParams.get("state");
      const errorParam = searchParams.get("error");
      const errorDescription = searchParams.get("error_description");

      // Check for errors from Microsoft
      if (errorParam) {
        logger.error("Microsoft login error", new Error(errorParam), {
          component: "MicrosoftCallback",
          errorDescription
        });
        setError(errorDescription || "Microsoft login failed");
        setIsProcessing(false);
        return;
      }

      // Check if we have the auth code
      if (!code) {
        logger.error("No authorization code received", undefined, {
          component: "MicrosoftCallback"
        });
        setError("No authorization code received from Microsoft");
        setIsProcessing(false);
        return;
      }

      try {
        logger.debug("Processing Microsoft callback", {
          component: "MicrosoftCallback",
          hasCode: !!code,
          hasState: !!state
        });

        // Send the code to the backend
        const result = await authService.handleMicrosoftCallback(code, state || undefined);

        if (result.success && result.userClaims) {
          logger.info("Microsoft login successful", {
            component: "MicrosoftCallback"
          });

          // The AuthProvider will pick up the user from the session cookie
          // Navigate to the intended destination (from state) or dashboard
          const redirectTo = state || ROUTES.DASHBOARD;
          navigate(redirectTo, { replace: true });
        } else {
          const errorMsg = result.errors.join(", ");
          logger.error("Microsoft authentication failed", new Error(errorMsg), {
            component: "MicrosoftCallback"
          });
          setError(errorMsg);
          setIsProcessing(false);
        }
      } catch (err) {
        logger.error("Microsoft callback processing failed", err as Error, {
          component: "MicrosoftCallback"
        });
        setError((err as Error).message || "Failed to process Microsoft login");
        setIsProcessing(false);
      }
    };

    // Only process if we don't already have a user
    if (!user) {
      void handleCallback();
    } else {
      // Already authenticated, redirect to dashboard
      navigate(ROUTES.DASHBOARD, { replace: true });
    }
  }, [searchParams, navigate, user]);

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen">
        <div className="max-w-md w-full p-6 bg-white rounded-lg shadow-md">
          <h2 className="text-2xl font-bold text-red-600 mb-4">Login Failed</h2>
          <p className="text-gray-700 mb-6">{error}</p>
          <button
            onClick={() => navigate(ROUTES.LOGIN)}
            className="w-full px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Back to Login
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-screen">
      <div className="max-w-md w-full p-6 bg-white rounded-lg shadow-md text-center">
        <div className="mb-4">
          <svg
            className="animate-spin h-12 w-12 mx-auto text-blue-600"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            ></circle>
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            ></path>
          </svg>
        </div>
        <h2 className="text-xl font-semibold text-gray-800 mb-2">
          {isProcessing ? "Completing Microsoft Sign In..." : "Signing you in..."}
        </h2>
        <p className="text-gray-600">Please wait while we authenticate your account.</p>
      </div>
    </div>
  );
}
