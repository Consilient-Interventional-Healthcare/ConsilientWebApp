using Consilient.Infrastructure.EmailMonitor.Processors;

namespace Consilient.Infrastructure.EmailMonitor
{
    public class MonitorConfiguration
    {
        public required string Host { get; init; } = string.Empty;
        public required int Port { get; init; } = 0;
        public required bool UseSsl { get; init; } = false;
        public required string UserName { get; init; } = string.Empty;
        public required string Password { get; init; } = string.Empty;
        public required int SearchLookbackDays { get; init; } = 7;
        public required ExtractAttachmentsEmailProcessorConfiguration ExtractAttachmentsEmailProcessor { get; init; } = null!;
    }
}