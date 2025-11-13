using Consilient.BackgroundHost.Configuration;
using Consilient.BackgroundHost.Infra.Security;
using Consilient.BackgroundHost.Init;
using Consilient.Constants;
using Consilient.Data;
using Consilient.Employees.Services;
using Consilient.Infrastructure.EmailMonitor;
using Consilient.Infrastructure.Injection;
using Consilient.Infrastructure.Logging;
using Consilient.Infrastructure.Logging.Configuration;
using Consilient.Insurances.Services;
using Consilient.Patients.Services;
using Consilient.Shared.Services;
using Consilient.Visits.Services;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Consilient.BackgroundHost
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile(ApplicationConstants.ConfigurationFiles.AppSettings, optional: true, reloadOnChange: true)
                .AddJsonFile(string.Format(ApplicationConstants.ConfigurationFiles.EnvironmentAppSettings, builder.Environment.EnvironmentName), optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var logger = CreateLogger(builder);
            Log.Logger = logger;
            try
            {
                Log.Information("Starting {App} ({Environment})", builder.Environment.ApplicationName, builder.Environment.EnvironmentName);

                var defaultConnectionString = builder.Configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.Default) ?? throw new NullReferenceException($"{ApplicationConstants.ConnectionStrings.Default} missing");
                var hangfireConnectionString = builder.Configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.Hangfire) ?? throw new Exception($"{ApplicationConstants.ConnectionStrings.Hangfire} missing");
                var applicationSettings = builder.Services.RegisterApplicationSettings<ApplicationSettings>(builder.Configuration);

                builder.Services.RegisterDataContext(defaultConnectionString, builder.Environment.IsProduction());
                builder.Services.RegisterEmailMonitorServices(applicationSettings.Email.Monitor);
                builder.Services.RegisterEmployeeServices();
                builder.Services.RegisterHangfireServices(hangfireConnectionString);
                builder.Services.RegisterInsuranceServices();
                builder.Services.RegisterPatientServices();
                builder.Services.RegisterSharedServices();
                builder.Services.RegisterVisitServices();

                builder.Services.RegisterLogging(logger);

                var app = builder.Build();

                app.UseHangfireDashboard(string.Empty, new DashboardOptions
                {
                    DashboardTitle = $"{builder.Environment.ApplicationName} ({builder.Environment.EnvironmentName.ToUpper()})",
                    Authorization = [new MyAuthorizationFilter()]
                });

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.Information("Shutting down {App}", builder.Environment.ApplicationName);
                Log.CloseAndFlush();
            }
        }

        private static ILogger CreateLogger(WebApplicationBuilder builder)
        {
            var loggingConfiguration =
                builder.Configuration.GetSection(ApplicationConstants.ConfigurationSections.Logging)
                    .Get<LoggingConfiguration>() ??
                throw new NullReferenceException($"{ApplicationConstants.ConfigurationFiles.AppSettings} missing");

            var labels = new Dictionary<string, string>
            {
                { LabelConstants.App, builder.Environment.ApplicationName },
                { LabelConstants.Env, builder.Environment.EnvironmentName.ToLower() }
            };
            var logger = LoggerFactory.Create(loggingConfiguration, labels);
            return logger;
        }
    }
}
