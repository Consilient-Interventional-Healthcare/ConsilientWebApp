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
        private readonly OAuthProviderServiceConfiguration _configuration;
        private readonly ILogger<MicrosoftOAuthProviderService> _logger;

        public MicrosoftOAuthProviderService(
            IOptions<UserServiceConfiguration> userConfig,
            ILogger<MicrosoftOAuthProviderService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = userConfig?.Value?.OAuth 
                ?? throw new InvalidOperationException(
                    "OAuth configuration is missing. Please ensure the OAuth section is properly configured in application settings.");

            ValidateConfiguration();

            if (!_configuration.Enabled)
            {
                _logger.LogWarning("Microsoft OAuth provider is not enabled in configuration");
            }
        }

        public string GetProviderName() => _configuration.ProviderName;

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

            var scopes = _configuration.Scopes ?? [];
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

            var app = CreateConfidentialClientApplication(redirectUri);
            var scopes = _configuration.Scopes ?? [];

            try
            {
                // Use PKCE code verifier for token exchange
                var result = await app.AcquireTokenByAuthorizationCode(scopes, code)
                    .WithPkceCodeVerifier(codeVerifier)
                    .ExecuteAsync(cancellationToken)
                    .ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                if (result.Account == null)
                {
                    throw new InvalidOperationException("Account information not present in token response.");
                }

                var account = result.Account;
                var userEmail = account.Username ?? string.Empty;
                var providerKey = account.HomeAccountId?.ObjectId ?? string.Empty;

                return new AuthorizationCodeValidationResult
                {
                    Succeeded = true,
                    ProviderName = _configuration.ProviderName,
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
            if (string.IsNullOrWhiteSpace(_configuration.ClientId))
            {
                throw new InvalidOperationException(
                    "OAuth ClientId is required but not configured in application settings.");
            }

            if (string.IsNullOrWhiteSpace(_configuration.ClientSecret))
            {
                throw new InvalidOperationException(
                    "OAuth ClientSecret is required but not configured in application settings.");
            }

            if (string.IsNullOrWhiteSpace(_configuration.Authority))
            {
                throw new InvalidOperationException(
                    "OAuth Authority is required but not configured in application settings.");
            }

            if (string.IsNullOrWhiteSpace(_configuration.TenantId))
            {
                throw new InvalidOperationException(
                    "OAuth TenantId is required but not configured in application settings.");
            }

            if (string.IsNullOrWhiteSpace(_configuration.ProviderName))
            {
                throw new InvalidOperationException(
                    "OAuth ProviderName is required but not configured in application settings.");
            }
        }

        private IConfidentialClientApplication CreateConfidentialClientApplication(string redirectUri)
        {
            var authority = _configuration.Authority!.TrimEnd('/');
            var authorityUri = $"{authority}/{_configuration.TenantId}";
            
            return ConfidentialClientApplicationBuilder
                .Create(_configuration.ClientId)
                .WithClientSecret(_configuration.ClientSecret)
                .WithAuthority(authorityUri)
                .WithRedirectUri(redirectUri)
                .Build();
        }
    }
}