using Consilient.Infrastructure.Storage.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                services.AddSingleton<IFileStorage, AzureBlobFileStorage>();
            }
            else
            {
                services.AddSingleton<IFileStorage, LocalFileStorage>();
            }

            return services;
        }
    }
}
