import { useState, useEffect } from "react";
import { useNavigate, useLocation, useSearchParams } from "react-router-dom";
import { Button } from "@/shared/components/ui/button";
import { useAuth } from "@/shared/hooks/useAuth";
import { logger } from "@/shared/core/logging/Logger";
import { ROUTES } from "@/constants";
import { getAuthService } from "@/features/auth/services/AuthServiceFactory";
import { AppSettingsServiceFactory } from "@/shared/core/appSettings/AppSettingsServiceFactory";

const authService = getAuthService();

export default function Login() {
  const { login, isLoading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [sessionExpired, setSessionExpired] = useState(false);
  const [externalLoginEnabled, setExternalLoginEnabled] = useState(false);

  // Get the redirect path from:
  // 1. URL query parameter (from 401 error handler)
  // 2. Location state (from ProtectedRoute)
  // 3. Default to dashboard
  const redirectParam = searchParams.get('redirect');
  const stateFrom = (location.state as { from?: string })?.from;
  const from = redirectParam ?? stateFrom ?? ROUTES.DASHBOARD;

  // Check if user was redirected due to session expiration or OAuth error
  useEffect(() => {
    // Check for OAuth error from callback
    const errorParam = searchParams.get('error');
    if (errorParam) {
      setError(decodeURIComponent(errorParam));
      logger.error('OAuth authentication error', new Error(errorParam), { component: 'Login' });
      return; // Don't process other logic if there's an error
    }

    if (redirectParam) {
      setSessionExpired(true);
      logger.info('User redirected to login due to session expiration', { component: 'Login' });
    }
  }, [redirectParam, searchParams]);

  // Fetch app settings to check if external login is enabled
  useEffect(() => {
    const loadSettings = async () => {
      try {
        const settings = await AppSettingsServiceFactory.create().getAppSettings();
        setExternalLoginEnabled(settings.externalLoginEnabled);
      } catch (error) {
        logger.error("Failed to load app settings", error as Error, { component: "Login" });
      }
    };
    void loadSettings();
  }, []);


  const handleMicrosoftLogin = () => {
    setError(null);
    try {
      logger.debug('Initiating Microsoft login', { component: 'Login', destination: from });
      // Pass login URL as returnUrl so errors come back to login page
      // Include the intended destination in the URL so we can navigate there on success
      const frontendBaseUrl = `${window.location.protocol}//${window.location.host}`;
      const returnUrl = `${frontendBaseUrl}/auth/login${from !== ROUTES.DASHBOARD ? `?redirect=${encodeURIComponent(from)}` : ''}`;
      authService.initiateMicrosoftLogin(returnUrl);
      // The page will redirect, no need to return anything
    } catch (err) {
      logger.error("Microsoft login initiation failed", err as Error, { component: "Login" });
      setError((err as Error).message || "Failed to initiate Microsoft login");
    }
  };

  const handleRegularLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      if (!username || !password) {
        setError("Please enter both username and password.");
        return;
      }
      await login({ userName: username, password });
      // Navigate to the intended destination after successful login
      logger.debug('Login component - Navigating after login', { component: 'Login', destination: from });
      void navigate(from, { replace: true });
    } catch (error) {
      logger.error("Login failed", error as Error, { component: "Login" });
      setError((error as Error).message || "Login failed. Please try again.");
    } finally {
      logger.debug('Login component - Regular login attempt finished', { component: 'Login' });
    }
  };

  return (
    <div>
      <h2 className="text-2xl font-bold text-gray-900 mb-6 text-center">Sign In</h2>

      {sessionExpired && (
        <div className="mb-4 p-3 bg-yellow-50 border border-yellow-200 rounded text-yellow-800 text-sm">
          Your session has expired. Please sign in again to continue.
        </div>
      )}

      <form className="space-y-4 mb-6" onSubmit={handleRegularLogin}>
        <input
          type="text"
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          className="w-full px-3 py-2 border rounded"
          autoComplete="username"
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          className="w-full px-3 py-2 border rounded"
          autoComplete="current-password"
        />
        {error && <div className="text-red-600 text-sm">{error}</div>}
        <Button type="submit" className="w-full h-11" disabled={isLoading}>
          {isLoading ? "Signing in..." : "Sign in"}
        </Button>
      </form>

      {externalLoginEnabled && (
        <div className="space-y-3">
          <Button
            onClick={handleMicrosoftLogin}
            variant="outline"
            className="w-full flex items-center justify-center gap-3 h-11"
          >
            <svg className="w-5 h-5" viewBox="0 0 23 23">
              <path fill="#f35325" d="M0 0h11v11H0z" />
              <path fill="#81bc06" d="M12 0h11v11H12z" />
              <path fill="#05a6f0" d="M0 12h11v11H0z" />
              <path fill="#ffba08" d="M12 12h11v11H12z" />
            </svg>
            Continue with Microsoft
          </Button>
        </div>
      )}
    </div>
  );
}
