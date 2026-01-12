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
                client.Timeout = TimeSpan.FromSeconds(15); // Allow time for connectivity + pipeline checks
            });

            services.AddHttpClient<MicrosoftOAuthHealthCheck>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15); // Allow time for discovery + JWKS + token checks
            });

            return services.AddHealthChecks()
                .AddDbContextCheck<ConsilientDbContext>()
                .AddCheck<LokiHealthCheck>("loki", tags: ["infrastructure", "logging"])
                .AddCheck<MicrosoftOAuthHealthCheck>("microsoft_oauth", tags: ["infrastructure", "authentication"]);
        }
    }
}
