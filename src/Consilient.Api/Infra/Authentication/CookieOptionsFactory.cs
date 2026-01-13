namespace Consilient.Api.Infra.Authentication
{
    /// <summary>
    /// Factory for creating standardized secure cookie options.
    /// </summary>
    /// <remarks>
    /// <para><strong>SameSite.None Configuration Decision:</strong></para>
    /// <para>
    /// Both auth token and CSRF cookies use SameSite.None to support cross-origin scenarios where
    /// the frontend is hosted on a different domain than the API.
    /// </para>
    /// <para><strong>When to use SameSite.None:</strong></para>
    /// <list type="bullet">
    /// <item>Frontend and API are on different domains (e.g., app.example.com and api.example.com)</item>
    /// <item>Local development with different ports (e.g., localhost:3000 and localhost:5000)</item>
    /// <item>OAuth flows where the navigation chain crosses site boundaries</item>
    /// </list>
    /// <para><strong>Security Requirements:</strong></para>
    /// <list type="number">
    /// <item>HTTPS is required (Secure=true must be set)</item>
    /// <item>CSRF protection via server-side state validation during OAuth flows</item>
    /// <item>Proper CORS policies must restrict credentialed requests to trusted origins</item>
    /// </list>
    /// <para><strong>Alternative Configuration:</strong></para>
    /// <para>
    /// If your frontend and API are on the same origin, consider changing to SameSite.Lax
    /// or SameSite.Strict for improved CSRF protection.
    /// </para>
    /// </remarks>
    internal static class CookieOptionsFactory
    {
        /// <summary>
        /// Creates secure cookie options for authentication tokens.
        /// </summary>
        /// <remarks>
        /// Uses SameSite.None to support cross-origin requests. Review deployment architecture
        /// before production to ensure this is the correct setting for your use case.
        /// </remarks>
        public static CookieOptions CreateAuthTokenOptions(
            HttpContext httpContext,
            bool isProduction,
            TimeSpan? maxAge = null)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction || httpContext.Request.IsHttps,
                // NOTE: SameSite.None is used for cross-origin scenarios (frontend on different domain).
                // Change to SameSite.Lax if frontend and API are on the same origin.
                SameSite = SameSiteMode.None,
                IsEssential = true,
                Path = "/",
                MaxAge = maxAge,
                Expires = maxAge.HasValue ? DateTimeOffset.UtcNow.Add(maxAge.Value) : null
            };
        }

        /// <summary>
        /// Creates secure cookie options for OAuth CSRF tokens.
        /// </summary>
        /// <remarks>
        /// Uses SameSite.None to support cross-origin OAuth flows where the navigation chain
        /// crosses site boundaries (Frontend → API → OAuth Provider → API callback).
        /// Modern browsers (Chrome 143+) block SameSite.Lax cookies in this scenario due to
        /// third-party cookie deprecation policies.
        ///
        /// Security is maintained because:
        /// - The CSRF token is validated against server-side state
        /// - The cookie is short-lived (15 minutes)
        /// - The cookie is cleared immediately after OAuth completion
        /// - Secure=true is required for SameSite.None
        /// </remarks>
        public static CookieOptions CreateCsrfTokenOptions(TimeSpan? maxAge = null)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Required for SameSite.None
                SameSite = SameSiteMode.None, // Required for cross-origin OAuth flows
                IsEssential = true,
                Path = "/",
                MaxAge = maxAge,
                Expires = maxAge.HasValue ? DateTimeOffset.UtcNow.Add(maxAge.Value) : null
            };
        }

        /// <summary>
        /// Creates cookie options for deletion (expired).
        /// </summary>
        public static CookieOptions CreateDeletionOptions(CookieOptions baseOptions)
        {
            baseOptions.Expires = DateTimeOffset.UtcNow.AddDays(-1);
            baseOptions.MaxAge = TimeSpan.Zero;
            return baseOptions;
        }
    }
}