using Consilient.Infrastructure.Logging.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Consilient.Api.Infra.HealthChecks
{
    internal class LokiHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly LoggingConfiguration? _loggingConfiguration;

        public LokiHealthCheck(HttpClient httpClient, LoggingConfiguration? loggingConfiguration)
        {
            _httpClient = httpClient;
            _loggingConfiguration = loggingConfiguration;
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

            try
            {
                var readyUrl = $"{lokiUrl.TrimEnd('/')}/ready";
                var response = await _httpClient.GetAsync(readyUrl, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy("Loki is reachable");
                }

                // Enhanced: Include response body for better diagnostics
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var preview = responseBody.Length > 200 ? responseBody.Substring(0, 200) : responseBody;

                return HealthCheckResult.Unhealthy(
                    $"Loki returned status code {(int)response.StatusCode} from {readyUrl}. Response: {preview}"
                );
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException ex)
            {
                // Timeout (not cancellation)
                return HealthCheckResult.Unhealthy($"Loki health check timed out after 5 seconds for URL: {lokiUrl}/ready", ex);
            }
            catch (HttpRequestException ex)
            {
                // Enhanced: More detailed connection error
                return HealthCheckResult.Unhealthy($"Cannot reach Loki at {lokiUrl}/ready: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Loki health check failed: {ex.Message}", ex);
            }
        }
    }
}
