namespace Consilient.Api.Init;

internal static class ConfigureCorsServiceCollectionExtensions
{
    public const string DefaultCorsPolicyName = "DefaultCorsPolicy";

    public static IServiceCollection ConfigureCors(this IServiceCollection services, string[]? allowedOrigins)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicyName, policy =>
            {
                if (allowedOrigins is { Length: > 0 })
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
                else
                {
                    // Allow any origin when none configured (development/CLI tooling only)
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
            });
        });

        return services;
    }
}
