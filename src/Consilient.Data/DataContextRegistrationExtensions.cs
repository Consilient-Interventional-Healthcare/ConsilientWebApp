using Consilient.Data.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Data
{
    public static class DataContextRegistrationExtensions
    {
        public static void RegisterCosilientDbContext(this IServiceCollection services, string connectionString, bool isProduction)
        {
            services.AddDbContext<ConsilientDbContext>(options =>
            {
                options.ConfigureDataContext(connectionString, isProduction);
                options.AddInterceptors(new AuditableEntityInterceptor());          
                options.ConfigureDataContext(connectionString, isProduction, "__EFMigrationsHistory_Consilient", "dbo");
            });
        }

        public static void RegisterUserDbContext(this IServiceCollection services, string connectionString, bool isProduction)
        {
            services.AddDbContext<UsersDbContext>(options =>
            {
                // Configure migrations history table inside the UseSqlServer options delegate via ConfigureDataContext
                options.ConfigureDataContext(connectionString, isProduction, "__EFMigrationsHistory_Users", UsersDbContext.Schemas.Identity);
                options.AddInterceptors(new AuditableEntityInterceptor());
            });
        }
    }
}
