using Consilient.Api.Infra.HealthChecks;
using Consilient.Data;
using Consilient.Infrastructure.Injection;
using Consilient.Infrastructure.Logging;
using Consilient.Infrastructure.Storage;
using Consilient.Users.Services;

namespace Consilient.Api.Init;

internal static class ConfigureHealthCheckBuilderExtensions
{
    public static IHealthChecksBuilder ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration, UserServiceOptions userServiceOptions)
    {
        var healthChecksBuilder = services.AddHealthChecks()
            .AddDbContextCheck<ConsilientDbContext>()
            .AddLokiHealthCheck(services);

        // Only add Azure Blob Storage health check when running in Azure App Service
        if (AzureEnvironment.IsRunningInAzure)
        {
            healthChecksBuilder.AddAzureBlobStorageHealthCheck(configuration);
        }

        if (userServiceOptions.OAuth?.Enabled == true)
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
