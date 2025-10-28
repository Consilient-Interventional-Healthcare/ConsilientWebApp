using Consilient.WebApp.Infra;

namespace Consilient.WebApp.Init
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCurrentUser(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            return services;
        }
    }
}