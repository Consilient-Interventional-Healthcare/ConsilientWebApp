using Consilient.BackgroundHost.Configuration;
using Hangfire.Dashboard;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Consilient.BackgroundHost.Infra.Security
{
    /// <summary>
    /// Hangfire dashboard authorization filter that validates JWT tokens.
    /// Users must be authenticated via the same JWT mechanism as the API.
    /// The token is read from the auth_token cookie (same as API).
    /// </summary>
    internal class JwtAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly bool _authEnabled;

        public JwtAuthorizationFilter(IOptions<AuthenticationSettings> authOptions)
        {
            var authSettings = authOptions?.Value ?? throw new ArgumentNullException(nameof(authOptions));
            _authEnabled = authSettings.DashboardAuthEnabled;

            var jwtSettings = authSettings.UserService?.Jwt;
            _secret = jwtSettings?.Secret ?? string.Empty;
            _issuer = jwtSettings?.Issuer ?? string.Empty;
            _audience = jwtSettings?.Audience ?? string.Empty;
        }

        public bool Authorize(DashboardContext context)
        {
            // If auth is disabled (dev mode), allow access
            if (!_authEnabled)
            {
                return true;
            }

            // If no secret is configured, deny access (misconfiguration)
            if (string.IsNullOrEmpty(_secret))
            {
                return false;
            }

            var httpContext = context.GetHttpContext();

            // Try to get token from cookie (same as API)
            if (!httpContext.Request.Cookies.TryGetValue("auth_token", out var token) ||
                string.IsNullOrEmpty(token))
            {
                // Also check Authorization header for API clients
                var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
                {
                    token = authHeader.Substring(7);
                }
                else
                {
                    return false;
                }
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
                    ValidateIssuer = !string.IsNullOrEmpty(_issuer),
                    ValidIssuer = _issuer,
                    ValidateAudience = !string.IsNullOrEmpty(_audience),
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (SecurityTokenException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Legacy filter that allows all access (for local development only).
    /// </summary>
    internal class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }
}
