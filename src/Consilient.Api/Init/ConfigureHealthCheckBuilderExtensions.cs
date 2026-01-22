using Consilient.Api.Infra.HealthChecks;
using Consilient.Data;
using Consilient.Infrastructure.Storage.Contracts;
using Consilient.Users.Services;

namespace Consilient.Api.Init
{
    internal static class ConfigureHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration, UserServiceConfiguration userServiceConfig)
        {
            services.AddHttpClient<LokiHealthCheck>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15); // Allow time for connectivity + pipeline checks
            });

            var healthChecksBuilder = services.AddHealthChecks()
                .AddDbContextCheck<ConsilientDbContext>()
                .AddCheck<LokiHealthCheck>("loki", tags: ["infrastructure", "logging"]);

            // Add Azure Blob Storage health check only when using AzureBlob provider
            var fileStorageOptions = configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>()
                                     ?? new FileStorageOptions();
            if (string.Equals(fileStorageOptions.Provider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
            {
                healthChecksBuilder.AddCheck<AzureBlobStorageHealthCheck>("azure_blob_storage", tags: ["infrastructure", "storage"]);
            }

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
