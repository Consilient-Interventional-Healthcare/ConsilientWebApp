using Consilient.Api.Infra.Hangfire;
using Hangfire;

namespace Consilient.Api.Init
{
    public static class HangfireServiceCollectionExtensions
    {
        public static void RegisterHangfire(this IServiceCollection services, string hanfireConnectionString)
        {
            services.AddHangfire(config =>
            {
                config
                    .UseSqlServerStorage(hanfireConnectionString)
                    .UseActivator(new WorkerJobActivator(services.BuildServiceProvider()));
            });
        }


    }
}
