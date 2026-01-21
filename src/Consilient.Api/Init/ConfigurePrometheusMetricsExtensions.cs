using Consilient.Api.Configuration;
using Prometheus;

namespace Consilient.Api.Init;

internal static class ConfigurePrometheusMetricsExtensions
{
    public static void UsePrometheusMetrics(this IApplicationBuilder app, PrometheusConfiguration config)
    {
        if (!config.Enabled)
        {
            return;
        }

        app.UseMetricServer();
        app.UseHttpMetrics();
    }
}
