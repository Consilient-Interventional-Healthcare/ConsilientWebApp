namespace Consilient.Infrastructure.EmailMonitor.Processors
{
    public class ExtractAttachmentsEmailProcessorConfiguration
    {
        public required string SavePath { get; init; } = string.Empty;
        public required IEnumerable<string> ExtensionFilter { get; init; } = [];
        public required IEnumerable<string> ContentTypeFilter { get; init; } = [];
    }
}
