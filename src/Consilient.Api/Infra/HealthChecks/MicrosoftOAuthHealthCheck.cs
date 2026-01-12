using System.Text.Json;
using Consilient.Users.Contracts.OAuth;
using Consilient.Users.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Consilient.Api.Infra.HealthChecks
{
    /// <summary>
    /// Health check for Microsoft OAuth authentication that verifies:
    /// 1. Configuration - Required OAuth settings are present (ClientId, TenantId, Authority, ClientSecret)
    /// 2. Connectivity - Microsoft's OpenID Connect discovery endpoint and JWKS are reachable
    /// 3. Credentials - Client credentials token acquisition succeeds
    ///
    /// Returns structured data for troubleshooting with separate status for each check.
    /// </summary>
    internal class MicrosoftOAuthHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly OAuthProviderServiceConfiguration? _configuration;

        // Cache the last successful check to avoid hammering Microsoft endpoints
        private static DateTime _lastSuccessfulCheck = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private const string GraphScope = "https://graph.microsoft.com/.default";

        public MicrosoftOAuthHealthCheck(
            HttpClient httpClient,
            IOptions<UserServiceConfiguration> userConfig)
        {
            _httpClient = httpClient;
            _configuration = userConfig?.Value?.OAuth;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>();

            // Step 1: Check if OAuth is configured and enabled
            var configResult = CheckConfiguration();
            data["enabled"] = configResult.IsEnabled;
            data["configStatus"] = configResult.Status;

            if (!configResult.IsEnabled)
            {
                data["configError"] = configResult.Error ?? "OAuth is not enabled";
                return HealthCheckResult.Degraded(
                    "Microsoft OAuth is not enabled",
                    data: data);
            }

            if (!configResult.IsSuccess)
            {
                data["configError"] = configResult.Error ?? "Unknown configuration error";
                return HealthCheckResult.Unhealthy(
                    $"Microsoft OAuth configuration invalid: {configResult.Error}",
                    data: data);
            }

            // Add masked configuration values for diagnostics
            data["authority"] = _configuration!.Authority!;
            data["tenantId"] = MaskValue(_configuration.TenantId) ?? "not configured";
            data["clientId"] = MaskValue(_configuration.ClientId) ?? "not configured";

            // Check cache - if recently successful, return cached result
            if (DateTime.UtcNow - _lastSuccessfulCheck < CacheDuration)
            {
                data["discoveryStatus"] = "cached";
                data["jwksStatus"] = "cached";
                data["tokenStatus"] = "cached";
                data["lastCheck"] = _lastSuccessfulCheck.ToString("o");
                return HealthCheckResult.Healthy(
                    $"Microsoft OAuth healthy (cached, last verified at {_lastSuccessfulCheck:HH:mm:ss} UTC)",
                    data: data);
            }

            // Step 2: Check discovery endpoint connectivity
            var discoveryResult = await CheckDiscoveryEndpointAsync(cancellationToken);
            data["discoveryStatus"] = discoveryResult.Status;
            if (discoveryResult.Error != null)
            {
                data["discoveryError"] = discoveryResult.Error;
            }

            if (!discoveryResult.IsSuccess)
            {
                data["jwksStatus"] = "skipped";
                data["tokenStatus"] = "skipped";
                return HealthCheckResult.Unhealthy(
                    $"Microsoft OAuth discovery endpoint unreachable: {discoveryResult.Error}",
                    data: data);
            }

            // Step 3: Check JWKS endpoint
            var jwksResult = await CheckJwksEndpointAsync(discoveryResult.JwksUri!, cancellationToken);
            data["jwksStatus"] = jwksResult.Status;
            if (jwksResult.KeyCount > 0)
            {
                data["signingKeysCount"] = jwksResult.KeyCount;
            }
            if (jwksResult.Error != null)
            {
                data["jwksError"] = jwksResult.Error;
            }

            if (!jwksResult.IsSuccess)
            {
                data["tokenStatus"] = "skipped";
                return HealthCheckResult.Unhealthy(
                    $"Microsoft OAuth JWKS endpoint failed: {jwksResult.Error}",
                    data: data);
            }

            // Step 4: Attempt client credentials token acquisition
            var tokenResult = await CheckTokenAcquisitionAsync(cancellationToken);
            data["tokenStatus"] = tokenResult.Status;
            if (tokenResult.Error != null)
            {
                data["tokenError"] = tokenResult.Error;
            }

            if (!tokenResult.IsSuccess)
            {
                // Endpoints are reachable but token acquisition failed - Degraded
                return HealthCheckResult.Degraded(
                    $"Microsoft OAuth endpoints reachable but token acquisition failed: {tokenResult.Error}",
                    data: data);
            }

            // All checks passed - update cache
            _lastSuccessfulCheck = DateTime.UtcNow;
            data["lastCheck"] = _lastSuccessfulCheck.ToString("o");

            return HealthCheckResult.Healthy(
                $"Microsoft OAuth healthy (config: ok, discovery: ok, jwks: {jwksResult.KeyCount} keys, token: ok)",
                data: data);
        }

        private ConfigCheckResult CheckConfiguration()
        {
            if (_configuration == null)
            {
                return new ConfigCheckResult
                {
                    IsSuccess = false,
                    IsEnabled = false,
                    Status = "missing",
                    Error = "OAuth configuration section is missing"
                };
            }

            if (!_configuration.Enabled)
            {
                return new ConfigCheckResult
                {
                    IsSuccess = false,
                    IsEnabled = false,
                    Status = "disabled",
                    Error = "OAuth is disabled in configuration"
                };
            }

            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(_configuration.ClientId))
                missingFields.Add("ClientId");
            if (string.IsNullOrWhiteSpace(_configuration.ClientSecret))
                missingFields.Add("ClientSecret");
            if (string.IsNullOrWhiteSpace(_configuration.TenantId))
                missingFields.Add("TenantId");
            if (string.IsNullOrWhiteSpace(_configuration.Authority))
                missingFields.Add("Authority");

            if (missingFields.Count > 0)
            {
                return new ConfigCheckResult
                {
                    IsSuccess = false,
                    IsEnabled = true,
                    Status = "incomplete",
                    Error = $"Missing required fields: {string.Join(", ", missingFields)}"
                };
            }

            return new ConfigCheckResult
            {
                IsSuccess = true,
                IsEnabled = true,
                Status = "ok"
            };
        }

        private async Task<DiscoveryCheckResult> CheckDiscoveryEndpointAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Authority may already include TenantId (e.g., https://login.microsoftonline.com/{tenantId})
                // or may be just the base URL. Handle both cases.
                var authority = _configuration!.Authority!.TrimEnd('/');
                var tenantId = _configuration.TenantId!;

                // Check if authority already ends with the tenant ID
                var discoveryUrl = authority.EndsWith(tenantId, StringComparison.OrdinalIgnoreCase)
                    ? $"{authority}/.well-known/openid-configuration"
                    : $"{authority}/{tenantId}/.well-known/openid-configuration";

                var response = await _httpClient.GetAsync(discoveryUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    var preview = errorBody.Length > 200 ? errorBody[..200] : errorBody;
                    return new DiscoveryCheckResult
                    {
                        IsSuccess = false,
                        Status = "failed",
                        Error = $"HTTP {(int)response.StatusCode}: {preview}"
                    };
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content);

                if (!doc.RootElement.TryGetProperty("jwks_uri", out var jwksUriElement))
                {
                    return new DiscoveryCheckResult
                    {
                        IsSuccess = false,
                        Status = "invalid",
                        Error = "Discovery document missing jwks_uri"
                    };
                }

                return new DiscoveryCheckResult
                {
                    IsSuccess = true,
                    Status = "ok",
                    JwksUri = jwksUriElement.GetString()
                };
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException ex)
            {
                return new DiscoveryCheckResult
                {
                    IsSuccess = false,
                    Status = "timeout",
                    Error = $"Timeout: {ex.Message}"
                };
            }
            catch (HttpRequestException ex)
            {
                return new DiscoveryCheckResult
                {
                    IsSuccess = false,
                    Status = "unreachable",
                    Error = $"Cannot reach discovery endpoint: {ex.Message}"
                };
            }
            catch (JsonException ex)
            {
                return new DiscoveryCheckResult
                {
                    IsSuccess = false,
                    Status = "invalid_json",
                    Error = $"Invalid JSON in discovery document: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new DiscoveryCheckResult
                {
                    IsSuccess = false,
                    Status = "error",
                    Error = ex.Message
                };
            }
        }

        private async Task<JwksCheckResult> CheckJwksEndpointAsync(string jwksUri, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(jwksUri, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    var preview = errorBody.Length > 200 ? errorBody[..200] : errorBody;
                    return new JwksCheckResult
                    {
                        IsSuccess = false,
                        Status = "failed",
                        Error = $"HTTP {(int)response.StatusCode}: {preview}"
                    };
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content);

                if (!doc.RootElement.TryGetProperty("keys", out var keysElement) ||
                    keysElement.ValueKind != JsonValueKind.Array)
                {
                    return new JwksCheckResult
                    {
                        IsSuccess = false,
                        Status = "invalid",
                        Error = "JWKS document missing 'keys' array"
                    };
                }

                var keyCount = keysElement.GetArrayLength();
                if (keyCount == 0)
                {
                    return new JwksCheckResult
                    {
                        IsSuccess = false,
                        Status = "empty",
                        Error = "JWKS contains no signing keys"
                    };
                }

                return new JwksCheckResult
                {
                    IsSuccess = true,
                    Status = "ok",
                    KeyCount = keyCount
                };
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException ex)
            {
                return new JwksCheckResult
                {
                    IsSuccess = false,
                    Status = "timeout",
                    Error = $"Timeout: {ex.Message}"
                };
            }
            catch (HttpRequestException ex)
            {
                return new JwksCheckResult
                {
                    IsSuccess = false,
                    Status = "unreachable",
                    Error = $"Cannot reach JWKS endpoint: {ex.Message}"
                };
            }
            catch (JsonException ex)
            {
                return new JwksCheckResult
                {
                    IsSuccess = false,
                    Status = "invalid_json",
                    Error = $"Invalid JSON in JWKS: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new JwksCheckResult
                {
                    IsSuccess = false,
                    Status = "error",
                    Error = ex.Message
                };
            }
        }

        private async Task<TokenCheckResult> CheckTokenAcquisitionAsync(CancellationToken cancellationToken)
        {
            try
            {
                var authority = _configuration!.Authority!.TrimEnd('/');
                var authorityUri = $"{authority}/{_configuration.TenantId}";

                var app = ConfidentialClientApplicationBuilder
                    .Create(_configuration.ClientId)
                    .WithClientSecret(_configuration.ClientSecret)
                    .WithAuthority(authorityUri)
                    .Build();

                var result = await app
                    .AcquireTokenForClient(new[] { GraphScope })
                    .ExecuteAsync(cancellationToken);

                if (string.IsNullOrEmpty(result.AccessToken))
                {
                    return new TokenCheckResult
                    {
                        IsSuccess = false,
                        Status = "empty_token",
                        Error = "Token acquisition returned empty access token"
                    };
                }

                return new TokenCheckResult
                {
                    IsSuccess = true,
                    Status = "ok"
                };
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_client")
            {
                return new TokenCheckResult
                {
                    IsSuccess = false,
                    Status = "invalid_credentials",
                    Error = "Invalid ClientId or ClientSecret"
                };
            }
            catch (MsalServiceException ex) when (ex.ErrorCode == "unauthorized_client")
            {
                return new TokenCheckResult
                {
                    IsSuccess = false,
                    Status = "unauthorized",
                    Error = "Client is not authorized for client_credentials grant"
                };
            }
            catch (MsalServiceException ex)
            {
                return new TokenCheckResult
                {
                    IsSuccess = false,
                    Status = "service_error",
                    Error = $"MSAL error ({ex.ErrorCode}): {ex.Message}"
                };
            }
            catch (MsalException ex)
            {
                return new TokenCheckResult
                {
                    IsSuccess = false,
                    Status = "msal_error",
                    Error = $"MSAL error ({ex.ErrorCode}): {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new TokenCheckResult
                {
                    IsSuccess = false,
                    Status = "error",
                    Error = ex.Message
                };
            }
        }

        private static string? MaskValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (value.Length <= 8)
                return "***";

            // Show first 4 and last 4 characters
            return $"{value[..4]}***{value[^4..]}";
        }

        private record ConfigCheckResult
        {
            public bool IsSuccess { get; init; }
            public bool IsEnabled { get; init; }
            public required string Status { get; init; }
            public string? Error { get; init; }
        }

        private record DiscoveryCheckResult
        {
            public bool IsSuccess { get; init; }
            public required string Status { get; init; }
            public string? Error { get; init; }
            public string? JwksUri { get; init; }
        }

        private record JwksCheckResult
        {
            public bool IsSuccess { get; init; }
            public required string Status { get; init; }
            public string? Error { get; init; }
            public int KeyCount { get; init; }
        }

        private record TokenCheckResult
        {
            public bool IsSuccess { get; init; }
            public required string Status { get; init; }
            public string? Error { get; init; }
        }
    }
}
