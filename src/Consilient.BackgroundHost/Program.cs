using Consilient.Background.Workers;
using Consilient.BackgroundHost.Init;
using Consilient.Constants;
using Consilient.Data;
using Consilient.Employees.Services;
using Consilient.Infrastructure.ExcelImporter;
using Consilient.Infrastructure.Logging;
using Consilient.Infrastructure.Logging.Configuration;
using Consilient.Infrastructure.Storage;
using Consilient.Insurances.Services;
using Consilient.Patients.Services;
using Consilient.ProviderAssignments.Services;
using Consilient.Shared.Services;
using Consilient.Visits.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Consilient.BackgroundHost
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuration loading
            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile(ApplicationConstants.ConfigurationFiles.AppSettings, optional: true, reloadOnChange: true)
                .AddJsonFile(string.Format(ApplicationConstants.ConfigurationFiles.EnvironmentAppSettings, builder.Environment.EnvironmentName), optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddBackgroundHostAzureAppConfiguration(builder.Environment.EnvironmentName);

            var logger = CreateLogger(builder);
            Log.Logger = logger;

            try
            {
                Log.Information("Starting {App} ({Environment})", builder.Environment.ApplicationName, builder.Environment.EnvironmentName);

                var defaultConnectionString = builder.Configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.Default)
                    ?? throw new NullReferenceException($"{ApplicationConstants.ConnectionStrings.Default} missing");
                var hangfireConnectionString = builder.Configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.Hangfire)
                    ?? throw new Exception($"{ApplicationConstants.ConnectionStrings.Hangfire} missing");

                // Register LoggingOptions for LokiHealthCheck dependency injection
                var loggingConfiguration = builder.Configuration
                    .GetSection(ApplicationConstants.ConfigurationSections.Logging)
                    .Get<LoggingOptions>();

                if (loggingConfiguration != null)
                {
                    builder.Services.AddSingleton(loggingConfiguration);
                }

                // Configure cross-cutting concerns via Init extensions
                builder.Services.ConfigureAuthenticationOptions(builder.Configuration);
                builder.Services.ConfigureUserContext();

                // Register domain services
                builder.Services.RegisterCosilientDbContext(defaultConnectionString, builder.Environment.IsProduction());
                builder.Services.RegisterEmployeeServices();
                builder.Services.ConfigureHangfireServices(hangfireConnectionString);
                builder.Services.RegisterInsuranceServices();
                builder.Services.RegisterPatientServices();
                builder.Services.RegisterSharedServices();
                builder.Services.RegisterVisitServices();
                builder.Services.AddProviderAssignmentsServices(builder.Configuration);
                builder.Services.AddFileStorage(builder.Configuration);
                builder.Services.AddExcelImporter();
                builder.Services.AddWorkers();
                builder.Services.RegisterLogging(logger);

                // Configure health checks
                builder.Services.ConfigureHealthChecks(builder.Configuration);

                var app = builder.Build();

                // Map endpoints
                app.MapHealthCheckEndpoint();

                // Configure Hangfire dashboard with JWT authentication
                app.UseHangfireDashboardWithAuth(builder.Environment);

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
                    .Get<LoggingOptions>();

            if (loggingConfiguration == null)
            {
                // Fallback to console logger when LoggingOptions is not available
                // (e.g., local development without full config, or before AAC loads)
                return CreateTrivialLogger(builder);
            }

            var labels = new Dictionary<string, string>
            {
                { LabelConstants.App, builder.Environment.ApplicationName },
                { LabelConstants.Env, builder.Environment.EnvironmentName.ToLower() }
            };
            var logger = LoggerFactory.Create(loggingConfiguration, labels);
            return logger;
        }

        private static Serilog.Core.Logger CreateTrivialLogger(WebApplicationBuilder builder)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithProperty(LabelConstants.App, builder.Environment.ApplicationName)
                .Enrich.WithProperty(LabelConstants.Env, builder.Environment.EnvironmentName.ToLower())
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
