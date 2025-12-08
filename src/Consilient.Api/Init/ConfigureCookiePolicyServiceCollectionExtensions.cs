using Microsoft.AspNetCore.CookiePolicy;

namespace Consilient.Api.Init
{
    internal static class ConfigureCookiePolicyServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureCookiePolicy(this IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Lax; // Changed from None
                options.HttpOnly = HttpOnlyPolicy.Always;
                options.Secure = CookieSecurePolicy.Always; // Changed from SameAsRequest
            });

            return services;
        }
    }
}