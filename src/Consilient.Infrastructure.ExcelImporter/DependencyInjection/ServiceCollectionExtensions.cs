using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Domain;
using Consilient.Infrastructure.ExcelImporter.Mappers;
using Consilient.Infrastructure.ExcelImporter.Readers;
using Consilient.Infrastructure.ExcelImporter.Transformers;
using Consilient.Infrastructure.ExcelImporter.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Infrastructure.ExcelImporter.DependencyInjection
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExcelImporter(this IServiceCollection services)
        {
            // Core services
            services.AddSingleton<IExcelReader, NpoiExcelReader>();
            services.AddSingleton(typeof(IRowMapper<>), typeof(ReflectionRowMapper<>));

            // Patient-specific registration
            services.AddDoctorAssignmentImporter();

            return services;
        }

        public static IServiceCollection AddDoctorAssignmentImporter(this IServiceCollection services)
        {
            // Validators
            services.AddSingleton<IRowValidator<DoctorAssignment>, DoctorAssignmentValidator>();

            // Transformers
            services.AddSingleton<IRowTransformer<DoctorAssignment>, TrimStringsTransformer<DoctorAssignment>>();
            services.AddSingleton<IRowTransformer<DoctorAssignment>, CalculateAgeFromDobTransformer>();

            // Importer
            services.AddScoped<IExcelImporter<DoctorAssignment>, ExcelImporter<DoctorAssignment>>();

            return services;
        }
    }

}