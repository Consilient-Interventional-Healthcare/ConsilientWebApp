using Consilient.Api.Infra.HealthChecks;
using Consilient.Data;

namespace Consilient.Api.Init
{
    internal static class ConfigureHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder ConfigureHealthChecks(this IServiceCollection services)
        {
            services.AddHttpClient<LokiHealthCheck>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
            });

            services.AddHttpClient<LokiLoggingHealthCheck>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            return services.AddHealthChecks()
                .AddDbContextCheck<ConsilientDbContext>()
                .AddCheck<LokiHealthCheck>("loki", tags: ["infrastructure"])
                .AddCheck<LokiLoggingHealthCheck>("loki-logging", tags: ["infrastructure", "logging"]);
        }
    }
}
