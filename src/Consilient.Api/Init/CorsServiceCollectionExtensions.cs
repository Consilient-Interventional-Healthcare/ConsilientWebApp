namespace Consilient.Api.Init
{
    internal static class CorsServiceCollectionExtensions
    {
        public const string DefaultCorsPolicyName = "DefaultCorsPolicy";

        public static IServiceCollection RegisterCors(this IServiceCollection services, string[]? allowedOrigins)
        {
            // Strongly-typed, named CORS policy to avoid accidental AllowAnyOrigin in production.
            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, policy =>
                {
                    if (allowedOrigins == null || allowedOrigins.Length == 0)
                    {
                        // No origins configured, default to disallowing all cross-origin requests.
                        policy.DisallowCredentials();
                        policy.WithOrigins(); // No origins
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
