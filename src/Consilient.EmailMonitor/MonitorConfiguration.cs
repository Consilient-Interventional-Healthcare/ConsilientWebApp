using Consilient.EmailMonitor.Processors;

namespace Consilient.EmailMonitor
{
    public class MonitorConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int SearchLookbackDays { get; set; } = 7;
        public ExtractAttachmentsEmailProcessorConfiguration ExtractAttachmentsEmailProcessor { get; set; } = null!;
    }
}