using Consilient.Billing.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Billing.Services;

public static class BillingRegistrationExtension
{
    public static void RegisterBillingServices(this IServiceCollection services)
    {
        services.AddScoped<IBillingCodeService, BillingCodeService>();
    }
}
