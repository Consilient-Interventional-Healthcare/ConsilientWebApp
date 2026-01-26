using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;

namespace Consilient.Infrastructure.Logging
{
    public static class LoggingRegistrationExtension
    {
        public static void RegisterLogging(this IServiceCollection services, ILogger logger)
        {
            services.AddSingleton(new SerilogLoggerFactory(logger));
        }

        /// <summary>
        /// Adds the Loki health check for Grafana Loki logging infrastructure.
        /// </summary>
        /// <param name="builder">The health checks builder.</param>
        /// <param name="services">The service collection for registering HttpClient.</param>
        /// <param name="timeout">Optional timeout for HTTP requests (default: 15 seconds).</param>
        /// <returns>The health checks builder for chaining.</returns>
        public static IHealthChecksBuilder AddLokiHealthCheck(
            this IHealthChecksBuilder builder,
            IServiceCollection services,
            TimeSpan? timeout = null)
        {
            services.AddHttpClient<LokiHealthCheck>(client =>
            {
                client.Timeout = timeout ?? TimeSpan.FromSeconds(15);
            });

            return builder.AddCheck<LokiHealthCheck>("loki", tags: ["infrastructure", "logging"]);
        }
    }
}
