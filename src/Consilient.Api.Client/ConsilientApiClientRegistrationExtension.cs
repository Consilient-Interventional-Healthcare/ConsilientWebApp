using Consilient.Api.Client.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Api.Client
{
    public static class ConsilientApiClientRegistrationExtension
    {
        public static void AddConsilientApiClient(this IServiceCollection services, ConsilientApiClientConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            // Register the root client
            services.AddScoped<IConsilientApiClient>(sp =>
            {
                return new ConsilientApiClient(configuration);
            });

            // Register each API property as scoped, resolved from the root client instance
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Employees);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Facilities);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Insurances);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Patients);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().ServiceTypes);
        }
    }
}
