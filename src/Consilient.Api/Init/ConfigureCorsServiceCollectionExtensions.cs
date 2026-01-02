namespace Consilient.Api.Init
{
    internal static class ConfigureCorsServiceCollectionExtensions
    {
        public const string DefaultCorsPolicyName = "DefaultCorsPolicy";

        public static IServiceCollection ConfigureCors(this IServiceCollection services, string[] allowedOrigins)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            return services;
        }
    }
}
