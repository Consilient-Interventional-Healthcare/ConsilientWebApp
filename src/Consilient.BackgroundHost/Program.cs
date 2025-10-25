using Consilient.BackgroundHost.Configuration;
using Consilient.BackgroundHost.Infra.Security;
using Consilient.BackgroundHost.Init;
using Consilient.Data;
using Consilient.Employees.Services;
using Consilient.Infrastructure.EmailMonitor;
using Consilient.Infrastructure.Injection;
using Consilient.Infrastructure.Logging;
using Consilient.Infrastructure.Logging.Configuration;
using Consilient.Insurances.Services;
using Consilient.Patients.Services;
using Consilient.Shared.Services;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Consilient.BackgroundHost
{
    internal static class Program
    {
        const string _configurationFile = "appsettings.json";
        const string _environmentConfigurationFile = "appsettings.{0}.json";
        const string _hangfireConnectionStringName = "HangfireConnection";
        const string _defaultConnectionStringName = "DefaultConnection";
        const string _loggingSectionName = "Logging";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile(_configurationFile, optional: true, reloadOnChange: true)
                .AddJsonFile(string.Format(_environmentConfigurationFile, builder.Environment.EnvironmentName), optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Add services to the container.
            var applicationSettings = builder.Services.RegisterApplicationSettings<ApplicationSettings>(builder.Configuration);
            var hangfireConnectionString = builder.Configuration.GetConnectionString(_hangfireConnectionStringName) ?? throw new Exception($"{_hangfireConnectionStringName} missing");
            var connectionString = builder.Configuration.GetConnectionString(_defaultConnectionStringName) ?? throw new ArgumentException($"{_defaultConnectionStringName} missing");

            builder.Services.RegisterDataContext(connectionString);
            builder.Services.RegisterDataContext(connectionString);
            builder.Services.RegisterEmailMonitorServices(applicationSettings.Email.Monitor);
            builder.Services.RegisterEmployeeServices();
            builder.Services.RegisterHangfireServices(hangfireConnectionString);
            builder.Services.RegisterInsuranceServices();
            builder.Services.RegisterPatientServices();
            builder.Services.RegisterSharedServices();

            var loggingConfiguration = builder.Configuration.GetSection(_loggingSectionName).Get<LoggingConfiguration>() ?? throw new NullReferenceException($"{_loggingSectionName} missing");
            var labels = new Dictionary<string, string>
            {
                { LabelConstants.App, builder.Environment.ApplicationName },
                { LabelConstants.Env, builder.Environment.EnvironmentName.ToLower() }
            };
            builder.Services.RegisterLogging(loggingConfiguration, labels);

            var app = builder.Build();

            app.UseHangfireDashboard(string.Empty, new DashboardOptions
            {
                DashboardTitle = $"{builder.Environment.ApplicationName} ({builder.Environment.EnvironmentName.ToUpper()})",
                Authorization = [new MyAuthorizationFilter()]
            });

            app.Run();
        }
    }
}
