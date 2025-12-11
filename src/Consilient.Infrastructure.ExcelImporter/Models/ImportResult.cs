namespace Consilient.Infrastructure.ExcelImporter.Models
{
    public record ImportResult
    {
        public required int TotalRowsRead { get; init; }
        public required int TotalRowsWritten { get; init; }
        public required int TotalRowsSkipped { get; init; }
        public required TimeSpan Duration { get; init; }
        public List<ValidationError> ValidationErrors { get; init; } = [];
        public bool HasErrors => ValidationErrors.Any(e => e.Severity == ValidationSeverity.Error);
    }
}
