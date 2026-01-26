namespace Consilient.Infrastructure.Logging.Configuration
{
    public class LoggingOptions
    {
        public const string SectionName = "Logging";

        public LogLevelOptions LogLevel { get; init; } = null!;
        public GrafanaLokiOptions GrafanaLoki { get; init; } = null!;
    }
}
