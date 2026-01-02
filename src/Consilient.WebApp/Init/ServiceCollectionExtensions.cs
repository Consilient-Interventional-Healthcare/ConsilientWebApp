using Consilient.WebApp.Infra;

namespace Consilient.WebApp.Init
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCurrentUser(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
        }
    }
}