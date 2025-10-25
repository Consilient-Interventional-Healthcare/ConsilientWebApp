namespace Consilient.Infrastructure.Logging.Configuration
{
    public class LoggingConfiguration
    {
        public LogLevelConfiguration LogLevel { get; set; } = null!;
        public GrafanaLokiConfiguration GrafanaLoki { get; set; } = null!;
    }
}
