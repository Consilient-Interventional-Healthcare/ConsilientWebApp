using Consilient.Background.Workers.ProviderAssignments;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Background.Workers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkers(this IServiceCollection services)
    {
        services.AddScoped<ProviderAssignmentsImportWorker>();
        services.AddScoped<ProviderAssignmentsResolutionWorker>();
        return services;
    }
}
