using Consilient.BackgroundHost.Config;
using Consilient.BackgroundHost.Infra.Security;
using Hangfire;
using Hangfire.SqlServer; // Add this using directive at the top
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Consilient.BackgroundHost
{
    public partial class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup()
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment ?? string.Empty}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static void Configure(IApplicationBuilder appBuilder)
        {
            var applicationSettings = appBuilder.ApplicationServices.GetRequiredService<ApplicationSettings>();
            appBuilder.UseHangfireDashboard(string.Empty, new DashboardOptions
            {
                DashboardTitle = $"AIS ({applicationSettings.Environment.ToUpper()})",
                Authorization = [new MyAuthorizationFilter()]
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            /* CONFIGURE DI */
            var currentAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName();
            var connectionString = Configuration.GetConnectionString("DefaultConnection") ?? throw new NullReferenceException("connectionString");

            //var applicationSettings = new ApplicationSettings();
            //Configuration.GetSection("ApplicationSettings").Bind(applicationSettings);
            //services.AddSingleton(applicationSettings);

            //var appId = GetApplicationId(AppDomain.CurrentDomain.FriendlyName);
            //var loggingPathToRoot = AppDomain.CurrentDomain.BaseDirectory;
            //if (AppContext.BaseDirectory.Contains("bin", StringComparison.CurrentCulture))
            //{
            //    loggingPathToRoot = AppContext.BaseDirectory[..AppContext.BaseDirectory.IndexOf("bin")];
            //}
            //var loggingDirectory = LoggingHelpers.GetLogDirectory(applicationSettings.Logging.FileSystem.Directory, loggingPathToRoot);
            //AddLogging(services, applicationSettings, loggingDirectory, appId);

            //services.AddEmail(applicationSettings.Email, loggingPathToRoot, applicationSettings.Environment, applicationSettings.IsProduction);


            //services.RegisterDataContext(connectionString, currentAssembly.Name, appId, applicationSettings.IsProduction);

            //services.AddIdentity<ThisUser, ThisRole>(o =>
            //{
            //    o.Password.RequireDigit = applicationSettings.PasswordPolicy.RequireDigit;
            //    o.Password.RequiredLength = applicationSettings.PasswordPolicy.RequiredLength;
            //    o.Password.RequiredUniqueChars = applicationSettings.PasswordPolicy.RequiredUniqueChars;
            //    o.Password.RequireLowercase = applicationSettings.PasswordPolicy.RequireLowercase;
            //    o.Password.RequireNonAlphanumeric = applicationSettings.PasswordPolicy.RequireNonAlphanumeric;
            //    o.Password.RequireUppercase = applicationSettings.PasswordPolicy.RequireUppercase;
            //})
            //    .AddEntityFrameworkStores<ThisIdentityDbContext>()
            //    .AddDefaultTokenProviders();

            //services.RegisterUserService(
            //    connectionString,
            //    new UserServiceConfiguration
            //    {
            //        EmailSendingEnabled = false,
            //        PrioritizeManageUsersWhenLinking = false,
            //    },
            //    applicationSettings.PasswordPolicy
            //);


            //// Configure MessageBus
            //if (applicationSettings.MessageBus.Enabled)
            //{
            //    services.RegisterMessageBusConsumers(applicationSettings.MessageBus.RabbitMQ, connectionString);
            //    services.AddHostedService<MessageBusBackgroundService>();
            //}

            // Configure Hangfire
            var hangfireConnectionString = Configuration.GetConnectionString("HangfireConnection") ?? throw new Exception("HangfireConnection missing");
            services.AddSingleton<IBackgroundJobClient, BackgroundJobClient>();
            services.AddSingleton<IRecurringJobManager, RecurringJobManager>();
            services.AddHangfire((provider, config) =>
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
            }); services.AddHangfireServer((provider, options) =>
            {
                options.ShutdownTimeout = TimeSpan.FromMinutes(30);
                options.WorkerCount = Math.Max(Environment.ProcessorCount, 20);
                options.Queues = ["default", "startup"];
            });
        }


        #region Private Helpers
        //private static void AddLogging(IServiceCollection services, ApplicationSettings applicationSettings, string loggingDirectory, string appId)
        //{
        //    var defaultLogger = LoggerProvider.CreateLogger(applicationSettings, loggingDirectory, appId);
        //    services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(defaultLogger));
        //}

        [GeneratedRegex(@".exe$")]
        private static partial Regex GetApplication();
        private static string GetApplicationId(string name) => GetApplication().Replace(name, string.Empty);
        #endregion
    }


}
