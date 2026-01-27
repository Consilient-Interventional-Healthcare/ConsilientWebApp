namespace Consilient.Api.Configuration;

// Constants used for rate-limiting defaults across the app.
internal static class RateLimitingConstants
{
    // Global fixed-window
    public const int GlobalPermitLimit = 100;
    public const int GlobalWindowSeconds = 60;

    // Authenticate endpoint (token bucket)
    public const int AuthenticateTokenLimit = 20;
    public const int AuthenticateWindowSeconds = 60;

    // Link external endpoint (token bucket)
    public const int LinkExternalTokenLimit = 10;
    public const int LinkExternalWindowSeconds = 60;

    // OAuth login endpoint (fixed window)
    public const int OAuthLoginPermitLimit = 10;
    public const int OAuthLoginWindowMinutes = 1;

    // OAuth callback endpoint (fixed window)
    public const int OAuthCallbackPermitLimit = 10;
    public const int OAuthCallbackWindowMinutes = 1;

    // Policy names
    public const string AuthenticatePolicy = "AuthenticatePolicy";
    public const string LinkExternalPolicy = "LinkExternalPolicy";
    public const string OAuthLoginPolicy = "OAuthLoginPolicy";
    public const string OAuthCallbackPolicy = "OAuthCallbackPolicy";
}
