namespace Consilient.Api.Infra.Authentication
{
    /// <summary>
    /// Factory for creating standardized secure cookie options.
    /// </summary>
    /// <remarks>
    /// <para><strong>SameSite.None Configuration Decision:</strong></para>
    /// <para>
    /// The auth token cookie uses SameSite.None to support cross-origin scenarios where
    /// the frontend is hosted on a different domain than the API.
    /// </para>
    /// <para><strong>When to use SameSite.None:</strong></para>
    /// <list type="bullet">
    /// <item>Frontend and API are on different domains (e.g., app.example.com and api.example.com)</item>
    /// <item>Local development with different ports (e.g., localhost:3000 and localhost:5000)</item>
    /// </list>
    /// <para><strong>Security Requirements:</strong></para>
    /// <list type="number">
    /// <item>HTTPS is required (Secure=true must be set)</item>
    /// <item>Additional CSRF protection must be implemented (handled via oauth_csrf cookie with SameSite.Lax)</item>
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
        /// Uses SameSite.Lax to provide CSRF protection while allowing GET navigation.
        /// </remarks>
        public static CookieOptions CreateCsrfTokenOptions(TimeSpan? maxAge = null)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // OAuth flows always require HTTPS
                SameSite = SameSiteMode.Lax,
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