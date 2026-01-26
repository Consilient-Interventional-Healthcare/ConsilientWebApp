using Consilient.BackgroundHost.Infra.Security;
using Consilient.Infrastructure.Injection;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Consilient.BackgroundHost.Init
{
    internal static class ConfigureHangfireDashboardExtensions
    {
        /// <summary>
        /// Configures Hangfire dashboard with Azure Entra authentication in Azure,
        /// or open access locally/Docker.
        /// </summary>
        public static IApplicationBuilder UseHangfireDashboardWithAuth(
            this IApplicationBuilder app,
            IHostEnvironment environment)
        {
            var dashboardOptions = new DashboardOptions
            {
                DashboardTitle = $"{environment.ApplicationName} ({environment.EnvironmentName.ToUpper()})"
            };

            // Configure authorization based on environment
            if (AzureEnvironment.IsRunningInAzure)
            {
                // In Azure: require Azure Entra authentication
                dashboardOptions.Authorization = [new EntraAuthorizationFilter()];
            }
            else
            {
                // Local/Docker: allow all requests (no auth required)
                dashboardOptions.Authorization = [];
            }

            app.UseHangfireDashboard("/hangfire", dashboardOptions);

            return app;
        }
    }
}
