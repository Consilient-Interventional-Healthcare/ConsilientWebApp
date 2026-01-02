namespace Consilient.Api.Infra.Authentication
{
    /// <summary>
    /// Standard cookie names used for authentication.
    /// </summary>
    internal static class AuthenticationCookieNames
    {
        /// <summary>
        /// Cookie name for JWT authentication token.
        /// </summary>
        public const string AuthToken = "auth_token";

        /// <summary>
        /// Cookie name for OAuth CSRF protection token.
        /// </summary>
        public const string OAuthCsrf = "oauth_csrf";
    }
}