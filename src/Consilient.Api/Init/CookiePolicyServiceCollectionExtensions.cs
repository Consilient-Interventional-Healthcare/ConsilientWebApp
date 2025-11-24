using Microsoft.AspNetCore.CookiePolicy;

namespace Consilient.Api.Init
{
    internal static class CookiePolicyServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCookiePolicy(this IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
                options.HttpOnly = HttpOnlyPolicy.Always;
                options.Secure = CookieSecurePolicy.Always;

                // Best-effort: enforce properties for cookies appended/deleted during request lifecycle.
                options.OnAppendCookie = cookieContext =>
                {
                    cookieContext.CookieOptions.Secure = true;
                    cookieContext.CookieOptions.HttpOnly = true;
                    if (cookieContext.CookieOptions.SameSite == SameSiteMode.Unspecified)
                    {
                        cookieContext.CookieOptions.SameSite = SameSiteMode.Lax;
                    }
                };

                options.OnDeleteCookie = cookieContext =>
                {
                    cookieContext.CookieOptions.Secure = true;
                    cookieContext.CookieOptions.HttpOnly = true;
                    if (cookieContext.CookieOptions.SameSite == SameSiteMode.Unspecified)
                    {
                        cookieContext.CookieOptions.SameSite = SameSiteMode.Lax;
                    }
                };
            });

            return services;
        }
    }
}