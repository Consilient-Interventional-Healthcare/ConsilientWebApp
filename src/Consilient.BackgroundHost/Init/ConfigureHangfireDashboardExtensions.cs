using Consilient.BackgroundHost.Configuration;
using Consilient.BackgroundHost.Infra.Security;
using Consilient.Infrastructure.Injection;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Consilient.BackgroundHost.Init
{
    internal static class ConfigureHangfireDashboardExtensions
    {
        /// <summary>
        /// Configures Hangfire dashboard with JWT authentication.
        /// The dashboard is protected using the same JWT token mechanism as the API.
        /// </summary>
        public static IApplicationBuilder UseHangfireDashboardWithAuth(
            this IApplicationBuilder app,
            IHostEnvironment environment)
        {
            var dashboardOptions = new DashboardOptions
            {
                DashboardTitle = $"{environment.ApplicationName} ({environment.EnvironmentName.ToUpper()})"
            };

            // Only require authentication when running in Azure
            if (AzureEnvironment.IsRunningInAzure)
            {
                var authOptions = app.ApplicationServices.GetRequiredService<IOptions<AuthenticationSettings>>();
                dashboardOptions.Authorization = [new JwtAuthorizationFilter(authOptions)];
            }

            app.UseHangfireDashboard("/hangfire", dashboardOptions);

            return app;
        }
    }
}
