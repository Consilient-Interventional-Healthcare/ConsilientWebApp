using Hangfire.Dashboard;

namespace Consilient.BackgroundHost.Infra.Security;

/// <summary>
/// Hangfire dashboard authorization filter that checks Azure Entra authentication.
/// Requires the user to be authenticated via the ASP.NET Core authentication middleware.
/// </summary>
internal class EntraAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true;
    }
}
