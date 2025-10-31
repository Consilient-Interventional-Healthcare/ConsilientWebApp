namespace Consilient.Infrastructure.Logging.Configuration
{
    public class GrafanaLokiConfiguration
    {
        public string Url { get; } = string.Empty;
        public int BatchPostingLimit { get; } = 100;
    }
}