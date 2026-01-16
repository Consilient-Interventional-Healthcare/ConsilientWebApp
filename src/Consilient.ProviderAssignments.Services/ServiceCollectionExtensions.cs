using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Services.Importer;
using Consilient.Infrastructure.ExcelImporter.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.ProviderAssignments.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProviderAssignmentsServices(this IServiceCollection services)
        {
            // Register data sink for provider assignments import
            services.AddScoped<IDataSink, EFCoreStagingProviderAssignmentSink>();
            services.AddScoped<ISinkProvider, TrivialSinkProvider>();

            // ImporterFactory creates ExcelImporter instances directly (not via DI)
            // because ExcelImporter requires runtime parameters like ImportOptions
            services.AddScoped<IImporterFactory, ImporterFactory>();
            services.AddScoped<IProviderAssignmentsResolver>(sp => new ProviderAssignmentsResolver(sp.GetRequiredService<Data.ConsilientDbContext>()));

            return services;
        }
    }
}
