using System.Data;

namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    public sealed class SheetReadResult
    {
        public Dictionary<string, int>? ColumnMap { get; init; }
        public DataTable DataTable { get; init; } = new DataTable();
    }
}