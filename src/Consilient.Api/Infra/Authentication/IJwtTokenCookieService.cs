namespace Consilient.Api.Infra.Authentication
{
    /// <summary>
    /// Service for managing authentication token cookies with secure defaults.
    /// </summary>
    public interface IJwtTokenCookieService
    {
        /// <summary>
        /// Sets the authentication token as a secure HTTP-only cookie.
        /// </summary>
        /// <param name="response">The HTTP response to append the cookie to.</param>
        /// <param name="token">The JWT token to store.</param>
        void SetAuthenticationCookie(HttpResponse response, string token);

        /// <summary>
        /// Clears the authentication token cookie.
        /// </summary>
        /// <param name="response">The HTTP response to clear the cookie from.</param>
        void ClearAuthenticationCookie(HttpResponse response);

        /// <summary>
        /// Retrieves the authentication token from the request cookies.
        /// </summary>
        /// <param name="request">The HTTP request to read cookies from.</param>
        /// <returns>The authentication token if found; otherwise, null.</returns>
        string? GetAuthenticationToken(HttpRequest request);
    }
}