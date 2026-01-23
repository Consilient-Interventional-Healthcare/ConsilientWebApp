using Consilient.Api.Infra.HealthChecks;
using Consilient.Data;
using Consilient.Infrastructure.Logging;
using Consilient.Infrastructure.Storage;
using Consilient.Users.Services;

namespace Consilient.Api.Init
{
    internal static class ConfigureHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration, UserServiceConfiguration userServiceConfig)
        {
            var healthChecksBuilder = services.AddHealthChecks()
                .AddDbContextCheck<ConsilientDbContext>()
                .AddLokiHealthCheck(services)
                .AddAzureBlobStorageHealthCheck(configuration);

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
