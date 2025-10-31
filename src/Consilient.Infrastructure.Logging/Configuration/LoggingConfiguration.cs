namespace Consilient.Infrastructure.Logging.Configuration
{
    public class LoggingConfiguration
    {
        public LogLevelConfiguration LogLevel { get; init; } = null!;
        public GrafanaLokiConfiguration GrafanaLoki { get; init; } = null!;
    }
}
