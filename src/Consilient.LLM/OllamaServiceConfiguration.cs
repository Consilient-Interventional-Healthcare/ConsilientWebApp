namespace Consilient.LLM
{
    public class OllamaServiceConfiguration
    {
        public required string BaseUrl { get; init; } = string.Empty;
        public required string DefaultModel { get; init; } = string.Empty;
    }
}
