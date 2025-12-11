namespace Consilient.Infrastructure.ExcelImporter.Models
{
    public record ImportProgress(
        string Stage,
        int ProcessedRows,
        int? TotalRows = null,
        string? CurrentOperation = null);
}
