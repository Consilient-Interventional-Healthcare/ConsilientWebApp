using Consilient.Users.Contracts;
using Consilient.Users.Contracts.OAuth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Consilient.Users.Services.OAuth
{
    /// <summary>
    /// Microsoft-specific OAuth provider service.
    /// </summary>
    public class MicrosoftOAuthProviderService : IOAuthProviderService
    {
        private readonly OAuthProviderOptions _oauthOptions;
        private readonly ILogger<MicrosoftOAuthProviderService> _logger;

        public MicrosoftOAuthProviderService(
            IOptions<UserServiceOptions> userServiceOptions,
            ILogger<MicrosoftOAuthProviderService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _oauthOptions = userServiceOptions?.Value?.OAuth
                ?? throw new InvalidOperationException(
                    "OAuth configuration is missing. Please ensure the OAuth section is properly configured in application settings.");

            ValidateConfiguration();

            if (!_oauthOptions.Enabled)
            {
                _logger.LogWarning("Microsoft OAuth provider is not enabled in configuration");
            }
        }

        public string GetProviderName() => _oauthOptions.ProviderName;

        public async Task<string> BuildAuthorizationUrlAsync(
            string state,
            string codeChallenge,
            string redirectUri,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(state);
            ArgumentException.ThrowIfNullOrWhiteSpace(codeChallenge);
            ArgumentException.ThrowIfNullOrWhiteSpace(redirectUri);
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogDebug("Building authorization URL for Microsoft OAuth");
            _logger.LogDebug("Redirect URI: {RedirectUri}", redirectUri);
            _logger.LogDebug("Authority: {Authority}/{TenantId}", _oauthOptions.Authority, _oauthOptions.TenantId);
            _logger.LogDebug("Client ID: {ClientId}", _oauthOptions.ClientId);

            var scopes = _oauthOptions.Scopes ?? [];
            _logger.LogDebug("Scopes requested: {Scopes}", string.Join(", ", scopes));
            if (!scopes.Any())
            {
                _logger.LogWarning("No OAuth scopes configured");
            }

            var app = CreateConfidentialClientApplication(redirectUri);

            // Build authorization URL with PKCE support
            var authUrl = await app.GetAuthorizationRequestUrl(scopes)
                .WithRedirectUri(redirectUri)
                .WithExtraQueryParameters($"state={Uri.EscapeDataString(state)}&code_challenge={Uri.EscapeDataString(codeChallenge)}&code_challenge_method=S256")
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug("Authorization URL built successfully: {AuthUrl}", authUrl);

            return authUrl.ToString();
        }

        public async Task<AuthorizationCodeValidationResult> ValidateAuthorizationCodeAsync(
            string code,
            string codeVerifier,
            string redirectUri,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(codeVerifier);
            ArgumentException.ThrowIfNullOrWhiteSpace(redirectUri);
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogDebug("Validating authorization code with Microsoft Entra");
            _logger.LogDebug("Redirect URI for token exchange: {RedirectUri}", redirectUri);
            _logger.LogDebug("Code length: {CodeLength}, CodeVerifier length: {VerifierLength}", code.Length, codeVerifier.Length);

            var app = CreateConfidentialClientApplication(redirectUri);
            var scopes = _oauthOptions.Scopes ?? [];
            _logger.LogDebug("Scopes for token exchange: {Scopes}", string.Join(", ", scopes));

            try
            {
                _logger.LogDebug("Initiating token exchange with Microsoft Entra (AcquireTokenByAuthorizationCode)");

                // Use PKCE code verifier for token exchange
                var result = await app.AcquireTokenByAuthorizationCode(scopes, code)
                    .WithPkceCodeVerifier(codeVerifier)
                    .ExecuteAsync(cancellationToken)
                    .ConfigureAwait(false);

                _logger.LogDebug("Token exchange response received from Microsoft Entra");

                cancellationToken.ThrowIfCancellationRequested();

                if (result.Account == null)
                {
                    _logger.LogDebug("Token exchange succeeded but Account is null in response");
                    throw new InvalidOperationException("Account information not present in token response.");
                }

                var account = result.Account;
                var userEmail = account.Username ?? string.Empty;
                var providerKey = account.HomeAccountId?.ObjectId ?? string.Empty;

                _logger.LogDebug("Token exchange successful. Account: {Username}, ObjectId: {ObjectId}, TenantId: {TenantId}",
                    account.Username, account.HomeAccountId?.ObjectId, account.HomeAccountId?.TenantId);

                return new AuthorizationCodeValidationResult
                {
                    Succeeded = true,
                    ProviderName = _oauthOptions.ProviderName,
                    ProviderKey = providerKey,
                    UserName = userEmail,
                    UserEmail = userEmail
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Token validation cancelled by client");
                return new AuthorizationCodeValidationResult
                {
                    Succeeded = false,
                    Error = "Request was cancelled"
                };
            }
            catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_grant")
            {
                _logger.LogWarning(ex, "Invalid or expired authorization code. ErrorCode: {ErrorCode}", ex.ErrorCode);
                return new AuthorizationCodeValidationResult
                {
                    Succeeded = false,
                    Error = "Invalid or expired authorization code"
                };
            }
            catch (MsalException ex)
            {
                _logger.LogError(ex, "Microsoft authentication failed. ErrorCode: {ErrorCode}", ex.ErrorCode);
                return new AuthorizationCodeValidationResult
                {
                    Succeeded = false,
                    Error = "Microsoft authentication failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Microsoft authentication: {ErrorMessage}", ex.Message);
                return new AuthorizationCodeValidationResult
                {
                    Succeeded = false,
                    Error = "Unexpected error during Microsoft authentication"
                };
            }
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_oauthOptions.ClientId))
            {
                throw new InvalidOperationException(
                    "OAuth ClientId is required but not configured in application settings.");
            }

            if (string.IsNullOrWhiteSpace(_oauthOptions.ClientSecret))
            {
                throw new InvalidOperationException(
                    "OAuth ClientSecret is required but not configured in application settings.");
            }

            if (string.IsNullOrWhiteSpace(_oauthOptions.Authority))
            {
                throw new InvalidOperationException(
                    "OAuth Authority is required but not configured in application settings.");
            }

            if (string.IsNullOrWhiteSpace(_oauthOptions.TenantId))
            {
                throw new InvalidOperationException(
                    "OAuth TenantId is required but not configured in application settings.");
            }

            if (string.IsNullOrWhiteSpace(_oauthOptions.ProviderName))
            {
                throw new InvalidOperationException(
                    "OAuth ProviderName is required but not configured in application settings.");
            }
        }

        private IConfidentialClientApplication CreateConfidentialClientApplication(string redirectUri)
        {
            var authority = _oauthOptions.Authority!.TrimEnd('/');
            var authorityUri = $"{authority}/{_oauthOptions.TenantId}";

            _logger.LogDebug("Creating MSAL ConfidentialClientApplication");
            _logger.LogDebug("Authority URI: {AuthorityUri}", authorityUri);
            _logger.LogDebug("Redirect URI: {RedirectUri}", redirectUri);

            return ConfidentialClientApplicationBuilder
                .Create(_oauthOptions.ClientId)
                .WithClientSecret(_oauthOptions.ClientSecret)
                .WithAuthority(authorityUri)
                .WithRedirectUri(redirectUri)
                .Build();
        }
    }
}