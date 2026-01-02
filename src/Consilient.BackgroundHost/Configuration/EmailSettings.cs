using Consilient.Infrastructure.EmailMonitor;

namespace Consilient.BackgroundHost.Configuration
{
    public class EmailSettings
    {
        public required MonitorConfiguration Monitor { get; init; } = null!;
    }
}