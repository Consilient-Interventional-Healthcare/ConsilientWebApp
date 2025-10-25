using Consilient.Infrastructure.Logging;

namespace Consilient.Api.Configuration
{
    internal class ApplicationSettings
    {
        public LoggingSettings Logging { get; set; } = null!;
    }
}
