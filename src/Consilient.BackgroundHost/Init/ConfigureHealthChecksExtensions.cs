using Consilient.Data;
using Consilient.Infrastructure.Injection;
using Consilient.Infrastructure.Logging;
using Consilient.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.BackgroundHost.Init;

internal static class ConfigureHealthChecksExtensions
{
    /// <summary>
    /// Configures health checks for BackgroundHost including database, Loki, and Azure Blob Storage (when running in Azure).
    /// </summary>
    public static IHealthChecksBuilder ConfigureHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks()
            .AddDbContextCheck<ConsilientDbContext>()
            .AddLokiHealthCheck(services);

        // Only add Azure Blob Storage health check when running in Azure App Service
        if (AzureEnvironment.IsRunningInAzure)
        {
            healthChecksBuilder.AddAzureBlobStorageHealthCheck(configuration);
        }

        return healthChecksBuilder;
    }
}
