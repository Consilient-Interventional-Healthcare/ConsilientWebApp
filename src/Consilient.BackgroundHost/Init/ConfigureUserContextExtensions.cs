using Consilient.Common.Contracts;
using Consilient.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.BackgroundHost.Init;

internal static class ConfigureUserContextExtensions
{
    /// <summary>
    /// Registers SettableUserContext for background jobs.
    /// This allows workers to impersonate users when performing operations
    /// that require ICurrentUserService (e.g., audit logging, entity tracking).
    /// Must be registered BEFORE DbContext registration.
    /// </summary>
    public static IServiceCollection ConfigureUserContext(this IServiceCollection services)
    {
        services.AddScoped<SettableUserContext>();
        services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<SettableUserContext>());
        services.AddScoped<IUserContextSetter>(sp => sp.GetRequiredService<SettableUserContext>());

        return services;
    }
}
