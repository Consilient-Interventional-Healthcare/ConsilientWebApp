using Consilient.Background.Workers.DoctorAssignments;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Background.Workers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWorkers(this IServiceCollection services)
        {
            services.AddScoped<DoctorAssignmentsImportWorker>();
            services.AddScoped<DoctorAssignmentsResolutionWorker>();
            services.AddScoped<DoctorAssignmentsImportWorkerEnqueuer>();
            return services;
        }
    }
}
