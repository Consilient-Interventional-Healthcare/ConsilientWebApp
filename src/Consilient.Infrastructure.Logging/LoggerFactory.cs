using Consilient.Infrastructure.Logging.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

namespace Consilient.Infrastructure.Logging;

public static class LoggerFactory
{
    public static ILogger Create(LoggingOptions loggingOptions, IDictionary<string, string>? labels = null)
    {
        var minimumLogLevel = ParseLogEventLevel(loggingOptions.LogLevel.Default);
        var loggerConfiguration = CreateLoggerConfiguration();

        loggerConfiguration
            .MinimumLevel.Is(minimumLogLevel)
            .MinimumLevel.Override("Microsoft.AspNetCore", ParseLogEventLevel(loggingOptions.LogLevel.MicrosoftAspNetCore));

        loggerConfiguration.WriteTo.Console();

        var lokiLabels = labels?.Select(x => new LokiLabel
        {
            Key = x.Key.ToLowerInvariant(),
            Value = x.Value.ToLowerInvariant()
        });

        // Configure Loki credentials if provided
        LokiCredentials? lokiCredentials = null;
        if (!string.IsNullOrEmpty(loggingOptions.GrafanaLoki.Username) &&
            !string.IsNullOrEmpty(loggingOptions.GrafanaLoki.Password))
        {
            lokiCredentials = new LokiCredentials
            {
                Login = loggingOptions.GrafanaLoki.Username,
                Password = loggingOptions.GrafanaLoki.Password
            };
        }

        loggerConfiguration.WriteTo.GrafanaLoki(
            uri: loggingOptions.GrafanaLoki.Url,
            labels: lokiLabels,
            propertiesAsLabels: null,
            credentials: lokiCredentials,
            tenant: null,
            restrictedToMinimumLevel: minimumLogLevel,
            batchPostingLimit: loggingOptions.GrafanaLoki.BatchPostingLimit,
            queueLimit: null,
            period: null,
            textFormatter: new LokiJsonTextFormatter()
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