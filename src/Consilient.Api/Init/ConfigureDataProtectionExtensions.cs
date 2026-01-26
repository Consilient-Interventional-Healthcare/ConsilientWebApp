using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Api.Init
{
    internal static class ConfigureDataProtectionExtensions
    {
        /// <summary>
        /// Configures Data Protection with proper cryptographic algorithms.
        /// Note: In Azure App Service containers, data protection keys are ephemeral by design.
        /// For applications requiring persistent data protection keys across container restarts,
        /// configure blob storage via WEBSITE_DataProtectionKeysPath in App Service settings.
        /// </summary>
        public static IServiceCollection ConfigureDataProtection(this IServiceCollection services)
        {
            services.AddDataProtection()
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });

            return services;
        }
    }
}
