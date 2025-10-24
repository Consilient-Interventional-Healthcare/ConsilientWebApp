using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Data
{
    public static class DataContextRegistrationExtensions
    {
        public static void RegisterDataContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ConsilientDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }
    }
}
