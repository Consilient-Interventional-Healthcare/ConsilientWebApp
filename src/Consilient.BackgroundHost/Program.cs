using Consilient.BackgroundHost.Configuration;
using Consilient.BackgroundHost.Infra.Security;
using Consilient.BackgroundHost.Init;
using Consilient.Data;
using Consilient.Employees.Services;
using Consilient.Infrastructure.EmailMonitor;
using Consilient.Infrastructure.Injection;
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
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Add services to the container.
            var applicationSettings = builder.Services.RegisterApplicationSettings<ApplicationSettings>(builder.Configuration);
            var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireConnection") ?? throw new Exception("HangfireConnection missing");
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentException("DefaultConnection missing");

            builder.Services.RegisterDataContext(connectionString);
            builder.Services.RegisterDataContext(connectionString);
            builder.Services.RegisterEmailMonitorServices(applicationSettings.Email.Monitor);
            builder.Services.RegisterEmployeeServices();
            builder.Services.RegisterHangfireServices(hangfireConnectionString);
            builder.Services.RegisterInsuranceServices();
            builder.Services.RegisterPatientServices();
            builder.Services.RegisterSharedServices();

            var app = builder.Build();

            app.UseHangfireDashboard(string.Empty, new DashboardOptions
            {
                DashboardTitle = $"{typeof(Program).Namespace!} ({builder.Environment.EnvironmentName.ToUpper()})",
                Authorization = [new MyAuthorizationFilter()]
            });

            app.Run();
        }
    }
}
