using Consilient.Api.Hubs;
using Consilient.Api.Infra.ModelBinders;
using Consilient.Api.Init;
using Consilient.Constants;
using Consilient.Data;
using Consilient.Data.GraphQL;
using Consilient.Employees.Services;
using Consilient.Infrastructure.Logging;
using Consilient.Infrastructure.Logging.Configuration;
using Consilient.Insurances.Services;
using Consilient.Patients.Services;
using Consilient.Shared.Services;
using Consilient.Visits.Services;
using GraphQL.Server.Ui.GraphiQL;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Serilog;

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
                .AddEnvironmentVariables();

            var loggingConfiguration = builder.Configuration.GetSection(ApplicationConstants.ConfigurationSections.Logging)?.Get<LoggingConfiguration>();
            var logger = CreateLogger(builder, loggingConfiguration);
            Log.Logger = logger;
            try
            {
                Log.Information("Starting {App} ({Environment})", builder.Environment.ApplicationName, builder.Environment.EnvironmentName);

                var defaultConnectionString = builder.Configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.Default);
                var hangfireConnectionString = builder.Configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.Hangfire);
                if (loggingConfiguration != null)
                {
                    builder.Services.AddSingleton(loggingConfiguration);
                }
                //var applicationSettings = builder.Services.RegisterApplicationSettings<ApplicationSettings>(builder.Configuration);
                if (!string.IsNullOrEmpty(defaultConnectionString))
                {
                    builder.Services.RegisterDataContext(defaultConnectionString, builder.Environment.IsProduction());
                }
                builder.Services.RegisterGraphQlServices();
                builder.Services.RegisterEmployeeServices();
                builder.Services.RegisterInsuranceServices();
                builder.Services.RegisterPatientServices();
                builder.Services.RegisterSharedServices();
                builder.Services.RegisterVisitServices();
                builder.Services.RegisterLogging(logger);
                builder.Services.RegisterHangfire(hangfireConnectionString);

                // ===== Configure CORS =====
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        policy.WithOrigins(
                                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                                ?? ["http://localhost:3000", "http://localhost:5173"])
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials(); // Required for SignalR
                    });
                });

                // ===== Configure Authentication =====
                // Example: JWT Bearer. Adjust to your auth provider/settings in appsettings.json (Jwt:Authority, Jwt:Audience).
                //builder.Services.AddAuthentication(options =>
                //{
                //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                //})
                //.AddJwtBearer(options =>
                //{
                //    // Use Authority/Audience when using an identity provider (e.g., IdentityServer, Auth0, Azure AD)
                //    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                //    options.SaveToken = true;
                //    options.Authority = builder.Configuration["Jwt:Authority"];
                //    options.Audience = builder.Configuration["Jwt:Audience"];

                //    // If not using an Authority, configure TokenValidationParameters (IssuerSigningKey, etc.)
                //    options.TokenValidationParameters = new TokenValidationParameters
                //    {
                //        ValidateIssuer = !string.IsNullOrEmpty(builder.Configuration["Jwt:Authority"]),
                //        ValidateAudience = !string.IsNullOrEmpty(builder.Configuration["Jwt:Audience"]),
                //        ValidateLifetime = true
                //    };
                //});

                // Require authorization globally for all controllers by adding an AuthorizeFilter
                builder.Services.AddControllers(options =>
                {
                    //var policy = new AuthorizationPolicyBuilder()
                    //    .RequireAuthenticatedUser()
                    //    .Build();

                    //options.Filters.Add(new AuthorizeFilter(policy));

                    // ensure our provider runs before defaults
                    options.ModelBinderProviders.Insert(0, new YyyyMmDdDateModelBinderProvider());
                }).AddNewtonsoftJson();

                builder.Services.AddHealthChecks().RegisterHealthChecks();

                builder.Services.AddDataProtection()
                    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
                    {
                        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                    });
                builder.Services.AddSwaggerGen(builder.Environment.ApplicationName, version);
                builder.Services.AddSignalR();


                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger(builder.Environment.ApplicationName, version);
                    app.UseGraphQLGraphiQL("/ui/graphiql", new GraphiQLOptions
                    {
                        GraphQLEndPoint = "/graphql",

                    });
                }

                if (app.Environment.IsProduction())
                {
                    app.UseHttpsRedirection();
                    app.UseHsts();
                }

                app.UseCors(); // Must be before UseAuthentication/UseAuthorization

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapHealthChecks("/health");
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

        private static Serilog.ILogger CreateLogger(WebApplicationBuilder builder, LoggingConfiguration? loggingConfiguration)
        {
            if (loggingConfiguration == null)
            {
                // Section exists but couldn't bind — still fallback rather than throw.
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

