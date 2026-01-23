using Consilient.Infrastructure.Storage.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Consilient.Infrastructure.Storage
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds file storage services to the service collection.
        /// Registers either LocalFileStorage or AzureBlobFileStorage based on configuration.
        /// </summary>
        public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>()
                          ?? new FileStorageOptions();

            services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));

            if (string.Equals(options.Provider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<AzureBlobFileStorage>();
                services.AddSingleton<IFileStorage>(sp => sp.GetRequiredService<AzureBlobFileStorage>());
            }
            else
            {
                services.AddSingleton<IFileStorage, LocalFileStorage>();
            }

            return services;
        }

        /// <summary>
        /// Adds the Azure Blob Storage health check.
        /// Only registers if the FileStorage provider is configured as "AzureBlob".
        /// </summary>
        /// <param name="builder">The health checks builder.</param>
        /// <param name="configuration">The configuration to check the storage provider.</param>
        /// <returns>The health checks builder for chaining.</returns>
        public static IHealthChecksBuilder AddAzureBlobStorageHealthCheck(
            this IHealthChecksBuilder builder,
            IConfiguration configuration)
        {
            var options = configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>()
                          ?? new FileStorageOptions();

            if (string.Equals(options.Provider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddCheck<AzureBlobStorageHealthCheck>("azure_blob_storage", tags: ["infrastructure", "storage"]);
            }

            return builder;
        }
    }
}
