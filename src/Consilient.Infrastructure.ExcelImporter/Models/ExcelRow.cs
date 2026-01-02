namespace Consilient.Infrastructure.ExcelImporter.Models
{
    public record ExcelRow(int RowNumber, IReadOnlyDictionary<string, string> Cells);
}
