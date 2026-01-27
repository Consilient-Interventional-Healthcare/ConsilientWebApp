using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Consilient.BackgroundHost.Init;

internal static class ConfigureHangfireDashboardExtensions
{
    /// <summary>
    /// Configures Hangfire dashboard with Entra authentication when running in Azure (or ForceEntraAuth is enabled),
    /// and without authentication when running locally.
    /// </summary>
    public static IApplicationBuilder UseHangfireDashboardWithAuth(
        this IApplicationBuilder app,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        var dashboardOptions = new DashboardOptions
        {
            DashboardTitle = $"{environment.ApplicationName} ({environment.EnvironmentName.ToUpper()})",
            // Empty array - let ASP.NET Core authorization handle it when Entra is enabled
            Authorization = []
        };

        if (ConfigureEntraAuthenticationExtensions.ShouldUseEntraAuth(configuration))
        {
            // Use endpoint routing with authorization policy
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHangfireDashboard("/hangfire", dashboardOptions)
                         .RequireAuthorization("HangfireAccess");
            });
        }
        else
        {
            // Local without override: no auth required
            app.UseHangfireDashboard("/hangfire", dashboardOptions);
        }

        return app;
    }
}
