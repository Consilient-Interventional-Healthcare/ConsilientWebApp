using Consilient.Infrastructure.Storage.Contracts;
using Consilient.Infrastructure.Storage.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

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
            // Read options once for conditional registration (before DI is built)
            var options = configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>()
                          ?? new FileStorageOptions();

            // Register validator and configuration with validation at startup
            services.AddSingleton<IValidateOptions<FileStorageOptions>, FileStorageOptionsValidator>();
            services.AddOptions<FileStorageOptions>()
                .Bind(configuration.GetSection(FileStorageOptions.SectionName))
                .ValidateOnStart();

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
        /// Adds the Azure Blob Storage health check if Azure Blob provider is configured.
        /// Checks if AzureBlobFileStorage is registered in the service collection (must be called after AddFileStorage).
        /// </summary>
        /// <param name="builder">The health checks builder.</param>
        /// <returns>The health checks builder for chaining.</returns>
        public static IHealthChecksBuilder AddAzureBlobStorageHealthCheck(this IHealthChecksBuilder builder)
        {
            // Check if AzureBlobFileStorage is registered (indicates Azure provider is configured)
            var isAzureProviderRegistered = builder.Services.Any(
                sd => sd.ServiceType == typeof(AzureBlobFileStorage));

            if (isAzureProviderRegistered)
            {
                builder.AddCheck<AzureBlobStorageHealthCheck>("azure_blob_storage", tags: ["infrastructure", "storage"]);
            }

            return builder;
        }

        /// <summary>
        /// Adds the Azure Blob Storage health check if Azure Blob provider is configured.
        /// </summary>
        /// <param name="builder">The health checks builder.</param>
        /// <param name="configuration">The configuration to check the storage provider.</param>
        /// <returns>The health checks builder for chaining.</returns>
        public static IHealthChecksBuilder AddAzureBlobStorageHealthCheck(
            this IHealthChecksBuilder builder,
            IConfiguration configuration)
        {
            // First check if AzureBlobFileStorage is already registered (AddFileStorage was called)
            var isAzureProviderRegistered = builder.Services.Any(
                sd => sd.ServiceType == typeof(AzureBlobFileStorage));

            if (isAzureProviderRegistered)
            {
                builder.AddCheck<AzureBlobStorageHealthCheck>("azure_blob_storage", tags: ["infrastructure", "storage"]);
                return builder;
            }

            // Fallback: read from configuration if AddFileStorage hasn't been called yet
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
