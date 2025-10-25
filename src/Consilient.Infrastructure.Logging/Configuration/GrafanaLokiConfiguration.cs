namespace Consilient.Infrastructure.Logging.Configuration
{
    public class GrafanaLokiConfiguration
    {
        public string Url { get; set; } = string.Empty;
        public int BatchPostingLimit { get; set; } = 100;
    }
}