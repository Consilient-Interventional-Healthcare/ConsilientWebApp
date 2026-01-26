namespace Consilient.LLM
{
    public class OllamaServiceOptions
    {
        public const string SectionName = "OllamaService";

        public required string BaseUrl { get; init; } = string.Empty;
        public required string DefaultModel { get; init; } = string.Empty;
    }
}
