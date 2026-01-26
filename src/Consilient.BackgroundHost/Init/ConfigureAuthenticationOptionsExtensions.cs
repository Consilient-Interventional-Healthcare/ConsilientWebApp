using Consilient.BackgroundHost.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.BackgroundHost.Init
{
    internal static class ConfigureAuthenticationOptionsExtensions
    {
        /// <summary>
        /// Registers AuthenticationSettings with IOptions pattern for Hangfire dashboard JWT validation.
        /// </summary>
        public static IServiceCollection ConfigureAuthenticationOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<AuthenticationSettings>()
                .Bind(configuration.GetSection(AuthenticationSettings.SectionName))
                .ValidateOnStart();

            return services;
        }
    }
}
