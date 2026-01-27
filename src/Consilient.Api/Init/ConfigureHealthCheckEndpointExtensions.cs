using Consilient.Infrastructure.Injection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace Consilient.Api.Init;

internal static class ConfigureHealthCheckEndpointExtensions
{
    /// <summary>
    /// Maps the health check endpoint with JSON response format.
    /// </summary>
    public static IEndpointRouteBuilder MapHealthCheckEndpoint(
        this IEndpointRouteBuilder endpoints,
        string path = "/health")
    {
        endpoints.MapHealthChecks(path, new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
        });

        return endpoints;
    }
}
