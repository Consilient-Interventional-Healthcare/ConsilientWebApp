using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Mappers;
using Consilient.Infrastructure.ExcelImporter.Readers;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Infrastructure.ExcelImporter
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExcelImporter(this IServiceCollection services)
        {
            // Core services
            services.AddSingleton<IExcelReader, NpoiExcelReader>();
            services.AddSingleton(typeof(IRowMapper<>), typeof(ReflectionRowMapper<>));
            return services;
        }
    }

}
