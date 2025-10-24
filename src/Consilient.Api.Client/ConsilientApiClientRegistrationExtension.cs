using Consilient.Api.Client.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Api.Client
{
    public static class ConsilientApiClientRegistrationExtension
    {
        public static void AddConsilientApiClient(this IServiceCollection services, ConsilientApiClientConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            services.AddScoped<IConsilientApiClient>(sp =>
            {
                return new ConsilientApiClient(configuration);
            });
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Patients);
        }
    }
}
