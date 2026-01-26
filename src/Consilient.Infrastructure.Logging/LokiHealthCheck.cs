using Consilient.Infrastructure.Logging.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Consilient.Infrastructure.Logging
{
    /// <summary>
    /// Unified health check for Loki that performs:
    /// 1. Connectivity check - verifies Loki is reachable via /ready endpoint (no auth)
    /// 2. Pipeline verification - writes a test log and queries Loki to verify it arrived (with auth)
    ///
    /// Returns structured data for troubleshooting with separate status for each check.
    /// </summary>
    public class LokiHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly LoggingOptions? _loggingOptions;
        private readonly Serilog.ILogger _logger;

        // Cache the last successful pipeline verification to avoid hammering Loki
        private static DateTime _lastSuccessfulPipelineCheck = DateTime.MinValue;
        private static readonly TimeSpan PipelineCacheDuration = TimeSpan.FromMinutes(5);

        public LokiHealthCheck(HttpClient httpClient, LoggingOptions? loggingOptions)
        {
            _httpClient = httpClient;
            _loggingOptions = loggingOptions;
            _logger = Log.Logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var lokiUrl = _loggingOptions?.GrafanaLoki?.Url;
            var data = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(lokiUrl))
            {
                data["connectivity"] = "skipped";
                data["pipeline"] = "skipped";
                data["error"] = "Loki URL is not configured";
                return HealthCheckResult.Degraded("Loki URL is not configured", data: data);
            }

            data["lokiUrl"] = lokiUrl;

            // Step 1: Connectivity check (fast, no auth required)
            var connectivityResult = await CheckConnectivityAsync(lokiUrl, cancellationToken);
            data["connectivity"] = connectivityResult.Status;
            if (connectivityResult.Error != null)
            {
                data["connectivityError"] = connectivityResult.Error;
            }

            if (!connectivityResult.IsSuccess)
            {
                data["pipeline"] = "skipped";
                return HealthCheckResult.Unhealthy(
                    $"Loki connectivity failed: {connectivityResult.Error}",
                    data: data);
            }

            // Step 2: Pipeline verification (slower, with auth, cached)
            var pipelineResult = await CheckPipelineAsync(lokiUrl, cancellationToken);
            data["pipeline"] = pipelineResult.Status;
            if (pipelineResult.LastCheck != null)
            {
                data["lastPipelineCheck"] = pipelineResult.LastCheck.Value.ToString("o");
            }
            if (pipelineResult.Error != null)
            {
                data["pipelineError"] = pipelineResult.Error;
            }
            if (pipelineResult.TestMarker != null)
            {
                data["testMarker"] = pipelineResult.TestMarker;
            }

            // Determine overall status
            if (pipelineResult.IsSuccess)
            {
                var description = pipelineResult.Status == "cached"
                    ? $"Loki healthy (connectivity: ok, pipeline: verified at {pipelineResult.LastCheck:HH:mm:ss} UTC)"
                    : "Loki healthy (connectivity: ok, pipeline: verified)";
                return HealthCheckResult.Healthy(description, data: data);
            }

            // Connectivity ok but pipeline failed = Degraded
            return HealthCheckResult.Degraded(
                $"Loki reachable but pipeline check failed: {pipelineResult.Error}",
                data: data);
        }

        private async Task<ConnectivityCheckResult> CheckConnectivityAsync(string lokiUrl, CancellationToken cancellationToken)
        {
            try
            {
                var readyUrl = $"{lokiUrl.TrimEnd('/')}/ready";
                var response = await _httpClient.GetAsync(readyUrl, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return new ConnectivityCheckResult { IsSuccess = true, Status = "ok" };
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var preview = responseBody.Length > 200 ? responseBody[..200] : responseBody;

                return new ConnectivityCheckResult
                {
                    IsSuccess = false,
                    Status = "failed",
                    Error = $"HTTP {(int)response.StatusCode} from /ready. Response: {preview}"
                };
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException ex)
            {
                return new ConnectivityCheckResult
                {
                    IsSuccess = false,
                    Status = "timeout",
                    Error = $"Timeout after {_httpClient.Timeout.TotalSeconds}s: {ex.Message}"
                };
            }
            catch (HttpRequestException ex)
            {
                return new ConnectivityCheckResult
                {
                    IsSuccess = false,
                    Status = "unreachable",
                    Error = $"Cannot reach Loki: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ConnectivityCheckResult
                {
                    IsSuccess = false,
                    Status = "error",
                    Error = ex.Message
                };
            }
        }

        private async Task<PipelineCheckResult> CheckPipelineAsync(string lokiUrl, CancellationToken cancellationToken)
        {
            // Check cache first
            if (DateTime.UtcNow - _lastSuccessfulPipelineCheck < PipelineCacheDuration)
            {
                return new PipelineCheckResult
                {
                    IsSuccess = true,
                    Status = "cached",
                    LastCheck = _lastSuccessfulPipelineCheck
                };
            }

            try
            {
                // Write a unique test log entry using Fatal level to bypass filtering
                var testMarker = $"HEALTHCHECK_{Guid.NewGuid():N}";
                var timestamp = DateTimeOffset.UtcNow;

                _logger.Write(LogEventLevel.Fatal, "Loki health check marker: {TestMarker}", testMarker);

                // Wait for Serilog's batch sink to flush
                await Task.Delay(2000, cancellationToken);

                // Query Loki for the test entry
                var startNs = (timestamp.AddMinutes(-1).ToUnixTimeMilliseconds() * 1_000_000).ToString();
                var endNs = (DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeMilliseconds() * 1_000_000).ToString();

                var query = Uri.EscapeDataString($"{{app=~\".+\"}} |= \"{testMarker}\"");
                var lokiQueryUrl = $"{lokiUrl.TrimEnd('/')}/loki/api/v1/query_range?query={query}&start={startNs}&end={endNs}&limit=1";

                var request = new HttpRequestMessage(HttpMethod.Get, lokiQueryUrl);

                // Add Basic Auth if credentials are configured
                var username = _loggingOptions?.GrafanaLoki?.Username;
                var password = _loggingOptions?.GrafanaLoki?.Password;
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    var preview = errorBody.Length > 200 ? errorBody[..200] : errorBody;

                    return new PipelineCheckResult
                    {
                        IsSuccess = false,
                        Status = "query_failed",
                        Error = $"HTTP {(int)response.StatusCode} querying Loki. Response: {preview}",
                        TestMarker = testMarker
                    };
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("result", out var result) &&
                    result.GetArrayLength() > 0)
                {
                    // Success! Log entry was found in Loki
                    _lastSuccessfulPipelineCheck = DateTime.UtcNow;
                    return new PipelineCheckResult
                    {
                        IsSuccess = true,
                        Status = "ok",
                        LastCheck = _lastSuccessfulPipelineCheck,
                        TestMarker = testMarker
                    };
                }

                return new PipelineCheckResult
                {
                    IsSuccess = false,
                    Status = "not_found",
                    Error = "Test log written but not found in Loki after 2s. Sink may not be flushing or labels mismatch.",
                    TestMarker = testMarker
                };
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException ex)
            {
                return new PipelineCheckResult
                {
                    IsSuccess = false,
                    Status = "timeout",
                    Error = $"Pipeline check timed out: {ex.Message}"
                };
            }
            catch (HttpRequestException ex)
            {
                return new PipelineCheckResult
                {
                    IsSuccess = false,
                    Status = "query_error",
                    Error = $"Failed to query Loki: {ex.Message}"
                };
            }
            catch (JsonException ex)
            {
                return new PipelineCheckResult
                {
                    IsSuccess = false,
                    Status = "invalid_response",
                    Error = $"Invalid JSON from Loki: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new PipelineCheckResult
                {
                    IsSuccess = false,
                    Status = "error",
                    Error = ex.Message
                };
            }
        }

        private record ConnectivityCheckResult
        {
            public bool IsSuccess { get; init; }
            public required string Status { get; init; }
            public string? Error { get; init; }
        }

        private record PipelineCheckResult
        {
            public bool IsSuccess { get; init; }
            public required string Status { get; init; }
            public string? Error { get; init; }
            public DateTime? LastCheck { get; init; }
            public string? TestMarker { get; init; }
        }
    }
}
