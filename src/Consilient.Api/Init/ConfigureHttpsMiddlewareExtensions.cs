using Consilient.Infrastructure.Injection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Consilient.Api.Init;

internal static class ConfigureHttpsMiddlewareExtensions
{
    /// <summary>
    /// Configures HTTPS/HSTS middleware based on environment.
    /// In Azure App Service, HTTPS/TLS is handled at the platform level.
    /// In local development, enables HTTPS redirection for testing.
    /// </summary>
    public static IApplicationBuilder UseHttpsMiddleware(
        this IApplicationBuilder app,
        IHostEnvironment environment)
    {
        if (AzureEnvironment.IsRunningInAzure)
        {
            // In Azure App Service, HTTPS/TLS is handled at the platform level.
            // The container listens on HTTP/80, and App Service manages TLS termination.
            // HSTS headers are added but HTTPS redirection is not needed.
            app.UseHsts();
        }
        else if (!environment.IsProduction())
        {
            // In local development, enable HTTPS redirection for testing
            // (from http://localhost:8090 to https://localhost:8091)
            app.UseHttpsRedirection();
        }

        return app;
    }
}
