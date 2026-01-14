using System.Text.Json;
using Consilient.Infrastructure.Logging.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController(
        LoggingConfiguration? loggingConfiguration,
        IConfiguration configuration,
        ILogger<DiagnosticsController> logger) : ControllerBase
    {
        private readonly LoggingConfiguration? _loggingConfiguration = loggingConfiguration;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<DiagnosticsController> _logger = logger;

        [HttpGet("loki-config")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or add auth
        public IActionResult GetLokiConfig()
        {
            return Ok(new
            {
                LokiUrlFromConfig = _loggingConfiguration?.GrafanaLoki?.Url,
                LokiPushEndpoint = _loggingConfiguration?.GrafanaLoki?.PushEndpoint,
                ConstructedReadyUrl = !string.IsNullOrEmpty(_loggingConfiguration?.GrafanaLoki?.Url)
                    ? $"{_loggingConfiguration.GrafanaLoki.Url.TrimEnd('/')}/ready"
                    : "N/A"
            });
        }

        [HttpGet("test-loki")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or add auth
        public async Task<IActionResult> TestLokiConnection()
        {
            var lokiUrl = _loggingConfiguration?.GrafanaLoki?.Url;
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
        public IActionResult GetAppConfiguration([FromQuery] string? prefix = null)
        {
            var configKeys = new Dictionary<string, object?>();

            // Get all configuration keys (or filter by prefix)
            var allConfig = _configuration.AsEnumerable()
                .Where(kvp => !string.IsNullOrEmpty(kvp.Key))
                .Where(kvp => string.IsNullOrEmpty(prefix) || kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(kvp => kvp.Key);

            foreach (var kvp in allConfig)
            {
                // Mask sensitive values
                var isSensitive = kvp.Key.Contains("Secret", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase);

                configKeys[kvp.Key] = isSensitive ? "***MASKED***" : kvp.Value;
            }

            return Ok(new
            {
                TotalKeys = configKeys.Count,
                FilterPrefix = prefix,
                Configuration = configKeys
            });
        }

        [HttpGet("app-config/verify")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or restrict with auth
        public IActionResult VerifyOAuthConfiguration()
        {
            // Verify specific OAuth keys are loaded with expected values
            var oauthSection = "ApplicationSettings:Authentication:UserService:OAuth";

            var result = new
            {
                OAuthEnabled = _configuration[$"{oauthSection}:Enabled"],
                OAuthProviderName = _configuration[$"{oauthSection}:ProviderName"],
                OAuthClientId = !string.IsNullOrEmpty(_configuration[$"{oauthSection}:ClientId"]) ? "SET" : "NOT SET",
                OAuthClientSecret = !string.IsNullOrEmpty(_configuration[$"{oauthSection}:ClientSecret"]) ? "SET" : "NOT SET",
                OAuthTenantId = !string.IsNullOrEmpty(_configuration[$"{oauthSection}:TenantId"]) ? "SET" : "NOT SET",
                OAuthAuthority = _configuration[$"{oauthSection}:Authority"],
                OAuthScopes = _configuration[$"{oauthSection}:Scopes"],

                // Show config source indicator
                AppConfigurationEndpoint = _configuration["AppConfiguration:Endpoint"] ?? "NOT SET",
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"]
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
            var lokiUrl = _loggingConfiguration?.GrafanaLoki?.Url;
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
