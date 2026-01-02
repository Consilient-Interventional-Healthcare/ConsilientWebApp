using Consilient.DoctorAssignments.Contracts;
using Consilient.DoctorAssignments.Services.Importer;
using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Transformers;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.DoctorAssignments.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDoctorAssignmentsServices(this IServiceCollection services)
        {
            services.AddSingleton<IRowTransformer<ExternalDoctorAssignment>, TrimStringsTransformer<ExternalDoctorAssignment>>();
            //services.AddSingleton<IRowTransformer<DoctorAssignment>, CalculateAgeFromDobTransformer>();
            services.AddSingleton<IRowValidator<ExternalDoctorAssignment>, DoctorAssignmentValidator>();
            services.AddScoped<IExcelImporter<ExternalDoctorAssignment>, ExcelImporter<ExternalDoctorAssignment>>();
            services.AddScoped<IImporterFactory, ImporterFactory>();
            services.AddScoped<IDoctorAssignmentsResolver>(sp => new DoctorAssignmentsResolver(sp.GetRequiredService<Data.ConsilientDbContext>()));

            return services;
        }
    }
}
