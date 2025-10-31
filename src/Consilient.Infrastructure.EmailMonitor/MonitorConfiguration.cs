using Consilient.Infrastructure.EmailMonitor.Processors;

namespace Consilient.Infrastructure.EmailMonitor
{
    public class MonitorConfiguration
    {
        public string Host { get; } = string.Empty;
        public int Port { get; } = 0;
        public bool UseSsl { get; } = false;
        public string UserName { get; } = string.Empty;
        public string Password { get; } = string.Empty;
        public int SearchLookbackDays { get; } = 7;
        public ExtractAttachmentsEmailProcessorConfiguration ExtractAttachmentsEmailProcessor { get; } = null!;
    }
}