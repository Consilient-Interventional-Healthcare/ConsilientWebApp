using Consilient.Api.Client;
using Consilient.Infrastructure.Logging;

namespace Consilient.WebApp.Configuration
{
    internal class ApplicationSettings
    {
        public ConsilientApiClientConfiguration ApiClient { get; set; } = null!;
        public LoggingSettings Logging { get; set; } = null!;
    }
}
