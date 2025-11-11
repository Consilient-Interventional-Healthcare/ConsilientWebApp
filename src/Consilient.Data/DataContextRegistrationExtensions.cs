using Consilient.Data.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Data
{
    public static class DataContextRegistrationExtensions
    {
        public static void RegisterDataContext(this IServiceCollection services, string connectionString, bool isProduction)
        {
            services.AddDbContext<ConsilientDbContext>(options =>
            {
                options.ConfigureDataContext(connectionString, isProduction);
                options.AddInterceptors(new AuditableEntityInterceptor());
            });
        }
    }
}
