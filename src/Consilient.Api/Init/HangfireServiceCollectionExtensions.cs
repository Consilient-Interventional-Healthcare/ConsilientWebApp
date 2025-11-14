using Consilient.Api.Infra.Hangfire;
using Hangfire;

namespace Consilient.Api.Init
{
    public static class HangfireServiceCollectionExtensions
    {
        public static void RegisterHangfire(this IServiceCollection services, string hangfireConnectionString)
        {
            services.AddHangfire(config =>
            {
                config
                    .UseSqlServerStorage(hangfireConnectionString)
                    .UseActivator(new WorkerJobActivator(services.BuildServiceProvider()));
            });
        }


    }
}
