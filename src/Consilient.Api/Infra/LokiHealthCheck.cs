using Consilient.Infrastructure.Logging.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Consilient.Api.Infra
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

                return HealthCheckResult.Unhealthy($"Loki returned status code {(int)response.StatusCode}");
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                return HealthCheckResult.Unhealthy($"Cannot reach Loki: {ex.Message}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Loki health check failed: {ex.Message}");
            }
        }
    }
}
