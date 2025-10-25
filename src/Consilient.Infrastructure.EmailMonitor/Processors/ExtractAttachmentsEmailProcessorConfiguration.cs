namespace Consilient.Infrastructure.EmailMonitor.Processors
{
    public class ExtractAttachmentsEmailProcessorConfiguration
    {
        public string SavePath { get; set; } = string.Empty;
        public IEnumerable<string> ExtensionFilter { get; set; } = [];
        public IEnumerable<string> ContentTypeFilter { get; set; } = [];
    }
}
