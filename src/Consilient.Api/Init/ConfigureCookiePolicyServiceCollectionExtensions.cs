using Microsoft.AspNetCore.CookiePolicy;

namespace Consilient.Api.Init
{
    internal static class ConfigureCookiePolicyServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureCookiePolicy(this IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.HttpOnly = HttpOnlyPolicy.Always;
                options.Secure = CookieSecurePolicy.SameAsRequest;

                options.OnAppendCookie = cookieContext =>
                {
                    cookieContext.CookieOptions.Secure = cookieContext.Context.Request.IsHttps;
                    cookieContext.CookieOptions.HttpOnly = true;
                };

                options.OnDeleteCookie = cookieContext =>
                {
                    cookieContext.CookieOptions.Secure = cookieContext.Context.Request.IsHttps;
                    cookieContext.CookieOptions.HttpOnly = true;
                };
            });

            return services;
        }
    }
}