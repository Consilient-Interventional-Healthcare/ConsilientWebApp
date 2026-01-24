using Consilient.Api.Configuration;
using Consilient.Api.Infra.Contracts;
using Consilient.Users.Contracts;
using Consilient.Users.Contracts.OAuth;
using Consilient.Users.Services.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Consilient.Api.Controllers
{
    /// <summary>
    /// API response for authentication operations.
    /// </summary>
    //public record AuthenticateUserApiResponse(
    //    bool Succeeded,
    //    IEnumerable<string>? Errors = null,
    //    CurrentUserDto? User = null);

    /// <summary>
    /// Handles authentication and authorization operations.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class AuthController(
        IUserService _userService,
        IOAuthStateManager _stateManager,
        ICsrfTokenCookieService _csrfTokenService,
        IOAuthProviderRegistry _providerRegistry,
        IJwtTokenCookieService _jwtTokenCookieService,
        IOptions<RedirectValidationOptions> _redirectValidationOptions,
        ILogger<AuthController> _logger) : ControllerBase
    {
        private readonly string[] _allowedOrigins =
    _redirectValidationOptions?.Value?.AllowedOrigins
    ?? throw new InvalidOperationException(
        "RedirectValidationOptions.AllowedOrigins must be configured in application settings.");

        /// <summary>
        /// Gets available OAuth providers.
        /// </summary>
        [HttpGet("oauth/providers")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetOAuthProviders()
        {
            var providers = _providerRegistry.GetSupportedProviders()
                .Select(p => new
                {
                    name = p,
                    loginUrl = $"/auth/{p.ToLowerInvariant()}/login",
                    callbackUrl = $"/auth/{p.ToLowerInvariant()}/callback"
                });

            return Ok(providers);
        }

        /// <summary>
        /// Authenticates a user with username and password.
        /// </summary>
        [HttpPost("authenticate")]
        [EnableRateLimiting(RateLimitingConstants.AuthenticatePolicy)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticateUserResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthenticateUserResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Authenticate(
            [FromBody] AuthenticateUserRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null)
            {
                return BadRequest("Request body is required.");
            }

            return HandleAuthenticationResult(await _userService.AuthenticateUserAsync(request, cancellationToken));
        }

        /// <summary>
        /// Authenticates a user via external OAuth provider.
        /// </summary>
        [HttpPost("external")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticateUserResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthenticateUserResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExternalAuthenticate(
            [FromBody] ExternalAuthenticateRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null)
            {
                return BadRequest("Request body is required.");
            }

            return HandleAuthenticationResult(
                await _userService.AuthenticateExternalAsync(request, cancellationToken));
        }

        /// <summary>
        /// Gets the currently authenticated user.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser(
            CancellationToken cancellationToken)
        {
            if (User.Identity?.Name is not { } userName)
            {
                return Unauthorized("User identity not found.");
            }

            var result = await _userService.GetCurrentUserAsync(userName, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Links an external login to the current user account.
        /// </summary>
        [HttpPost("link-external")]
        [EnableRateLimiting(RateLimitingConstants.LinkExternalPolicy)]
        [ProducesResponseType(typeof(LinkExternalLoginResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LinkExternal(
            [FromBody] LinkExternalLoginRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null)
            {
                return BadRequest("Request body is required.");
            }

            var result = await _userService.LinkExternalLoginAsync(request, cancellationToken);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Logs out the current user by clearing the authentication cookie.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            _jwtTokenCookieService.ClearAuthenticationCookie(Response);
            return Ok();
        }

        /// <summary>
        /// OAuth callback endpoint for any provider.
        /// Enhanced with CSRF protection.
        /// </summary>
        [HttpGet("{provider}/callback")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingConstants.OAuthCallbackPolicy)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> OAuthCallback(
            string provider,
            [FromQuery] string? code,
            [FromQuery] string? state,
            [FromQuery] string? error,
            [FromQuery] string? error_description,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("OAuth callback received for provider: {Provider}, code present: {CodePresent}, error: {Error}",
                provider, !string.IsNullOrWhiteSpace(code), error);
            _logger.LogDebug("OAuth callback received for provider: {Provider}", provider);
            _logger.LogDebug("Callback parameters - Code present: {CodePresent}, State present: {StatePresent}, Error: {Error}",
                !string.IsNullOrWhiteSpace(code), !string.IsNullOrWhiteSpace(state), error);

            // Validate provider
            if (!_providerRegistry.IsProviderSupported(provider))
            {
                _logger.LogWarning("OAuth callback received for unsupported provider: {Provider}", provider);
                return BadRequest($"OAuth provider '{provider}' is not supported.");
            }

            // Handle OAuth provider errors
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logger.LogWarning(
                    "OAuth provider {Provider} returned error: {Error}, Description: {Description}",
                    provider, error, error_description);

                return await HandleOAuthErrorAsync(state, error, error_description, cancellationToken);
            }

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            {
                _logger.LogWarning(
                    "OAuth callback for {Provider} missing required parameters. Code present: {CodePresent}, State present: {StatePresent}",
                    provider, !string.IsNullOrWhiteSpace(code), !string.IsNullOrWhiteSpace(state));
                return BadRequest("Missing code or state parameter.");
            }

            _logger.LogDebug("Authorization code received (length: {CodeLength}), State: {State}", code.Length, state);

            // Retrieve CSRF token from cookie
            var csrfToken = _csrfTokenService.GetAndValidateFromCookie(Request);
            _logger.LogDebug("CSRF token from cookie: {HasCsrf}", !string.IsNullOrWhiteSpace(csrfToken));
            if (string.IsNullOrWhiteSpace(csrfToken))
            {
                _logger.LogWarning("CSRF token missing from cookie during OAuth callback for {Provider}", provider);
                return BadRequest("CSRF validation failed: token not found.");
            }

            // Validate state and CSRF token
            _logger.LogDebug("Validating state and CSRF token");
            var validationResult = await _stateManager.ValidateAndConsumeAsync(
                state,
                csrfToken,
                cancellationToken);

            _logger.LogDebug("State validation result - Valid: {IsValid}, ReturnUrl: {ReturnUrl}, HasCodeVerifier: {HasCodeVerifier}",
                validationResult.IsValid, validationResult.ReturnUrl, !string.IsNullOrWhiteSpace(validationResult.CodeVerifier));

            // Clear CSRF cookie regardless of validation result
            _csrfTokenService.ClearCookie(Response);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("State validation failed for {Provider}: {Error}", provider, validationResult.ErrorMessage);
                return BadRequest(validationResult.ErrorMessage ?? "Invalid or expired state token.");
            }

            if (string.IsNullOrWhiteSpace(validationResult.CodeVerifier))
            {
                _logger.LogError("Code verifier not found in validated state for {Provider}", provider);
                return BadRequest("Code verifier not found.");
            }

            // Construct dynamic redirect URI from current request (matching the one used in login flow)
            // Use X-Forwarded-Proto header when behind reverse proxy (Azure App Service)
            var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
            var redirectUri = $"{scheme}://{Request.Host}{Request.PathBase}/auth/{provider.ToLowerInvariant()}/callback";
            _logger.LogDebug("Constructed callback redirect URI: {RedirectUri} (scheme: {Scheme}, host: {Host})",
                redirectUri, scheme, Request.Host);

            // Authenticate with OAuth provider
            _logger.LogDebug("Calling UserService.AuthenticateExternalAsync to exchange code for token");
            var authResult = await _userService.AuthenticateExternalAsync(
                new ExternalAuthenticateRequest(provider, code, validationResult.CodeVerifier, redirectUri),
                cancellationToken);

            _logger.LogDebug("AuthenticateExternalAsync result - Succeeded: {Succeeded}, HasToken: {HasToken}, Errors: {Errors}",
                authResult.Succeeded, !string.IsNullOrWhiteSpace(authResult.Token),
                authResult.Errors != null ? string.Join(", ", authResult.Errors) : "none");

            if (authResult.Succeeded)
            {
                _jwtTokenCookieService.SetAuthenticationCookie(Response, authResult.Token!);

                _logger.LogInformation(
                    "OAuth authentication successful for {Provider}, redirecting to: {ReturnUrl}",
                    provider, validationResult.ReturnUrl);

                // Build absolute URL for cross-origin redirect to frontend
                var redirectUrl = BuildAbsoluteReturnUrl(validationResult.ReturnUrl);
                _logger.LogDebug("Built absolute return URL: {RedirectUrl}", redirectUrl);

                if (!string.IsNullOrWhiteSpace(redirectUrl))
                {
                    _logger.LogInformation("Redirecting to frontend: {RedirectUrl}", redirectUrl);
                    return Redirect(redirectUrl);
                }

                _logger.LogWarning("Could not build valid redirect URL, using default frontend origin");
                return Redirect(_allowedOrigins.FirstOrDefault() ?? "/");
            }

            // Handle authentication failure
            var errorMsg = authResult.Errors?.FirstOrDefault() ?? "Authentication failed";
            _logger.LogWarning("OAuth authentication failed for {Provider}: {Error}", provider, errorMsg);

            return await RedirectWithErrorAsync(validationResult.ReturnUrl, errorMsg);
        }

        /// <summary>
        /// Initiates OAuth login flow for any provider with CSRF protection.
        /// </summary>
        [HttpGet("{provider}/login")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingConstants.OAuthLoginPolicy)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> OAuthLogin(
            string provider,
            [FromQuery] string? returnUrl,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("OAuth login request received for provider: {Provider}, returnUrl: {ReturnUrl}", provider, returnUrl);
            _logger.LogDebug("OAuth login initiated for provider: {Provider}, returnUrl: {ReturnUrl}", provider, returnUrl);

            // Validate provider
            if (!_providerRegistry.IsProviderSupported(provider))
            {
                _logger.LogWarning("OAuth login attempted for unsupported provider: {Provider}", provider);
                return BadRequest($"OAuth provider '{provider}' is not supported.");
            }

            try
            {
                // Generate PKCE parameters
                var codeVerifier = PkceHelper.GenerateCodeVerifier();
                var codeChallenge = PkceHelper.GenerateCodeChallenge(codeVerifier);
                _logger.LogDebug("PKCE parameters generated - CodeVerifier length: {VerifierLength}, CodeChallenge length: {ChallengeLength}",
                    codeVerifier.Length, codeChallenge.Length);

                // Generate and set CSRF token cookie
                var csrfToken = _csrfTokenService.GenerateAndSetCookie(Response);
                _logger.LogDebug("CSRF token generated and set in cookie");

                // Construct dynamic redirect URI from current request
                // Use X-Forwarded-Proto header when behind reverse proxy (Azure App Service)
                var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
                var redirectUri = $"{scheme}://{Request.Host}{Request.PathBase}/auth/{provider.ToLowerInvariant()}/callback";
                _logger.LogDebug("Constructed redirect URI: {RedirectUri} (scheme: {Scheme}, host: {Host}, pathBase: {PathBase})",
                    redirectUri, scheme, Request.Host, Request.PathBase);

                // Normalize returnUrl to be relative path only
                var normalizedReturnUrl = NormalizeReturnUrl(returnUrl);
                _logger.LogDebug("Normalized return URL: {NormalizedReturnUrl}", normalizedReturnUrl);

                // Store state with code verifier and CSRF token
                var state = await _stateManager.GenerateStateAsync(
                    normalizedReturnUrl,
                    codeVerifier,
                    csrfToken,
                    cancellationToken);
                _logger.LogDebug("State token generated: {State}", state);

                // Build authorization URL with PKCE challenge and dynamic redirect URI
                var authUrl = await _userService.BuildAuthorizationUrlAsync(
                    provider,
                    state,
                    codeChallenge,
                    redirectUri,
                    cancellationToken);

                _logger.LogInformation("Initiating OAuth login flow for {Provider} with state: {State}, returnUrl: {ReturnUrl}",
                    provider, state, normalizedReturnUrl);
                _logger.LogDebug("Redirecting to authorization URL: {AuthUrl}", authUrl);

                return Redirect(authUrl);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to initiate OAuth login for {Provider}", provider);
                return Problem(ex.Message);
            }
        }

        // Private helper methods

        private IActionResult HandleAuthenticationResult(AuthenticateUserResult result)
        {
            if (result.Succeeded)
            {
                _jwtTokenCookieService.SetAuthenticationCookie(Response, result.Token!);
                return Ok(result);
            }
            return Unauthorized(result);
        }

        private async Task<IActionResult> HandleOAuthErrorAsync(
            string? state,
            string error,
            string? errorDescription,
            CancellationToken cancellationToken)
        {
            string? returnUrl = null;

            // Try to get returnUrl from state if available
            if (!string.IsNullOrWhiteSpace(state))
            {
                var csrfToken = _csrfTokenService.GetAndValidateFromCookie(Request);
                if (!string.IsNullOrWhiteSpace(csrfToken))
                {
                    var validationResult = await _stateManager.ValidateAndConsumeAsync(
                        state,
                        csrfToken,
                        cancellationToken);

                    if (validationResult.IsValid)
                    {
                        returnUrl = validationResult.ReturnUrl;
                    }
                }

                // Clear CSRF cookie
                _csrfTokenService.ClearCookie(Response);
            }

            var encodedError = Uri.EscapeDataString(errorDescription ?? error);
            var redirectUrl = BuildAbsoluteReturnUrl(returnUrl);

            if (!string.IsNullOrWhiteSpace(redirectUrl))
            {
                var separator = redirectUrl.Contains('?') ? "&" : "?";
                return Redirect($"{redirectUrl}{separator}error={encodedError}");
            }

            _logger.LogWarning("Could not build valid error redirect URL, using default");
            var defaultUrl = _allowedOrigins.FirstOrDefault() ?? "/";
            return Redirect($"{defaultUrl}?error={encodedError}");
        }

        private Task<IActionResult> RedirectWithErrorAsync(string? returnUrl, string errorMessage)
        {
            var encodedError = Uri.EscapeDataString(errorMessage);

            var absoluteUrl = BuildAbsoluteReturnUrl(returnUrl);
            if (!string.IsNullOrWhiteSpace(absoluteUrl))
            {
                var separator = absoluteUrl.Contains('?') ? "&" : "?";
                return Task.FromResult<IActionResult>(Redirect($"{absoluteUrl}{separator}error={encodedError}"));
            }

            _logger.LogWarning(
                "Invalid return URL detected for error redirect, using default. URL: {ReturnUrl}",
                returnUrl);
            var defaultOrigin = _allowedOrigins.FirstOrDefault() ?? "/";
            return Task.FromResult<IActionResult>(Redirect($"{defaultOrigin}?error={encodedError}"));
        }

        /// <summary>
        /// Validates that the returnUrl is a relative path or from an allowed origin to prevent open redirect attacks.
        /// </summary>
        //private bool IsValidReturnUrl(string? url)
        //{
        //    if (string.IsNullOrWhiteSpace(url))
        //    {
        //        return false;
        //    }

        //    // Allow relative URLs (starting with /)
        //    if (url.StartsWith('/') && !url.StartsWith("//"))
        //    {
        //        return true;
        //    }

        //    // For absolute URLs, validate against allowed origins
        //    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        //    {
        //        return false;
        //    }

        //    var urlOrigin = $"{uri.Scheme}://{uri.Authority}";
        //    return _allowedOrigins.Any(origin =>
        //        origin.Equals(urlOrigin, StringComparison.OrdinalIgnoreCase));
        //}

        /// <summary>
        /// Normalizes a return URL to be relative, extracting the path from absolute URLs.
        /// </summary>
        private string NormalizeReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return "/";
            }

            // If it's already a relative URL, return it as-is
            if (returnUrl.StartsWith('/') && !returnUrl.StartsWith("//"))
            {
                return returnUrl;
            }

            // If it's an absolute URL, extract the path + query + fragment
            if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
            {
                // Validate it's from an allowed origin first
                var urlOrigin = $"{uri.Scheme}://{uri.Authority}";
                if (_allowedOrigins.Any(origin => origin.Equals(urlOrigin, StringComparison.OrdinalIgnoreCase)))
                {
                    // Return path + query + fragment (relative URL)
                    var relativePath = uri.PathAndQuery;
                    if (!string.IsNullOrEmpty(uri.Fragment))
                    {
                        relativePath += uri.Fragment;
                    }

                    _logger.LogDebug("Normalized absolute URL to relative: {Original} -> {Normalized}", returnUrl, relativePath);
                    return relativePath;
                }

                _logger.LogWarning("Return URL from disallowed origin, using default. Origin: {Origin}", urlOrigin);
                return "/";
            }

            // Invalid format, use default
            _logger.LogWarning("Invalid return URL format, using default. URL: {ReturnUrl}", returnUrl);
            return "/";
        }

        /// <summary>
        /// Builds an absolute URL for redirecting back to the frontend application.
        /// </summary>
        private string? BuildAbsoluteReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return _allowedOrigins.FirstOrDefault();
            }

            // If it's already an absolute URL and valid, return it
            if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absoluteUri))
            {
                var urlOrigin = $"{absoluteUri.Scheme}://{absoluteUri.Authority}";
                if (_allowedOrigins.Any(origin => origin.Equals(urlOrigin, StringComparison.OrdinalIgnoreCase)))
                {
                    return returnUrl;
                }

                _logger.LogWarning("Absolute URL from disallowed origin: {Origin}", urlOrigin);
                return null;
            }

            // It's a relative URL, prepend the first allowed origin (frontend origin)
            var frontendOrigin = _allowedOrigins.FirstOrDefault();
            if (frontendOrigin != null)
            {
                // Ensure returnUrl starts with /
                var path = returnUrl.StartsWith('/') ? returnUrl : $"/{returnUrl}";
                return $"{frontendOrigin}{path}";
            }

            return null;
        }
    }
}
