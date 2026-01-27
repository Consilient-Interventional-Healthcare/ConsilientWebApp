using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Consilient.BackgroundHost.Init;

internal static class ConfigureHangfireServiceCollectionExtensions
{
    public static void ConfigureHangfireServices(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IBackgroundJobClient, BackgroundJobClient>();
        services.AddSingleton<IRecurringJobManager, RecurringJobManager>();
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                TransactionTimeout = TimeSpan.FromMinutes(15),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            });
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
            config.UseSimpleAssemblyNameTypeSerializer();
            config.UseRecommendedSerializerSettings();
        });
        services.AddHangfireServer((provider, options) =>
        {
            options.ShutdownTimeout = TimeSpan.FromMinutes(30);

            // Use fewer workers in development for faster startup
            var env = provider.GetRequiredService<IHostEnvironment>();
            options.WorkerCount = env.IsDevelopment()
                ? Math.Min(Environment.ProcessorCount, 5)
                : Math.Max(Environment.ProcessorCount, 20);

            var workerRegistration = new WorkerRegistration(provider.GetRequiredService<IRecurringJobManager>(), provider.GetRequiredService<JobStorage>());
            workerRegistration.Register();
        });

    }
}
