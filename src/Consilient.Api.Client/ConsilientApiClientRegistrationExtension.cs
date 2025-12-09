using Consilient.Api.Client.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Api.Client
{
    public static class ConsilientApiClientRegistrationExtension
    {
        public static void AddConsilientApiClient(this IServiceCollection services, ConsilientApiClientConfiguration configuration, Func<IServiceProvider, string?> getUserNameFunc)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(getUserNameFunc);

            services.AddScoped<IConsilientApiClient>(sp =>
            {
                return new ConsilientApiClient(() =>
                {
                    //return new HttpClient(new AddUserToHeaderHandler(GetUserName, new HttpClientHandler()))
                    //{
                    //    BaseAddress = new Uri(configuration.BaseUrl)
                    //};
                    //string? GetUserName() => getUserNameFunc(sp);
                    return new HttpClient
                    {
                        BaseAddress = new Uri(configuration.BaseUrl)
                    };
                });
            });

            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Employees);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Facilities);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().GraphQl);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Insurances);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Patients);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().Visits);
            services.AddScoped(sp => sp.GetRequiredService<IConsilientApiClient>().ServiceTypes);
        }
    }
}
