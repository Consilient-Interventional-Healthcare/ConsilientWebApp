using Consilient.Shared.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Shared.Services
{
    public static class SharedRegistrationExtension
    {
        public static void RegisterSharedServices(this IServiceCollection services)
        {
            services.AddScoped<IFacilityService, FacilityService>();
            services.AddScoped<IServiceTypeService, ServiceTypeService>();
        }
    }
}
