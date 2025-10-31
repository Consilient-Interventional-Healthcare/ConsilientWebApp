namespace Consilient.Infrastructure.Logging.Configuration
{
    public class GrafanaLokiConfiguration
    {
        public required string Url { get; init; } = string.Empty;
        public required int BatchPostingLimit { get; init; } = 100;
    }
}