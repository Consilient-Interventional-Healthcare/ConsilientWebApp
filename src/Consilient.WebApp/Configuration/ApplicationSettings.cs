using Consilient.Api.Client;

namespace Consilient.WebApp.Configuration
{
    internal class ApplicationSettings
    {
        public ConsilientApiClientConfiguration ApiClient { get; } = null!;
    }
}
