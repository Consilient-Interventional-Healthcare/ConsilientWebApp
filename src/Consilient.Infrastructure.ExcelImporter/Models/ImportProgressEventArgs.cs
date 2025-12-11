namespace Consilient.Infrastructure.ExcelImporter.Models
{
    public class ImportProgressEventArgs : EventArgs
    {
        public required string Stage { get; init; }
        public int ProcessedRows { get; init; }
        public int? TotalRows { get; init; }
        public string? CurrentOperation { get; init; }
        public Guid? BatchId { get; init; }
    }
}
