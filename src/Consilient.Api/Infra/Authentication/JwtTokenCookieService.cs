using Consilient.Api.Configuration;
using Consilient.Api.Infra.Contracts;
using Microsoft.Extensions.Options;

namespace Consilient.Api.Infra.Authentication;

/// <summary>
/// Default implementation of authentication token cookie management.
/// </summary>
internal class JwtTokenCookieService(
    IOptions<AuthenticationOptions> authOptions,
    IWebHostEnvironment environment,
    ILogger<JwtTokenCookieService> logger) : IJwtTokenCookieService
{
    private readonly AuthenticationOptions _authOptions = authOptions?.Value ?? throw new ArgumentNullException(nameof(authOptions));
    private readonly IWebHostEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    private readonly ILogger<JwtTokenCookieService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public void SetAuthenticationCookie(HttpResponse response, string token)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var expiryMinutes = _authOptions.CookieExpiryMinutes;
        var maxAge = TimeSpan.FromMinutes(expiryMinutes);

        var options = CookieOptionsFactory.CreateAuthTokenOptions(
            response.HttpContext,
            _environment.IsProduction(),
            maxAge);

        response.Cookies.Append(AuthenticationCookieNames.AuthToken, token, options);

        _logger.LogDebug(
            "Authentication cookie set successfully. Expiry: {ExpiryMinutes} minutes, Secure: {IsSecure}",
            expiryMinutes,
            options.Secure);
    }

    public void ClearAuthenticationCookie(HttpResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var baseOptions = CookieOptionsFactory.CreateAuthTokenOptions(
            response.HttpContext,
            _environment.IsProduction());

        var options = CookieOptionsFactory.CreateDeletionOptions(baseOptions);

        response.Cookies.Delete(AuthenticationCookieNames.AuthToken, options);
        _logger.LogDebug("Authentication cookie cleared successfully");
    }

    public string? GetAuthenticationToken(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Cookies.TryGetValue(AuthenticationCookieNames.AuthToken, out var token)
            && !string.IsNullOrWhiteSpace(token))
        {
            _logger.LogTrace("Authentication token retrieved from cookie");
            return token;
        }

        _logger.LogTrace("No authentication token found in cookies");
        return null;
    }
}