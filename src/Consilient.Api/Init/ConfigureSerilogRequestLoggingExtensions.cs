using Serilog;
using Serilog.Events;

namespace Consilient.Api.Init;

internal static class ConfigureSerilogRequestLoggingExtensions
{
    public static void UseSerilogRequestLoggingWithEnrichment(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var endpoint = httpContext.GetEndpoint();
                var routePattern = (endpoint as RouteEndpoint)?.RoutePattern?.RawText;

                diagnosticContext.Set("RouteTemplate", routePattern ?? httpContext.Request.Path.Value ?? "unknown");
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

                var actionDescriptor = endpoint?.Metadata
                    .GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();
                if (actionDescriptor != null)
                {
                    diagnosticContext.Set("Controller", actionDescriptor.ControllerName);
                    diagnosticContext.Set("Action", actionDescriptor.ActionName);
                }
            };

            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null || httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;
                if (httpContext.Response.StatusCode >= 400 || elapsed > 500)
                    return LogEventLevel.Warning;
                return LogEventLevel.Information;
            };
        });
    }
}
