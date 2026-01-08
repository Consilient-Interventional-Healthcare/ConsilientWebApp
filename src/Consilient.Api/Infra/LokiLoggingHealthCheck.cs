using System.Text.Json;
using Consilient.Infrastructure.Logging.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;

namespace Consilient.Api.Infra
{
    /// <summary>
    /// Health check that verifies the complete logging pipeline to Loki by:
    /// 1. Writing a test log entry via Serilog
    /// 2. Querying Loki to verify the entry was received
    ///
    /// This is more comprehensive than LokiHealthCheck which only checks connectivity.
    /// Use this health check sparingly (e.g., tagged for detailed diagnostics) as it
    /// introduces latency due to the write-then-read verification.
    /// </summary>
    internal class LokiLoggingHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly LoggingConfiguration? _loggingConfiguration;
        private readonly Serilog.ILogger _logger;

        // Cache the last successful verification to avoid hammering Loki on every health check
        private static DateTime _lastSuccessfulVerification = DateTime.MinValue;
        private static readonly TimeSpan VerificationCacheDuration = TimeSpan.FromMinutes(5);

        public LokiLoggingHealthCheck(HttpClient httpClient, LoggingConfiguration? loggingConfiguration)
        {
            _httpClient = httpClient;
            _loggingConfiguration = loggingConfiguration;
            _logger = Log.Logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var lokiUrl = _loggingConfiguration?.GrafanaLoki?.Url;

            if (string.IsNullOrEmpty(lokiUrl))
            {
                return HealthCheckResult.Degraded("Loki URL is not configured");
            }

            // If we recently verified successfully, return cached result to reduce load
            if (DateTime.UtcNow - _lastSuccessfulVerification < VerificationCacheDuration)
            {
                return HealthCheckResult.Healthy(
                    $"Loki logging verified (cached, last check: {_lastSuccessfulVerification:HH:mm:ss} UTC)");
            }

            try
            {
                // Step 1: Write a unique test log entry using Fatal level to bypass filtering
                var testMarker = $"HEALTHCHECK_{Guid.NewGuid():N}";
                var timestamp = DateTimeOffset.UtcNow;

                _logger.Write(LogEventLevel.Fatal,
                    "Loki health check marker: {TestMarker}",
                    testMarker);

                // Step 2: Wait for Serilog's batch sink to flush
                // The Loki sink batches logs, so we need to wait a bit
                await Task.Delay(2000, cancellationToken);

                // Step 3: Query Loki for the test entry
                var startNs = (timestamp.AddMinutes(-1).ToUnixTimeMilliseconds() * 1_000_000).ToString();
                var endNs = (DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeMilliseconds() * 1_000_000).ToString();

                var query = Uri.EscapeDataString($"{{app=~\".+\"}} |= \"{testMarker}\"");
                var lokiQueryUrl = $"{lokiUrl.TrimEnd('/')}/loki/api/v1/query_range?query={query}&start={startNs}&end={endNs}&limit=1";

                var response = await _httpClient.GetAsync(lokiQueryUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    return HealthCheckResult.Unhealthy(
                        $"Failed to query Loki: HTTP {(int)response.StatusCode}. Response: {errorBody.Substring(0, Math.Min(200, errorBody.Length))}");
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("result", out var result) &&
                    result.GetArrayLength() > 0)
                {
                    // Success! Log entry was found in Loki
                    _lastSuccessfulVerification = DateTime.UtcNow;
                    return HealthCheckResult.Healthy(
                        $"Loki logging pipeline verified: test entry '{testMarker}' successfully written and retrieved");
                }

                // Entry not found - this could mean the sink hasn't flushed yet or there's a pipeline issue
                return HealthCheckResult.Degraded(
                    $"Test log entry written but not found in Loki after 2 seconds. Marker: {testMarker}. " +
                    "This may indicate the Serilog Loki sink is not flushing or there's a configuration mismatch.");
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException ex)
            {
                return HealthCheckResult.Unhealthy(
                    $"Loki logging health check timed out: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                return HealthCheckResult.Unhealthy(
                    $"Cannot reach Loki at {lokiUrl}: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                return HealthCheckResult.Unhealthy(
                    $"Invalid JSON response from Loki: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    $"Loki logging health check failed: {ex.Message}", ex);
            }
        }
    }
}
