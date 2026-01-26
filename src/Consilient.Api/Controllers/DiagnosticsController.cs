using Consilient.Api.Configuration;
using Consilient.Infrastructure.Logging.Configuration;
using Consilient.Users.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Consilient.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController(
        LoggingOptions? loggingOptions,
        IOptions<AuthenticationOptions> authOptions,
        IOptions<UserServiceOptions> userServiceOptions,
        ILogger<DiagnosticsController> logger,
        IWebHostEnvironment environment) : ControllerBase
    {
        private readonly LoggingOptions? _loggingOptions = loggingOptions;
        private readonly AuthenticationOptions _authOptions = authOptions.Value;
        private readonly UserServiceOptions _userServiceOptions = userServiceOptions.Value;
        private readonly ILogger<DiagnosticsController> _logger = logger;
        private readonly IWebHostEnvironment _environment = environment;

        [HttpGet("loki-config")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or add auth
        public IActionResult GetLokiConfig()
        {
            return Ok(new
            {
                LokiUrlFromConfig = _loggingOptions?.GrafanaLoki?.Url,
                LokiPushEndpoint = _loggingOptions?.GrafanaLoki?.PushEndpoint,
                ConstructedReadyUrl = !string.IsNullOrEmpty(_loggingOptions?.GrafanaLoki?.Url)
                    ? $"{_loggingOptions.GrafanaLoki.Url.TrimEnd('/')}/ready"
                    : "N/A"
            });
        }

        [HttpGet("test-loki")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or add auth
        public async Task<IActionResult> TestLokiConnection()
        {
            var lokiUrl = _loggingOptions?.GrafanaLoki?.Url;
            if (string.IsNullOrEmpty(lokiUrl))
            {
                return BadRequest("Loki URL not configured");
            }

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var readyUrl = $"{lokiUrl.TrimEnd('/')}/ready";

            try
            {
                var response = await client.GetAsync(readyUrl);
                var body = await response.Content.ReadAsStringAsync();

                return Ok(new
                {
                    Url = readyUrl,
                    StatusCode = (int)response.StatusCode,
                    Body = body
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Url = readyUrl,
                    Error = ex.Message,
                    ExceptionType = ex.GetType().Name
                });
            }
        }

        [HttpGet("app-config")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or restrict with auth
        public IActionResult GetAppConfiguration()
        {
            // Return a summary of loaded configuration (non-sensitive values only)
            return Ok(new
            {
                Environment = _environment.EnvironmentName,
                Authentication = new
                {
                    Enabled = _authOptions.Enabled,
                    CookieExpiryMinutes = _authOptions.CookieExpiryMinutes,
                    PasswordPolicy = _authOptions.PasswordPolicy != null ? new
                    {
                        _authOptions.PasswordPolicy.RequiredLength,
                        _authOptions.PasswordPolicy.RequireDigit,
                        _authOptions.PasswordPolicy.RequireUppercase,
                        _authOptions.PasswordPolicy.RequireLowercase,
                        _authOptions.PasswordPolicy.RequireNonAlphanumeric
                    } : null
                },
                UserService = new
                {
                    AutoProvisionUser = _userServiceOptions.AutoProvisionUser,
                    AllowedEmailDomains = _userServiceOptions.AllowedEmailDomains,
                    JwtConfigured = _userServiceOptions.Jwt != null,
                    OAuthConfigured = _userServiceOptions.OAuth != null
                }
            });
        }

        [HttpGet("app-config/verify")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or restrict with auth
        public IActionResult VerifyOAuthConfiguration()
        {
            var oauth = _userServiceOptions.OAuth;

            var result = new
            {
                OAuthEnabled = oauth?.Enabled ?? false,
                OAuthProviderName = oauth?.ProviderName ?? "NOT SET",
                OAuthClientId = !string.IsNullOrEmpty(oauth?.ClientId) ? "SET" : "NOT SET",
                OAuthClientSecret = !string.IsNullOrEmpty(oauth?.ClientSecret) ? "SET" : "NOT SET",
                OAuthTenantId = !string.IsNullOrEmpty(oauth?.TenantId) ? "SET" : "NOT SET",
                OAuthAuthority = oauth?.Authority ?? "NOT SET",
                OAuthScopes = oauth?.Scopes != null ? string.Join(", ", oauth.Scopes) : "NOT SET",
                Environment = _environment.EnvironmentName
            };

            return Ok(result);
        }

        /// <summary>
        /// Writes a test log entry to Loki via Serilog and then queries Loki to verify it was received.
        /// Uses Fatal level to bypass any log level filtering.
        /// </summary>
        [HttpGet("test-loki-logging")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or add auth
        public async Task<IActionResult> TestLokiLogging()
        {
            var lokiUrl = _loggingOptions?.GrafanaLoki?.Url;
            if (string.IsNullOrEmpty(lokiUrl))
            {
                return BadRequest(new { Error = "Loki URL not configured" });
            }

            // Generate a unique marker so we can find this specific log entry
            var testMarker = $"LOKI_TEST_{Guid.NewGuid():N}";
            var timestamp = DateTimeOffset.UtcNow;

            // Write the test log using Critical level to ensure it bypasses any filtering
            _logger.LogCritical("Loki connectivity test: {TestMarker} at {Timestamp}",
                testMarker,
                timestamp);

            // Give Serilog's batch sink time to flush (it batches logs before sending)
            await Task.Delay(3000);

            // Now query Loki to see if the log entry appears
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            var results = new List<object>();
            var foundInLoki = false;
            string? lokiError = null;

            try
            {
                // Query Loki for recent logs with our test marker
                // Use a time range from 5 minutes ago to now
                var startNs = (timestamp.AddMinutes(-5).ToUnixTimeMilliseconds() * 1_000_000).ToString();
                var endNs = (DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeMilliseconds() * 1_000_000).ToString();

                // Query for any logs containing our marker
                var query = Uri.EscapeDataString($"{{app=~\".+\"}} |= \"{testMarker}\"");
                var lokiQueryUrl = $"{lokiUrl.TrimEnd('/')}/loki/api/v1/query_range?query={query}&start={startNs}&end={endNs}&limit=10";

                var response = await client.GetAsync(lokiQueryUrl);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse the response to check if we found matches
                    using var doc = JsonDocument.Parse(responseBody);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var data) &&
                        data.TryGetProperty("result", out var result) &&
                        result.GetArrayLength() > 0)
                    {
                        foundInLoki = true;
                        foreach (var stream in result.EnumerateArray())
                        {
                            if (stream.TryGetProperty("values", out var values))
                            {
                                foreach (var value in values.EnumerateArray())
                                {
                                    if (value.GetArrayLength() >= 2)
                                    {
                                        results.Add(new
                                        {
                                            Timestamp = value[0].GetString(),
                                            Message = value[1].GetString()
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    lokiError = $"Loki query failed with status {response.StatusCode}: {responseBody}";
                }
            }
            catch (Exception ex)
            {
                lokiError = $"Error querying Loki: {ex.Message}";
            }

            // Also check what labels exist in Loki
            string? labelsResponse = null;
            try
            {
                var labelsUrl = $"{lokiUrl.TrimEnd('/')}/loki/api/v1/labels";
                var response = await client.GetAsync(labelsUrl);
                labelsResponse = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                labelsResponse = $"Error: {ex.Message}";
            }

            return Ok(new
            {
                TestMarker = testMarker,
                LogWrittenAt = timestamp,
                LokiUrl = lokiUrl,
                FoundInLoki = foundInLoki,
                MatchingEntries = results,
                LokiLabels = labelsResponse,
                LokiError = lokiError,
                Note = foundInLoki
                    ? "SUCCESS: Log entry was written to Loki and retrieved successfully!"
                    : "Log entry was written via Serilog but not yet found in Loki. This could mean: (1) The Serilog Loki sink hasn't flushed yet, (2) The Loki URL in config doesn't match where logs are being sent, or (3) There's a network/auth issue between the API and Loki."
            });
        }

        /// <summary>
        /// Logs a message at the specified log level. Useful for verifying Loki receives logs at all levels.
        /// </summary>
        /// <param name="level">Log level: Trace, Debug, Information, Warning, Error, or Critical</param>
        /// <param name="message">The message to log. Defaults to a standard test message if not provided.</param>
        [HttpGet("log")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or add auth
        public IActionResult LogMessage([FromQuery] string level, [FromQuery] string? message = null)
        {
            const string defaultMessage = "Test log message from diagnostics endpoint";
            var logMessage = string.IsNullOrWhiteSpace(message) ? defaultMessage : message;

            if (!Enum.TryParse<LogLevel>(level, ignoreCase: true, out var logLevel))
            {
                return BadRequest($"Invalid log level '{level}'. Valid values: Trace, Debug, Information, Warning, Error, Critical");
            }

            _logger.Log(logLevel, "Diagnostic log: {Message}", logMessage);

            return Ok($"[{logLevel}] {logMessage}");
        }
    }
}
