using Consilient.Api.Client.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Api.Client
{
    public static class ConsilientApiClientRegistrationExtension
    {
        public static void AddConsilientApiClient(this IServiceCollection services, ConsilientApiClientConfiguration configuration, Func<string> getUserNameFunc)
        {
            ArgumentNullException.ThrowIfNull(configuration);


            // Register the root client; pass IHttpClientFactory so ConsilientApiClient can create the named client.
            services.AddScoped<IConsilientApiClient>(sp =>
            {
                return new ConsilientApiClient(() =>
                {
                    return new HttpClient(new AddUserToHeaderHandler(getUserNameFunc))
                    {
                        BaseAddress = new Uri(configuration.BaseUrl)
                    };
                });
            });

            // Register each API property as scoped, resolved from the root client instance
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Employees);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Facilities);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Insurances);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Patients);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().PatientVisits);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().ServiceTypes);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().StagingPatientVisits);
        }
    }
}
