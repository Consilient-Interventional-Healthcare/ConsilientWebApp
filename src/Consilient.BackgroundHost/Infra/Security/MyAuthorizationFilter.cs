using Hangfire.Dashboard;

namespace Consilient.BackgroundHost.Infra.Security
{
    /// <summary>
    /// Legacy filter that allows all access (for local development only).
    /// </summary>
    internal class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }
}
