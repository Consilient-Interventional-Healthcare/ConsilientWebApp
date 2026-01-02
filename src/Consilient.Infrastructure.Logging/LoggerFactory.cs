using Consilient.Infrastructure.Logging.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

namespace Consilient.Infrastructure.Logging
{
    public static class LoggerFactory
    {
        public static ILogger Create(LoggingConfiguration loggingSettings, IDictionary<string, string>? labels = null)
        {
            var minimumLogLevel = ParseLogEventLevel(loggingSettings.LogLevel.Default);
            var loggerConfiguration = CreateLoggerConfiguration();

            loggerConfiguration
                .MinimumLevel.Is(minimumLogLevel)
                .MinimumLevel.Override("Microsoft.AspNetCore", ParseLogEventLevel(loggingSettings.LogLevel.MicrosoftAspNetCore));

            loggerConfiguration.WriteTo.Console();

            var lokiLabels = labels?.Select(x => new LokiLabel
            {
                Key = x.Key.ToLowerInvariant(),
                Value = x.Value.ToLowerInvariant()
            });

            loggerConfiguration.WriteTo.GrafanaLoki(
                loggingSettings.GrafanaLoki.Url,
                lokiLabels,
                null,
                null,
                null,
                minimumLogLevel,
                loggingSettings.GrafanaLoki.BatchPostingLimit,
                null,
                null,
                new LokiJsonTextFormatter()
            );

            var logger = loggerConfiguration.CreateLogger();
            return logger;
        }

        private static LoggerConfiguration CreateLoggerConfiguration()
        {
            var defaultLoggerConfiguration = new LoggerConfiguration();
            defaultLoggerConfiguration.MinimumLevel.Verbose();
            defaultLoggerConfiguration.Enrich.FromLogContext();
            // defaultLoggerConfiguration.Enrich.With<ActivityEnricher>();
            // configure?.Invoke(defaultLoggerConfiguration);
            return defaultLoggerConfiguration;
        }

        private static LogEventLevel ParseLogEventLevel(string input)
        {
            return Enum.Parse<LogEventLevel>(input, ignoreCase: true);
        }
    }
}