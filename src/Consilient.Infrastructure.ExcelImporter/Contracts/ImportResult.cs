namespace Consilient.Infrastructure.ExcelImporter.Contracts;

public record ImportResult
{
    public required int TotalRowsRead { get; init; }
    public required int TotalRowsWritten { get; init; }
    public required int TotalRowsSkipped { get; init; }
    public int TotalRowsWithErrors { get; init; }
    public required TimeSpan Duration { get; init; }
    public List<ValidationError> ValidationErrors { get; init; } = [];
    public required Guid BatchId { get; init; }
}
