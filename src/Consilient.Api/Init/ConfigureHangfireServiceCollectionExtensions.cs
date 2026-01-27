using Consilient.Api.Infra.Hangfire;
using Hangfire;

namespace Consilient.Api.Init;

internal static class ConfigureHangfireServiceCollectionExtensions
{
    public static void ConfigureHangfire(this IServiceCollection services, string hangfireConnectionString)
    {
        services.AddHangfire((provider, config) =>
        {
            config
                .UseSqlServerStorage(hangfireConnectionString)
                .UseActivator(new WorkerJobActivator(provider));
        });
    }
}
