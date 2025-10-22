using Consilient.BackgroundHost.Configuration;
using Consilient.BackgroundHost.Infra.Security;
using Consilient.EmailMonitor;
using Consilient.Infrastructure.Injection;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.BackgroundHost
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new NullReferenceException("connectionString");

            // Configure Hangfire
            var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireConnection") ?? throw new Exception("HangfireConnection missing");
            builder.Services.AddSingleton<IBackgroundJobClient, BackgroundJobClient>();
            builder.Services.AddSingleton<IRecurringJobManager, RecurringJobManager>();
            builder.Services.AddHangfire((provider, config) =>
            {
                config.UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
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
            builder.Services.AddHangfireServer((provider, options) =>
            {
                options.ShutdownTimeout = TimeSpan.FromMinutes(30);
                options.WorkerCount = Math.Max(Environment.ProcessorCount, 20);
                var workerRegistration = new WorkerRegistration(provider.GetRequiredService<IBackgroundJobClient>(), provider.GetRequiredService<IRecurringJobManager>(), provider.GetRequiredService<JobStorage>());
                workerRegistration.Register();
            });
            var applicationSettings = builder.Services.RegisterApplicationSettings<ApplicationSettings>(builder.Configuration);
            builder.Services.RegisterEmailMonitorServices(applicationSettings.Email.Monitor);

            var app = builder.Build();

            var environmentName = builder.Environment.EnvironmentName;
            app.UseHangfireDashboard(string.Empty, new DashboardOptions
            {
                DashboardTitle = $"{typeof(Program).Namespace!} ({environmentName.ToUpper()})",
                Authorization = [new MyAuthorizationFilter()]
            });

            app.Run();
        }
    }
}
