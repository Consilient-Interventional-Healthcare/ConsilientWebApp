using Hangfire.Dashboard;

namespace Consilient.BackgroundHost.Infra.Security
{
    internal class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }
}
