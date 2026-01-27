using Microsoft.AspNetCore.CookiePolicy;

namespace Consilient.Api.Init;

internal static class ConfigureCookiePolicyServiceCollectionExtensions
{
    public static IServiceCollection ConfigureCookiePolicy(this IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            // Use Unspecified to allow individual cookies to set their own SameSite policy.
            // CookieOptionsFactory sets SameSite.None for auth/CSRF cookies needed for cross-origin OAuth.
            options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            options.HttpOnly = HttpOnlyPolicy.Always;
            options.Secure = CookieSecurePolicy.Always;
        });

        return services;
    }
}