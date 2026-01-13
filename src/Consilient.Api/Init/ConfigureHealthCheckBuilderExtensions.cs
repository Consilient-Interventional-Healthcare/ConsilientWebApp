using Consilient.Api.Infra.HealthChecks;
using Consilient.Data;
using Consilient.Users.Services;

namespace Consilient.Api.Init
{
    internal static class ConfigureHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder ConfigureHealthChecks(this IServiceCollection services, UserServiceConfiguration userServiceConfig)
        {
            services.AddHttpClient<LokiHealthCheck>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15); // Allow time for connectivity + pipeline checks
            });

            var healthChecksBuilder = services.AddHealthChecks()
                .AddDbContextCheck<ConsilientDbContext>()
                .AddCheck<LokiHealthCheck>("loki", tags: ["infrastructure", "logging"]);

            if (userServiceConfig.OAuth?.Enabled == true)
            {
                services.AddHttpClient<MicrosoftOAuthHealthCheck>(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(15); // Allow time for discovery + JWKS + token checks
                });

                healthChecksBuilder.AddCheck<MicrosoftOAuthHealthCheck>("microsoft_oauth", tags: ["infrastructure", "authentication"]);
            }

            return healthChecksBuilder;
        }
    }
}
