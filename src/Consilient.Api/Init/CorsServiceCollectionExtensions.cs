using System;

namespace Consilient.Api.Init
{
    internal static class CorsServiceCollectionExtensions
    {
        public const string DefaultCorsPolicyName = "DefaultCorsPolicy";

        public static IServiceCollection RegisterCors(this IServiceCollection services, bool isDev, string[]? allowedOrigins)
        {
            // Strongly-typed, named CORS policy to avoid accidental AllowAnyOrigin in production.
            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, policy =>
                {
                    if (allowedOrigins == null || allowedOrigins.Length == 0)
                    {
                        if (isDev)
                        {
                            // Development convenience: allow local dev servers commonly used by frontends.
                            policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
                                  .AllowAnyMethod()
                                  .AllowAnyHeader()
                                  .AllowCredentials();
                        }
                        else
                        {
                            // No origins configured, default to disallowing all cross-origin requests.
                            policy.DisallowCredentials();
                            policy.WithOrigins(); // No origins
                        }
                    }
                    else
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    }
                });
            });

            return services;
        }
    }
}
