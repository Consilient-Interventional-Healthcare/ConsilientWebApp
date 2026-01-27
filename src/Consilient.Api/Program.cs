using Consilient.Api.Configuration;
using Consilient.Api.Hubs;
using Consilient.Api.Init;
using Consilient.Background.Workers;
using Consilient.Constants;
using Consilient.Data;
using Consilient.Data.GraphQL;
using Consilient.Employees.Services;
using Consilient.Hospitalizations.Services;
using Consilient.Infrastructure.Logging;
using Consilient.Infrastructure.Logging.Configuration;
using Consilient.Infrastructure.Storage;
using Consilient.Insurances.Services;
using Consilient.Patients.Services;
using Consilient.ProviderAssignments.Services;
using Consilient.Shared.Services;
using Consilient.Users.Services;
using Consilient.Visits.Services;
using GraphQL.Server.Ui.GraphiQL;
using Microsoft.Extensions.Options;
using Serilog;

namespace Consilient.Api;

internal static class Program
{
    public static void Main(string[] args)
    {
        const string version = "v1";  

        var builder = WebApplication.CreateBuilder(args);

        // Configuration loading
        builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile(ApplicationConstants.ConfigurationFiles.AppSettings, optional: true, reloadOnChange: true)
            .AddJsonFile(string.Format(ApplicationConstants.ConfigurationFiles.EnvironmentAppSettings, builder.Environment.EnvironmentName), optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddApiAzureAppConfiguration(builder.Environment.EnvironmentName, builder.Environment.IsProduction());

        var loggingConfiguration = builder.Configuration.GetSection(ApplicationConstants.ConfigurationSections.Logging)?.Get<LoggingOptions>();
        var logger = CreateLogger(builder, loggingConfiguration);
        Log.Logger = logger;
        builder.Host.UseSerilog(logger);

        try
        {
            Log.Information("Starting {App} ({Environment})", builder.Environment.ApplicationName, builder.Environment.EnvironmentName);

            var defaultConnectionString = builder.Configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.Default)
                ?? throw new Exception($"Connection String missing: '{ApplicationConstants.ConnectionStrings.Default}'");
            var hangfireConnectionString = builder.Configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.Hangfire)
                ?? throw new Exception($"Connection String missing: '{ApplicationConstants.ConnectionStrings.Hangfire}'");

            if (loggingConfiguration != null)
            {
                builder.Services.AddSingleton(loggingConfiguration);
            }

            // Get configuration for startup-time use (before DI is built)
            var authOptions = builder.Configuration
                .GetSection(AuthenticationOptions.SectionName)
                .Get<AuthenticationOptions>() ?? throw new Exception($"Configuration section missing: '{AuthenticationOptions.SectionName}'");

            // Configure cross-cutting concerns via Init extensions
            builder.Services.ConfigureApplicationOptions(builder.Configuration);
            builder.Services.ConfigureUserContext();

            // Register domain services
            builder.Services.RegisterCosilientDbContext(defaultConnectionString, builder.Environment.IsProduction());
            builder.Services.RegisterUserDbContext(defaultConnectionString, builder.Environment.IsProduction());
            builder.Services.RegisterGraphQlServices();
            builder.Services.RegisterEmployeeServices();
            builder.Services.RegisterInsuranceServices();
            builder.Services.RegisterPatientServices();
            builder.Services.RegisterSharedServices();
            builder.Services.RegisterUserServices(
                authOptions.PasswordPolicy,
                useDistributedCache: builder.Environment.IsProduction());
            builder.Services.RegisterVisitServices();
            builder.Services.RegisterHospitalizationServices();
            builder.Services.AddProviderAssignmentsServices(builder.Configuration);
            builder.Services.AddFileStorage(builder.Configuration);
            builder.Services.RegisterLogging(logger);
            builder.Services.ConfigureHangfire(hangfireConnectionString);
            builder.Services.AddWorkers();

            var allowedOrigins = builder.Configuration.GetSection(AllowedOriginsOptions.SectionName).Get<string[]>();
            builder.Services.ConfigureCors(allowedOrigins);

            // Register redirect validation options (uses same origins as CORS for consistency)
            builder.Services.AddSingleton(Options.Create(
                new RedirectValidationOptions { AllowedOrigins = allowedOrigins ?? [] }));

            builder.Services.ConfigureRateLimiting();
            builder.Services.ConfigureCookiePolicy();
            builder.Services.ConfigureControllers(authOptions, builder.Environment);
            builder.Services.ConfigureHealthChecks(builder.Configuration, authOptions.UserService);
            builder.Services.ConfigureDataProtection();
            builder.Services.AddSwaggerGen(builder.Environment.ApplicationName, version);
            builder.Services.AddSignalR();
            builder.ConfigureAuthentication(authOptions.UserService.Jwt);

            var prometheusOptions = builder.Configuration
                .GetSection(PrometheusOptions.SectionName)
                .Get<PrometheusOptions>() ?? new PrometheusOptions();

            var app = builder.Build();

            // Prometheus metrics endpoint (behind feature flag)
            app.UsePrometheusMetrics(prometheusOptions);

            // Serilog request logging with enrichment (always enabled for endpoint analytics)
            app.UseSerilogRequestLoggingWithEnrichment();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(builder.Environment.ApplicationName, version);
                app.UseGraphQLGraphiQL("/ui/graphiql", new GraphiQLOptions
                {
                    GraphQLEndPoint = "/graphql",
                });
            }

            // Apply cookie policy before authentication so cookies are always marked secure/httpOnly
            app.UseCookiePolicy();

            // IMPORTANT: run CORS before any middleware that may issue redirects (HTTPS redirection or auth)
            // This prevents preflight (OPTIONS) being redirected which browsers disallow
            app.UseCors(ConfigureCorsServiceCollectionExtensions.DefaultCorsPolicyName);

            // Configure HTTPS/HSTS based on environment
            app.UseHttpsMiddleware(app.Environment);

            // Rate limiting middleware (applies GlobalLimiter by default)
            app.UseRateLimiter();

            // Authentication and authorization
            var authenticationEnabled = builder.Environment.IsProduction() || builder.Environment.IsDevelopment() && authOptions.Enabled;
            if (authenticationEnabled)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            // Map endpoints
            app.MapHealthCheckEndpoint();
            app.MapControllers();
            app.MapHub<ProgressHub>("/hubs/import-progress");

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

    private static Serilog.ILogger CreateLogger(WebApplicationBuilder builder, LoggingOptions? loggingConfiguration)
    {
        if (loggingConfiguration == null)
        {
            // Section exists but couldn't bind – still fallback rather than throw.
            return CreateTrivialLogger(builder);
        }

        var labels = new Dictionary<string, string>
        {
            { LabelConstants.App, builder.Environment.ApplicationName },
            { LabelConstants.Env, builder.Environment.EnvironmentName.ToLower() }
        };
        var logger = Infrastructure.Logging.LoggerFactory.Create(loggingConfiguration, labels);
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
