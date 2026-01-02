namespace Consilient.Api.Infra.Authentication
{
    /// <summary>
    /// Service for managing CSRF token cookies to prevent cross-site request forgery attacks.
    /// </summary>
    public interface ICsrfTokenCookieService
    {
        /// <summary>
        /// Clears the CSRF token cookie from the response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        void ClearCookie(HttpResponse response);

        /// <summary>
        /// Generates a cryptographically secure CSRF token and sets it as an HTTP-only cookie.
        /// </summary>
        /// <param name="response">The HTTP response to append the cookie to.</param>
        /// <returns>The generated CSRF token value.</returns>
        string GenerateAndSetCookie(HttpResponse response);

        /// <summary>
        /// Retrieves and validates the CSRF token from the request cookies.
        /// </summary>
        /// <param name="request">The HTTP request to read cookies from.</param>
        /// <returns>The CSRF token if found and valid; otherwise, null.</returns>
        string? GetAndValidateFromCookie(HttpRequest request);
    }
}