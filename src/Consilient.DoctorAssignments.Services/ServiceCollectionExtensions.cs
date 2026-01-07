using Consilient.DoctorAssignments.Contracts;
using Consilient.DoctorAssignments.Services.Importer;
using Consilient.Infrastructure.ExcelImporter.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.DoctorAssignments.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDoctorAssignmentsServices(this IServiceCollection services)
        {
            // Register data sink for doctor assignments import
            services.AddScoped<IDataSink, EFCoreStagingDoctorAssignmentSink>();
            services.AddScoped<ISinkProvider, TrivialSinkProvider>();

            // ImporterFactory creates ExcelImporter instances directly (not via DI)
            // because ExcelImporter requires runtime parameters like ImportOptions
            services.AddScoped<IImporterFactory, ImporterFactory>();
            services.AddScoped<IDoctorAssignmentsResolver>(sp => new DoctorAssignmentsResolver(sp.GetRequiredService<Data.ConsilientDbContext>()));

            return services;
        }
    }
}
