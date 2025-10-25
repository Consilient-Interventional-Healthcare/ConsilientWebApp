using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.Grafana.Loki;

namespace Consilient.Infrastructure.Logging
{
    public static class LoggingRegistrationExtension
    {
        public static void RegisterLogging(this IServiceCollection services, LoggingSettings loggingSettings)
        {
            var loggingLevel = Helpers.ParseLogEventLevel(loggingSettings.LogLevelEvent);
            var loggerConfiguration = CreateLoggerConfiguration();
            loggerConfiguration.WriteTo.Console(loggingLevel);
            var labels = loggingSettings.GrafanaLoki.Labels?.Select((x) => new LokiLabel
            {
                Key = x.Key.ToLower(),
                Value = x.Value.ToLower()
            });
            loggerConfiguration.WriteTo.GrafanaLoki(loggingSettings.GrafanaLoki.Url, labels, null, null, null, loggingLevel, loggingSettings.GrafanaLoki.BatchPostingLimit, null, null, new LokiJsonTextFormatter(), null, null, false);
            var logger = loggerConfiguration.CreateLogger();
            services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(logger));
        }

        private static LoggerConfiguration CreateLoggerConfiguration()
        {
            var defaultLoggerConfiguration = new LoggerConfiguration();
            defaultLoggerConfiguration.MinimumLevel.Verbose();
            defaultLoggerConfiguration.Enrich.FromLogContext();
            //defaultLoggerConfiguration.Enrich.With<ActivityEnricher>();
            //configure?.Invoke(defaultLoggerConfiguration);
            return defaultLoggerConfiguration;
        }
    }
}
