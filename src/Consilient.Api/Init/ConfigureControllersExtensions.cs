using Consilient.Api.Configuration;
using Consilient.Api.Infra.ModelBinders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Consilient.Api.Init
{
    internal static class ConfigureControllersExtensions
    {
        /// <summary>
        /// Configures MVC controllers with authorization policy and custom model binders.
        /// </summary>
        public static IServiceCollection ConfigureControllers(
            this IServiceCollection services,
            AuthenticationOptions authOptions,
            IHostEnvironment environment)
        {
            services.AddControllers(options =>
            {
                if (authOptions.Enabled && (environment.IsProduction() || environment.IsDevelopment()))
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

                    options.Filters.Add(new AuthorizeFilter(policy));
                }
                // Ensure our provider runs before defaults
                options.ModelBinderProviders.Insert(0, new YyyyMmDdDateModelBinderProvider());
            }).AddNewtonsoftJson();

            return services;
        }
    }
}
