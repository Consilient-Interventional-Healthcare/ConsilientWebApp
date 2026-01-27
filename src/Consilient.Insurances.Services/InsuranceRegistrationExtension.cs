using Consilient.Insurances.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Insurances.Services;

public static class InsuranceRegistrationExtension
{
    public static void RegisterInsuranceServices(this IServiceCollection services)
    {
        services.AddScoped<IInsuranceService, InsuranceService>();
    }
}
