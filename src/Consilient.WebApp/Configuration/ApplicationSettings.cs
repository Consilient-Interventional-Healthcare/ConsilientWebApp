using Consilient.Api.Client;

namespace Consilient.WebApp.Configuration
{
    internal class ApplicationSettings
    {
        public required ConsilientApiClientConfiguration ApiClient { get; init; } = null!;
    }
}
