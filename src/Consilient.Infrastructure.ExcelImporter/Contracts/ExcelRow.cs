namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    public record ExcelRow(int RowNumber, IReadOnlyDictionary<string, string> Cells);
}
