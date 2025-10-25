using Consilient.Insurances.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Insurances.Services
{
    public static class InsuranceRegistrationExtension
    {
        public static void AddInsuranceServices(this IServiceCollection services)
        {
            services.AddScoped<IInsuranceService, InsuranceService>();
        }
    }
}
