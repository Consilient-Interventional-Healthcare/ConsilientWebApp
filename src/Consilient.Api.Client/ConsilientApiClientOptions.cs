namespace Consilient.Api.Client
{
    public class ConsilientApiClientOptions
    {
        public const string SectionName = "ApiClient";

        public required string BaseUrl { get; init; } = string.Empty;
    }
}
