namespace Consilient.Infrastructure.Logging
{
    public class LoggingSettings
    {
        public string LogLevelEvent { get; set; } = "Information";
        public GrafanaLokiSettings GrafanaLoki { get; set; } = null!;
    }
}