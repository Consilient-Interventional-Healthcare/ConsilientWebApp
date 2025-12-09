using Consilient.Hospitalizations.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Hospitalizations.Services
{
    public static class HospitalizationRegistrationExtension
    {
        public static void RegisterHospitalizationServices(this IServiceCollection services)
        {
            services.AddScoped<IHospitalizationService, HospitalizationService>();
        }
    }
}
