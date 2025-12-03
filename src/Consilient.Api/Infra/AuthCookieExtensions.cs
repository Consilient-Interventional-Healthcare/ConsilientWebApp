namespace Consilient.Api.Infra
{
    internal static class AuthCookieExtensions
    {
        public const string AuthCookieName = "auth_token";

        public static void AppendAuthTokenCookie(this HttpResponse response, string token, int expiryMinutes, bool persistent = false)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = response.HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.None,
                IsEssential = true, // if you use consent features
                Path = "/"
            };

            if (persistent)
            {
                var expirySpan = TimeSpan.FromMinutes(expiryMinutes);
                options.MaxAge = expirySpan;
                options.Expires = DateTimeOffset.UtcNow.AddMinutes(expirySpan.TotalMinutes);
            }

            response.Cookies.Append(AuthCookieName, token, options);
        }

        public static void DeleteAuthTokenCookie(this HttpResponse response)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = response.HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.None,
                Path = "/",
                // Ensure deletion is handled by both Expires and MaxAge for broader compatibility
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
                MaxAge = TimeSpan.Zero
            };

            response.Cookies.Delete(AuthCookieName, options);
        }
    }
}