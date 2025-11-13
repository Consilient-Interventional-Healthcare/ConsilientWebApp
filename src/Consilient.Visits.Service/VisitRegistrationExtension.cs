using Consilient.Visits.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Visits.Services
{
    public static class VisitRegistrationExtension
    {
        public static void RegisterVisitServices(this IServiceCollection services)
        {
            services.AddScoped<IVisitService, VisitService>();
            services.AddScoped<IVisitStagingService, VisitStagingService>();
        }
    }
}
