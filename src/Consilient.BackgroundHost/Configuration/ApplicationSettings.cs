using Consilient.Infrastructure.Logging;

namespace Consilient.BackgroundHost.Configuration
{
    internal class ApplicationSettings
    {
        public EmailSettings Email { get; set; } = null!;
        public LoggingSettings Logging { get; set; } = null!;
    }
}
