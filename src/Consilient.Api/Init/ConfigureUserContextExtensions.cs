using Consilient.Api.Infra.Authentication;
using Consilient.Common.Contracts;
using Consilient.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Api.Init;

internal static class ConfigureUserContextExtensions
{
    /// <summary>
    /// Registers user context services for the API.
    /// Must be called BEFORE DbContext registration (required by HospitalizationStatusChangeInterceptor).
    /// </summary>
    public static IServiceCollection ConfigureUserContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        // NoOp setter for Hangfire dependency resolution (actual setting happens in BackgroundHost)
        services.AddScoped<IUserContextSetter, NoOpUserContextSetter>();

        return services;
    }
}
