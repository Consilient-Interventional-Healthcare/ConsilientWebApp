using Consilient.Users.Services.OAuth;

namespace Consilient.Api.Infra.Authentication
{
    internal class CsrfTokenCookieService : ICsrfTokenCookieService
    {
        public string GenerateAndSetCookie(HttpResponse response)
        {
            var token = CryptographicTokenGenerator.Generate(OAuthSecurityConstants.TokenSizeBytes);
            var maxAge = TimeSpan.FromMinutes(OAuthSecurityConstants.CsrfTokenExpirationMinutes);

            var cookieOptions = CookieOptionsFactory.CreateCsrfTokenOptions(maxAge);

            response.Cookies.Append(AuthenticationCookieNames.OAuthCsrf, token, cookieOptions);
            return token;
        }

        public string? GetAndValidateFromCookie(HttpRequest request)
        {
            if (request.Cookies.TryGetValue(AuthenticationCookieNames.OAuthCsrf, out var token)
                && !string.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            return null;
        }

        public void ClearCookie(HttpResponse response)
        {
            var baseOptions = CookieOptionsFactory.CreateCsrfTokenOptions();
            var cookieOptions = CookieOptionsFactory.CreateDeletionOptions(baseOptions);

            response.Cookies.Delete(AuthenticationCookieNames.OAuthCsrf, cookieOptions);
        }
    }
}