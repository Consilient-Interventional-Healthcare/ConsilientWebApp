using Consilient.Api.Configuration;
using Prometheus;

namespace Consilient.Api.Init;

internal static class ConfigurePrometheusMetricsExtensions
{
    public static void UsePrometheusMetrics(this IApplicationBuilder app, PrometheusOptions prometheusOptions)
    {
        if (!prometheusOptions.Enabled)
        {
            return;
        }

        app.UseMetricServer();
        app.UseHttpMetrics();
    }
}
