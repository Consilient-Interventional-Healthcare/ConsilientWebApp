using System.Diagnostics;
using Serilog.Context;

namespace Consilient.Api.Infra.Middleware;

/// <summary>
/// Middleware that enriches all log entries with request-level information:
/// RequestId, HttpMethod, and Path.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Use W3C trace context if available (distributed tracing), otherwise fall back to ASP.NET Core's TraceIdentifier
        var requestId = Activity.Current?.Id ?? context.TraceIdentifier;
        var httpMethod = context.Request.Method;
        var path = context.Request.Path.Value;

        // Push properties to Serilog's LogContext - automatically included in all log entries within this scope
        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("HttpMethod", httpMethod))
        using (LogContext.PushProperty("Path", path))
        {
            await _next(context);
        }
    }
}
