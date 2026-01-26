using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Consilient.Api.Configuration;
using Consilient.Api.Configuration.Validators;
using Consilient.Api.Hubs;
using Consilient.Api.Infra.Authentication;
using Consilient.Api.Infra.ModelBinders;
using Consilient.Api.Init;
using Consilient.Background.Workers;
using Consilient.Common.Services;
using Consilient.Constants;
using Consilient.Data;
using Consilient.Data.GraphQL;
using Consilient.Employees.Services;
using Consilient.Hospitalizations.Services;
using Consilient.Infrastructure.Injection;
using Consilient.Infrastructure.Logging;
using Consilient.Infrastructure.Logging.Configuration;
using Consilient.Insurances.Services;
using Consilient.Patients.Services;
using Consilient.Shared.Services;
using Consilient.Users.Services;
using Consilient.Users.Services.Validators;
using Consilient.Visits.Services;
using Consilient.ProviderAssignments.Services;
using Consilient.Infrastructure.Storage;
using GraphQL.Server.Ui.GraphiQL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Authorization;
using Serilog;
using Serilog.Events;
using Prometheus;
using Consilient.Common.Contracts;

namespace Consilient.Api
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            const string version = "v1";

            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile(ApplicationConstants.ConfigurationFiles.AppSettings, optional: true, reloadOnChange: true)
                .AddJsonFile(string.Format(ApplicationConstants.ConfigurationFiles.EnvironmentAppSettings, builder.Environment.EnvironmentName), optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // NEW: Add Azure App Configuration as primary source for runtime configuration
            // This is the single source of truth for all application settings and Key Vault references
            // Requires "AppConfiguration__Endpoint" app setting in App Service
            var appConfigEndpoint = builder.Configuration[$"{AppConfigurationOptions.SectionName}:Endpoint"];
            if (!string.IsNullOrEmpty(appConfigEndpoint))
            {
                try
                {
                    var credential = new DefaultAzureCredential();

                    // Map ASP.NET Core environment names to App Configuration labels
                    // Development -> dev, Production -> prod (matches Terraform var.environment)
                    var appConfigLabel = builder.Environment.EnvironmentName switch
                    {
                        "Development" => "dev",
                        "Production" => "prod",
                        _ => builder.Environment.EnvironmentName.ToLower()
                    };

                    builder.Configuration.AddAzureAppConfiguration(options =>
                    {
                        options
                            .Connect(new Uri(appConfigEndpoint), credential)
                            // Load keys with ConsilientApi: prefix (shared across all environments)
                            .Select("ConsilientApi:*", LabelFilter.Null)
                            // Load keys matching environment label (dev, prod)
                            .Select("ConsilientApi:*", appConfigLabel)
                            // Strip the ConsilientApi: prefix so keys match expected configuration paths
                            // e.g., "ConsilientApi:ApplicationSettings:..." becomes "ApplicationSettings:..."
                            .TrimKeyPrefix("ConsilientApi:")
                            // Configure Key Vault reference resolution (AAC resolves KV references at read time)
                            .ConfigureKeyVault(kv =>
                            {
                                kv.SetCredential(credential);
                            });

                        // Enable dynamic configuration refresh for production only
                        // Dev uses Free SKU and stop/start for config changes
                        if (builder.Environment.IsProduction())
                        {
                            options.ConfigureRefresh(refresh =>
                            {
                                refresh.Register("ConsilientApi:RefreshSentinel", refreshAll: true)
                                       .SetRefreshInterval(TimeSpan.FromMinutes(5));
                            });
                        }
                    });

                    Log.Information("Azure App Configuration loaded successfully from {Endpoint} with label {Label}", appConfigEndpoint, appConfigLabel);
                }
                catch (Exception ex)
                {
                    // If AAC connection fails, fall back to legacy Key Vault integration
                    Log.Warning(ex, "Failed to load Azure App Configuration, falling back to Azure Key Vault");
                    var keyVaultUrl = builder.Configuration[$"{AzureKeyVaultOptions.SectionName}:Url"];
                    if (!string.IsNullOrEmpty(keyVaultUrl))
                    {
                        var credential = new DefaultAzureCredential();
                        builder.Configuration.AddAzureKeyVault(
                            new Uri(keyVaultUrl),
                            credential,
                            new KeyVaultSecretManager());
                        Log.Information("Azure Key Vault loaded as fallback from {Url}", keyVaultUrl);
                    }
                }
            }
            else
            {
                // LEGACY: Fallback to direct Key Vault integration if AppConfiguration endpoint not configured
                // This supports deployments without App Configuration (local dev, legacy environments, etc.)
                var keyVaultUrl = builder.Configuration[$"{AzureKeyVaultOptions.SectionName}:Url"];
                if (!string.IsNullOrEmpty(keyVaultUrl))
                {
                    var credential = new DefaultAzureCredential();
                    builder.Configuration.AddAzureKeyVault(
                        new Uri(keyVaultUrl),
                        credential,
                        new KeyVaultSecretManager());
                    Log.Information("Azure Key Vault loaded directly from {Url}", keyVaultUrl);
                }
            }

            var loggingConfiguration = builder.Configuration.GetSection(ApplicationConstants.ConfigurationSections.Logging)?.Get<LoggingOptions>();
            var logger = CreateLogger(builder, loggingConfiguration);
            Log.Logger = logger;
            builder.Host.UseSerilog(logger);
            try
            {
                Log.Information("Starting {App} ({Environment})", builder.Environment.ApplicationName, builder.Environment.EnvironmentName);

                var defaultConnectionString = builder.Configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.Default) ?? throw new Exception($"Connection String missing: '{ApplicationConstants.ConnectionStrings.Default}'");
                var hangfireConnectionString = builder.Configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.Hangfire) ?? throw new Exception($"Connection String missing: '{ApplicationConstants.ConnectionStrings.Hangfire}'");
                if (loggingConfiguration != null)
                {
                    builder.Services.AddSingleton(loggingConfiguration);
                }
                // Get configuration for startup-time use (before DI is built)
                var authOptions = builder.Configuration
                    .GetSection(AuthenticationOptions.SectionName)
                    .Get<AuthenticationOptions>() ?? throw new Exception($"Configuration section missing: '{AuthenticationOptions.SectionName}'");

                // Register configuration validators
                builder.Services.AddSingleton<IValidateOptions<AuthenticationOptions>, AuthenticationOptionsValidator>();
                builder.Services.AddSingleton<IValidateOptions<ProviderAssignmentsUploadsOptions>, ProviderAssignmentsUploadsOptionsValidator>();
                builder.Services.AddSingleton<IValidateOptions<UserServiceOptions>, UserServiceOptionsValidator>();
                builder.Services.AddSingleton<IValidateOptions<TokenGeneratorOptions>, TokenGeneratorOptionsValidator>();

                // Register configuration with IOptions pattern and validation at startup
                builder.Services.AddOptions<AuthenticationOptions>()
                    .Bind(builder.Configuration.GetSection(AuthenticationOptions.SectionName))
                    .ValidateOnStart();

                builder.Services.AddOptions<ProviderAssignmentsUploadsOptions>()
                    .Bind(builder.Configuration.GetSection(ProviderAssignmentsUploadsOptions.SectionName))
                    .ValidateOnStart();

                builder.Services.AddOptions<UserServiceOptions>()
                    .Bind(builder.Configuration.GetSection(UserServiceOptions.SectionName))
                    .ValidateOnStart();

                builder.Services.AddOptions<TokenGeneratorOptions>()
                    .Bind(builder.Configuration.GetSection(TokenGeneratorOptions.SectionName))
                    .ValidateOnStart();

                // Register ICurrentUserService BEFORE DbContext (required by HospitalizationStatusChangeInterceptor)
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
                // NoOp setter for Hangfire dependency resolution (actual setting happens in BackgroundHost)
                builder.Services.AddScoped<IUserContextSetter, NoOpUserContextSetter>();

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
                builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

                var allowedOrigins = builder.Configuration.GetSection(AllowedOriginsOptions.SectionName).Get<string[]>();
                builder.Services.ConfigureCors(allowedOrigins);

                // Register redirect validation options (uses same origins as CORS for consistency)
                builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(
                    new RedirectValidationOptions { AllowedOrigins = allowedOrigins ?? [] }));

                builder.Services.ConfigureRateLimiting();

                builder.Services.ConfigureCookiePolicy();

                builder.Services.AddControllers(options =>
                {
                    if (authOptions.Enabled && (builder.Environment.IsProduction() || builder.Environment.IsDevelopment()))
                    {
                        var policy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();

                        options.Filters.Add(new AuthorizeFilter(policy));
                    }
                    // ensure our provider runs before defaults
                    options.ModelBinderProviders.Insert(0, new YyyyMmDdDateModelBinderProvider());
                }).AddNewtonsoftJson();

                builder.Services.ConfigureHealthChecks(builder.Configuration, authOptions.UserService);

                // Configure Data Protection with proper cryptographic algorithms
                // Note: In Azure App Service containers, data protection keys are ephemeral by design.
                // For applications requiring persistent data protection keys across container restarts,
                // configure blob storage via WEBSITE_DataProtectionKeysPath in App Service settings.
                builder.Services.AddDataProtection()
                    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
                    {
                        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                    });
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

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger(builder.Environment.ApplicationName, version);
                    app.UseGraphQLGraphiQL("/ui/graphiql", new GraphiQLOptions
                    {
                        GraphQLEndPoint = "/graphql",

                    });
                }

                // Apply cookie policy before authentication so cookies are always marked secure/httpOnly.
                app.UseCookiePolicy();

                // IMPORTANT: run CORS before any middleware that may issue redirects (HTTPS redirection or auth).
                // This prevents preflight (OPTIONS) being redirected which browsers disallow.
                app.UseCors(Init.ConfigureCorsServiceCollectionExtensions.DefaultCorsPolicyName); // Must be before UseAuthentication/UseAuthorization and before HTTPS redirect

                // Check if running in Azure App Service (WEBSITE_SITE_NAME is set by Azure)
                var isRunningInAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));

                if (isRunningInAzure)
                {
                    // In Azure App Service, HTTPS/TLS is handled at the platform level.
                    // The container listens on HTTP/80, and App Service manages TLS termination.
                    // HSTS headers are added but HTTPS redirection is not needed.
                    app.UseHsts();
                }
                else if (!app.Environment.IsProduction())
                {
                    // In local development, enable HTTPS redirection for testing
                    // (from http://localhost:8090 to https://localhost:8091)
                    app.UseHttpsRedirection();
                }

                // Rate limiting middleware (applies GlobalLimiter by default)
                app.UseRateLimiter();

                var authenticationEnabled = builder.Environment.IsProduction() || builder.Environment.IsDevelopment() && authOptions.Enabled;
                if (authenticationEnabled)
                {
                    app.UseAuthentication();
                    app.UseAuthorization();
                }

                app.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
                });
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
                // Section exists but couldn't bind � still fallback rather than throw.
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
}

