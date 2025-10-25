namespace Consilient.Infrastructure.Logging
{
    public class GrafanaLokiSettings
    {
        public string Url { get; set; } = string.Empty;
        public int BatchPostingLimit { get; set; } = 100;
        public IDictionary<string, string>? Labels { get; set; }
    }
}