using Consilient.Infrastructure.EmailMonitor;

namespace Consilient.BackgroundHost.Configuration
{
    public class EmailSettings
    {
        public MonitorConfiguration Monitor { get; set; } = null!;
    }
}