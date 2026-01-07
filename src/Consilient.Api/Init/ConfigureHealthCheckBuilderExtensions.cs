using Consilient.Api.Infra;
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

            return services.AddHealthChecks()
                .AddDbContextCheck<ConsilientDbContext>()
                .AddCheck<LokiHealthCheck>("loki", tags: ["infrastructure"]);
        }
    }

}
