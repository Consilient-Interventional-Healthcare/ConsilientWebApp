import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { useAuth } from "@/shared/hooks/useAuth";
import { logger } from "@/shared/core/logging/Logger";
import { msalService } from "@/features/auth/services/MsalService";

export default function Login() {
  const { login } = useAuth();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleMicrosoftLogin = async () => {
    logger.info("Microsoft login initiated", { component: "Login", action: "oauth_init" });
    try {
      await login();
    } catch (error) {
      logger.error("Microsoft login failed", error as Error, { component: "Login" });
      // Error is already logged, MSAL will handle redirect
    }
  };

  const handleRegularLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      if (!username || !password) {
        setError("Please enter both username and password.");
        setLoading(false);
        return;
      }
      await login({ username, password });
      setUsername("");
      setPassword("");
      setError(null);
    } catch (err) {
      setError("Login failed. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const isMsalConfigured = msalService.isConfigured();

  return (
    <div>
      <h2 className="text-2xl font-bold text-gray-900 mb-6 text-center">Sign In</h2>

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
        <Button type="submit" className="w-full h-11" disabled={loading}>
          {loading ? "Signing in..." : "Sign in"}
        </Button>
      </form>

      <div className="space-y-3">
        {isMsalConfigured && (
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
        )}
      </div>
    </div>
  );
}
