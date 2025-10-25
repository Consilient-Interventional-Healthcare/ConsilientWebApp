using Consilient.Api.Configuration;
using Consilient.Api.Init;
using Consilient.Data;
using Consilient.Employees.Services;
using Consilient.Infrastructure.Injection;
using Consilient.Infrastructure.Logging;
using Consilient.Infrastructure.Logging.Configuration;
using Consilient.Insurances.Services;
using Consilient.Patients.Services;
using Consilient.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Consilient.Api
{
    internal static class Program
    {
        const string _configurationFile = "appsettings.json";
        const string _environmentConfigurationFile = "appsettings.{0}.json";
        //const string _hangfireConnectionStringName = "HangfireConnection";
        const string _defaultConnectionStringName = "DefaultConnection";
        const string _loggingSectionName = "Logging";

        public static void Main(string[] args)
        {
            const string version = "v1";

            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile(_configurationFile, optional: true, reloadOnChange: true)
                .AddJsonFile(string.Format(_environmentConfigurationFile, builder.Environment.EnvironmentName), optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Add services to the container.
            var applicationSettings = builder.Services.RegisterApplicationSettings<ApplicationSettings>(builder.Configuration);
            var connectionString = builder.Configuration.GetConnectionString(_defaultConnectionStringName) ?? throw new NullReferenceException($"{_defaultConnectionStringName} missing");

            builder.Services.RegisterDataContext(connectionString);
            builder.Services.RegisterEmployeeServices();
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

            // Require authorization globally for all controllers by adding an AuthorizeFilter
            builder.Services.AddControllers(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.Filters.Add(new AuthorizeFilter(policy));
            });

            builder.Services.AddHealthChecks();

            builder.Services.AddDataProtection()
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });
            builder.Services.AddSwaggerGen(builder.Environment.ApplicationName, version);


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(builder.Environment.ApplicationName, version);
            }

            app.UseHttpsRedirection();

            // Ensure authentication middleware runs before authorization. Configure authentication (JWT, cookies, etc.) elsewhere.
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapHealthChecks("/health");
            app.MapControllers();
            app.Run();
        }
    }
}
