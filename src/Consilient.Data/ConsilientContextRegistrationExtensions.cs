using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Data
{
    public static class ConsilientContextRegistrationExtensions
    {
        public static void RegisterConsilientDataServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ConsilientContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }
    }
}
