import { Button } from "@/shared/components/ui/button";
import { useAuth } from "@/shared/hooks/useAuth";
import { logger } from "@/shared/core/logging/logger";

export default function Login() {
  const { login } = useAuth();

  const handleMicrosoftLogin = async () => {
    logger.info('Microsoft login initiated', { component: 'Login', action: 'oauth_init' });
    try {
      await login();
    } catch (error) {
      logger.error('Microsoft login failed', error as Error, { component: 'Login' });
      // Error is already logged, MSAL will handle redirect
    }
  };

  return (
    <div>
      <h2 className="text-2xl font-bold text-gray-900 mb-6 text-center">
        Sign In
      </h2>

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
    </div>
  );
}
